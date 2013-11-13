using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Text;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Interfaces;

namespace MovieRatingCalculator.DataAccess.Repository
{
    public class MovieRepository : IMovieRepository
    {
        public void Add(Movie movie)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var movieParticipants = new List<MovieParticipant>();
                foreach (var productionParticipant in movie.MovieProductionParticipants)
                {
                    if (!movieParticipants.Any(pr => pr.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name))
                    {
                        var participant = context.MovieParticipants.SingleOrDefault(
                            p => p.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name);
                        if (participant == null)
                        {
                            movieParticipants.Add(productionParticipant.MovieParticipantType.MovieParticipant);
                            context.MovieParticipants.Add(productionParticipant.MovieParticipantType.MovieParticipant);
                        }
                        else
                        {
                            movieParticipants.Add(participant);
                        }
                    }
                }

                foreach (var productionParticipant in movie.MovieProductionParticipants)
                {
                    var movieParticipant = context.MovieParticipantTypes.FirstOrDefault(
                        pr => pr.MovieParticipant.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name &&
                              pr.ParticipantTypeId == productionParticipant.MovieParticipantType.ParticipantTypeId);

                    if (movieParticipant == null)
                    {
                        productionParticipant.MovieParticipantType.MovieParticipant = movieParticipants.Single(
                                pr => pr.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name);

                        context.MovieParticipantTypes.Add(productionParticipant.MovieParticipantType);
                    }
                    else
                    {
                        productionParticipant.MovieParticipantType = movieParticipant;
                    }
                }

                var movieCountries = new List<Country>();
                foreach (var country in movie.Countries)
                {
                    var existingCountry = context.Countries.SingleOrDefault(c => c.Name == country.Name);
                    if (existingCountry == null)
                    {
                        context.Countries.Add(country);
                        movieCountries.Add(country);
                    }
                    else
                    {
                        movieCountries.Add(existingCountry);
                    }
                }
                movie.Countries = movieCountries;

                var movieGenres = new List<Genre>();
                foreach (var genre in movie.Genres)
                {
                    var existingGenre = context.Genres.SingleOrDefault(g => g.Name == genre.Name);
                    if (existingGenre == null)
                    {
                        context.Genres.Add(genre);
                        movieGenres.Add(genre);
                    }
                    else
                    {
                        movieGenres.Add(existingGenre);
                    }
                }
                movie.Genres = movieGenres;

