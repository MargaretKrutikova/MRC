using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Dissimilarities;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Repository;

namespace MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms
{
    public class RecommendedMovie
    {
        public int Id { get; set; }
        public double Rating { get; set; }
        
        public RecommendedMovie(double rating, int id)
        {
            Rating = rating;
            Id = id;
        }
    }

    public class MovieRecommendation
    {
        public List<RecommendedMovie> GetItemBasedTopNRecommendations(ClusterRatedItem user, 
            CalculateDistanceDelegate<ClusterRatedItem> calculateDistance,
            MovieRepository movieRepository)
        {
            List<int> movieIds = user.Ratings.Where(m => m.Rating != 0).Select(r => r.ElementId).ToList();
            List<short> clusterMovieIds = movieRepository.GetMovieClusterIdsByMovieIds(movieIds);

            //all movies, which have clusterid one of the clusterids of user rated movies
            var groupedItems = movieRepository.GetMoviesRatingsByClusterIds(clusterMovieIds);

            //
            var currentUserMovies = movieRepository.GetMoviesRatingsByMovieIds(movieIds);

            var recommendedMovies = new List<RecommendedMovie>();

            for(int i = 0; i < clusterMovieIds.Count; i++)
            {
                var userRatedMovies = currentUserMovies.Where(m => m.ClusterId == clusterMovieIds[i]).ToList();
                var groupMovieIds = userRatedMovies.Select(m => m.Id).ToList();

                var groupMovies = groupedItems.Where(m => m.ClusterId == clusterMovieIds[i] && !groupMovieIds.Contains(m.Id)).ToList();
               
                foreach (var groupMovie in groupMovies)
                {
                    double sum1 = 0, sum2 = 0;
                    foreach (var movie in userRatedMovies)
                    {
                        double similarity = ElementsDissimilarities.PearsonCoefficient(groupMovie, movie);
                        int currentRating = user.Ratings.First(m => m.ElementId == movie.Id).Rating;

                        sum1 += currentRating*similarity;
                        sum2 += Math.Abs(similarity);
                    }

                    recommendedMovies.Add(new RecommendedMovie(sum1/sum2, groupMovie.Id));
                }
            }

            return recommendedMovies.OrderByDescending(m => m.Rating).Take(20).ToList();
        }

        public List<RecommendedMovie> GetUserBasedTopNRecommendations(ClusterRatedItem user, 
            CalculateDistanceDelegate<ClusterRatedItem> calculateDistance,
            UserRepository userRepository)
        {
            var groupUsers = userRepository.GetUsersByClusterId(user.ClusterId);
            if (!groupUsers.Any())
            {
                return null;
            }

            List<int> currentUserMoviesIds = user.Ratings.Where(r => r.Rating != 0)
                                                         .Select(r => r.ElementId).ToList();

            List<ElementRating> ratings = groupUsers.SelectMany(u => u.Ratings).ToList();
            List<ElementRating> movies = ratings.Where(m => m.Rating != 0 && !currentUserMoviesIds.Contains(m.ElementId)).ToList();

            List<int> movieIds = movies.Select(m => m.ElementId).Distinct().ToList();
           
            var recommendedMovies = new List<RecommendedMovie>();
            double avgRating = user.Ratings.Where(r => r.Rating != 0).Average(r => r.Rating);

            for (int i = 0; i < movieIds.Count; i++)
            {
                var currentGroupUsers = groupUsers
                                        .Where(u => u.Ratings.First(
                                            r => r.ElementId == movieIds[i]).Rating != 0).ToList();

                double sum1 = 0, sum2 = 0;

                foreach (var currentGroupUser in currentGroupUsers)
                {
                    double similarity = calculateDistance(user, currentGroupUser),
                           avgCurrentUserRating = currentGroupUser.Ratings.Where(r => r.Rating != 0).Average(r => r.Rating),
                           currentRating = currentGroupUser.Ratings.First(r => r.ElementId == movieIds[i]).Rating;

                    sum1 += (currentRating - avgCurrentUserRating) * similarity;
                    sum2 += Math.Abs(similarity);
                }

                recommendedMovies.Add(new RecommendedMovie(avgRating + sum1/sum2, movieIds[i]));
            }

            return recommendedMovies.OrderByDescending(m => m.Rating).Take(20).ToList();
        }
    }
}