                context.Movies.Add(movie);
                context.SaveChanges();
            }
        }

        public List<Movie> FindMovies(string searchString, short? releaseYear, string email, int page, int pageSize, ref int moviesCount)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                if (searchString != null || releaseYear.HasValue)
                {
                    IQueryable<Movie> movieQuery = context.Movies;
                    
                    if(!string.IsNullOrWhiteSpace(searchString))
                    {
                        searchString = searchString.Trim().ToLower();
                        searchString = System.Text.RegularExpressions.Regex.Replace(searchString, @"\s+", " ");
                        movieQuery = movieQuery.Where(
                                m =>
                                m.Name.ToLower().Contains(searchString) ||
                                m.OriginalName.ToLower().Contains(searchString));
                    }

                    if(releaseYear.HasValue)
                    {
                        movieQuery = movieQuery.Where(m => m.ReleaseYear == releaseYear);
                    }

                    moviesCount = movieQuery.Count();

                    var movies = movieQuery.
                        Include(m => m.Genres).
                        Include(m => m.Countries).
                        Include(m => m.MovieRatings.Select(mr => mr.User)).
                        Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.ParticipantType)).
                        Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.MovieParticipant)).
                        OrderByDescending(m => m.ReleaseYear).
                        ThenByDescending(m => m.KinopoiskMovieRating).
                        ThenBy(m => m.OriginalName).
                        Skip((page - 1) * pageSize).
                        Take(pageSize).
                        ToList();

                    foreach (var movie in movies)
                    {
                        movie.MovieRatings = movie.MovieRatings.Where(mr => mr.User.Email == email).ToList();
                    }
                  
                    return movies;
                }

                moviesCount = 0;
                return new List<Movie>();
            }
        }

        public List<Movie> FindSuggestedMovies(string email, int page, int pageSize, 
            ref int moviesCount, short? releaseYear = null)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                IQueryable<MovieRating> movieCountQuery = context.MovieRatings;
                IQueryable<MovieRating> query = context.MovieRatings;

                if (releaseYear.HasValue)
                {
                    query = query.Where(m => m.Movie.ReleaseYear == releaseYear);
                    movieCountQuery = movieCountQuery.Where(m => m.Movie.ReleaseYear == releaseYear);
                }
                moviesCount = movieCountQuery.GroupBy(mr => mr.Movie).Count();
                
                var movies = query.
                    GroupBy(mr => mr.Movie).
                    OrderByDescending(mr => mr.Count(rate => rate.User.Email != email)).
                    ThenBy(m => m.Key.OriginalName).
                    Select(m => m.Key).
                    Include(m => m.Genres).
                    Include(m => m.Countries).
                    Include(m => m.MovieRatings.Select(mr => mr.User)).
                    Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.ParticipantType)).
                    Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.MovieParticipant)).
                    Skip((page - 1) * pageSize).
                    Take(pageSize).
                    ToList();

                foreach (var movie in movies)
                {
                    movie.MovieRatings = movie.MovieRatings.Where(mr => mr.User.Email == email).ToList();
                }

                return movies;
            }
        }

        public List<DtoRatedMovie> GetUserRatedMovies(string email, short? releaseYear = null)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                IQueryable<Movie> movieQuery = context.MovieRatings.
                                               Where(mr => mr.User.Email == email).
                                               Select(mr => mr.Movie);

                if (releaseYear.HasValue)
                {
                    movieQuery = movieQuery.Where(m => m.ReleaseYear == releaseYear);
                }
                var movies = movieQuery.
                             Select(m => new DtoRatedMovie()
                                    {
                                        Id = m.Id,
                                        Name = m.Name,
                                        OriginalName = m.OriginalName,
                                        ReleaseYear = m.ReleaseYear,
                                        TotalRatesNumber = m.MovieRatings.Count,
                                        UsersWithSameRating = m.MovieRatings.Count(
                                            r =>
                                            r.Rating ==
                                            m.MovieRatings.FirstOrDefault(mr => mr.User.Email == email).Rating) - 1,
                                        UsersRate = m.MovieRatings.FirstOrDefault(mr => mr.User.Email == email).Rating
                                    }).
                    ToList();

                return movies;
            }                       
        }

        private void SaveMovieRating(MovieRating movieRate, MovieRatingCalculatorEntities context)
        {
            if (context == null)
            {
                return;
            }

            var existingRating =
                context.MovieRatings.FirstOrDefault(
                    rate => rate.MovieId == movieRate.MovieId && rate.UserId == movieRate.UserId);

            if (existingRating != null)
            {
                existingRating.Rating = movieRate.Rating;
                existingRating.UpdateDate = movieRate.UpdateDate;
                context.Entry(existingRating).State = EntityState.Modified;
            }
            else
            {
                context.MovieRatings.Add(movieRate);
            }

            context.SaveChanges();
        }

        public void ChangeMovieRating(MovieRating movieRate)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                SaveMovieRating(movieRate, context);
            }
        }

        public DtoRatedMovie UpdateRatedMovieInfo(MovieRating movieRate)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                SaveMovieRating(movieRate, context);

                var updateInfo = context.Movies.
                    Where(m => m.Id == movieRate.MovieId).
                    Select(m => new DtoRatedMovie()
                                    {
                                        Id = m.Id,
                                        TotalRatesNumber = m.MovieRatings.Count,
                                        UsersWithSameRating = m.MovieRatings.Count(
                                            r =>
                                            r.Rating ==
                                            m.MovieRatings.FirstOrDefault(mr => mr.User.Id == movieRate.UserId).Rating) - 1,
                                        UsersRate = movieRate.Rating
                                    }).First();
                return updateInfo;
            }
        }

        public void DeleteMovieRating(MovieRating movieRate)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var existingRating =
                    context.MovieRatings.FirstOrDefault(
                        rate => rate.MovieId == movieRate.MovieId && rate.UserId == movieRate.UserId);

                if (existingRating != null)
                {
                    context.Entry(existingRating).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
        }

        public void AddMovieList(List<Movie> movieList)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var genres = new List<Genre>();
                var countries = new List<Country>();
                var participants = new List<MovieParticipant>();
                var movieParticipantTypes = new List<MovieParticipantType>();

                foreach (var movie in movieList)
                {
                    var movieGenres = new List<Genre>();
                    foreach (var genre in movie.Genres)
                    {
                        if (!genres.Any(pr => pr.Name == genre.Name))
                        {
                            var existingGenre = context.Genres.SingleOrDefault(p => p.Name == genre.Name);
                            if (existingGenre == null)
                            {
                                movieGenres.Add(genre);
                                genres.Add(genre);
                                context.Genres.Add(genre);
                            }
                            else
                            {
                                movieGenres.Add(existingGenre);
                                genres.Add(existingGenre);
                            }
                        }
                        else
                        {
                            movieGenres.Add(genres.Single(pr => pr.Name == genre.Name));
                        }
                    }
                    movie.Genres = movieGenres;

                    var movieContries = new List<Country>();
                    foreach (var country in movie.Countries)
                    {
                        if (!countries.Any(pr => pr.Name == country.Name))
                        {
                            var existingCountry = context.Countries.SingleOrDefault(p => p.Name == country.Name);
                            if (existingCountry == null)
                            {
                                movieContries.Add(country);
                                countries.Add(country);
                                context.Countries.Add(country);
                            }
                            else
                            {
                                movieContries.Add(existingCountry);
                                countries.Add(existingCountry);
                            }
                        }
                        else
                        {
                            movieContries.Add(countries.Single(pr => pr.Name == country.Name));
                        }
                    }
                    movie.Countries = movieContries;

                    foreach (var productionParticipant in movie.MovieProductionParticipants)
                    {
                        if (!participants.Any(
                            pr => pr.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name))
                        {
                            var participant = context.MovieParticipants.SingleOrDefault(
                                p => p.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name);
                            if (participant == null)
                            {
                                participants.Add(productionParticipant.MovieParticipantType.MovieParticipant);
                                context.MovieParticipants.Add(
                                    productionParticipant.MovieParticipantType.MovieParticipant);
                            }
                            else
                            {
                                participants.Add(participant);
                            }
                        }
                    }

                    var productionParticipants = new List<MovieProductionParticipant>();
                    foreach (var productionParticipant in movie.MovieProductionParticipants)
                    {
                        var exisitingParticipant = movieParticipantTypes.
                            FirstOrDefault(pr =>
                                           pr.MovieParticipant.Name == productionParticipant.MovieParticipantType.
                                                                           MovieParticipant.Name &&
                                           pr.ParticipantTypeId == productionParticipant.MovieParticipantType.
                                                                       ParticipantTypeId);
                        if (exisitingParticipant == null)
                        {
                            var movieParticipant = context.MovieParticipantTypes.Include(pr => pr.MovieParticipant).
                                FirstOrDefault(
                                    pr =>
                                    pr.MovieParticipant.Name ==
                                    productionParticipant.MovieParticipantType.MovieParticipant.Name &&
                                    pr.ParticipantTypeId == productionParticipant.MovieParticipantType.ParticipantTypeId);

                            if (movieParticipant == null)
                            {
                                productionParticipant.MovieParticipantType.MovieParticipant =
                                    participants.Single(
                                        pr =>
                                        pr.Name == productionParticipant.MovieParticipantType.MovieParticipant.Name);
                                productionParticipants.Add(productionParticipant);
                                context.MovieParticipantTypes.Add(productionParticipant.MovieParticipantType);
                            }
                            else
                            {
                                movieParticipantTypes.Add(movieParticipant);
                                productionParticipants.Add(new MovieProductionParticipant()
                                                               { MovieParticipantType = movieParticipant });
                            }
                        }
                        else
                        {
                            productionParticipants.Add(new MovieProductionParticipant()
                                                           { MovieParticipantType = exisitingParticipant });
                        }
                    }
                    movie.MovieProductionParticipants = productionParticipants;
                    context.Movies.Add(movie);
                }

                //and finally...
                
                 context.SaveChanges();
            }
        }

        public List<string> GetKinopoiskMovieIds()
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Movies.Select(m => m.KinopoiskMovieId).ToList();
            }
        }

        public List<Movie> GetMoviesWithRatings()
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Movies.
                    Where(m => m.MovieRatings.Any()).
                    Include(m => m.MovieRatings.Select(r => r.User)).
                    ToList();
            }
        }

        public Movie GetMovieStats(int movieId)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return
                    context.Movies
                           .Include(m => m.MovieRatings.Select(r => r.User))
                           .SingleOrDefault(m => m.Id == movieId);
            }
        }

        public int[] GetMovieRatingDistribution(int movieId)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                int[] ratingDistribution = new int[10];
                var groupedRatings = context.MovieRatings.Where(r => r.MovieId == movieId).GroupBy(r => r.Rating);

                for (int i = 0; i < 10; i++)
                {
                    var groupWithCurrentRating = groupedRatings.SingleOrDefault(g => g.Key == i + 1);
                    ratingDistribution[i] = groupWithCurrentRating != null ? groupWithCurrentRating.Count() : 0;
                }

                return ratingDistribution;
            }
        }

        public List<ClusterRatedItem> GetMoviesRatingsForClustering()
        {
            return GetMoviesWithUserRatings();
        }

        public List<short> GetMovieClusterIdsByMovieIds(List<int> movieIds)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Movies.Where(m => movieIds.Contains(m.Id) && m.ClusterId != null)
                                     .Select(m => (short)m.ClusterId).Distinct().ToList();
            }
        }

        public List<ClusterRatedItem> GetMoviesRatingsByClusterIds(List<short> clusterIds)
        {
            return GetMoviesWithUserRatings(clusterIds);
        }

        public List<ClusterRatedItem> GetMoviesRatingsByMovieIds(List<int> movieIds)
        {
            return GetMoviesWithUserRatings(null, movieIds);
        }

        public void UpdateClusters(List<List<ClusterRatedItem>> clusters)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var allMovies = context.Movies.Where(u => u.MovieRatings.Any())
                                            .ToList();
                short clusterId = 1;
                var clusterMovieIds = new List<int>();

                foreach (var cluster in clusters)
                {
                    var ids = cluster.Select(u => u.Id).ToList();
                    clusterMovieIds.AddRange(ids);

                    var clusterMovies = allMovies.Where(u => ids.Contains(u.Id)).ToList();

                    foreach (var clusterMovie in clusterMovies)
                    {
                        clusterMovie.ClusterId = clusterId;
                        context.Entry(clusterMovie).State = EntityState.Modified;
                    }

                    clusterId++;
                }

                var moviesWithNoCluster = allMovies.Where(u => !clusterMovieIds.Contains(u.Id)).ToList();

                foreach (var movie in moviesWithNoCluster)
                {
                    movie.ClusterId = null;
                    context.Entry(movie).State = EntityState.Modified;
                }

                context.SaveChanges();
            }
        }

        List<ClusterRatedItem> GetMoviesWithUserRatings(List<short> clusterIds = null, List<int> movieIds = null)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var allUsers = context.Users.Where(u => u.MovieRatings.Any())
                    .Select(u => new ElementRating
                    {
                        ElementId = u.Id,
                        Rating = 0
                    }).ToList();

                IQueryable<Movie> query = context.Movies.Where(m => m.MovieRatings.Any());

                if (clusterIds != null)
                {
                    query = query.Where(m => m.ClusterId != null && clusterIds.Contains((short) m.ClusterId));
                }

                if (movieIds != null)
                {
                    query = query.Where(m => movieIds.Contains(m.Id));
                }

                var movies = query.GroupJoin(context.MovieRatings, m => m.Id, mr => mr.MovieId,
                    (movie, movieRatings) => new
                    {
                        Id = movie.Id,
                        ClusterId = movie.ClusterId,
                        Ratings = movieRatings.Select(r => new { ElementId = r.UserId,
                                                                 Rating = r.Rating, })
                                                                }).ToList();

                var ratings = new List<ClusterRatedItem>();

                foreach (var movie in movies)
                {
                    var allRatings = allUsers.GroupJoin(movie.Ratings, u => u.ElementId, m => m.ElementId,
                                                       (u, m) => m.Select(mr => new ElementRating(u.ElementId, mr.Rating))
                                                                  .DefaultIfEmpty(new ElementRating(u.ElementId, 0))
                                                                  .Single()).OrderBy(r => r.ElementId).ToList();

                    ratings.Add(new ClusterRatedItem(movie.Id, allRatings, movie.ClusterId ?? 0));
                }

                return ratings;
            }
        }

        public List<Movie> GetMoviesByIds(List<int> movieIds)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Movies.Where(m => movieIds.Contains(m.Id)).
                    Include(m => m.Genres).
                    Include(m => m.Countries).
                    Include(m => m.MovieRatings.Select(mr => mr.User)).
                    Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.ParticipantType)).
                    Include(m => m.MovieProductionParticipants.Select(p => p.MovieParticipantType.MovieParticipant)).
                    ToList();
            }
        }
    }
}
