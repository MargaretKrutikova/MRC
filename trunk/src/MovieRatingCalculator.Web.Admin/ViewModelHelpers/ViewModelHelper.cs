using System.Collections.Generic;
using System.Linq;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.Web.Admin.ViewModels;

namespace MovieRatingCalculator.Web.Admin.ViewModelHelpers
{
    public static class ViewModelHelper
    {
        public static UserRatingsViewModel PopulateUserRatingViewModel(List<User> users)
        {
            return new UserRatingsViewModel
                       {
                           UserRatings = users.
                            Select(u => new UserRatingInfoViewModel
                                    {
                                        UserId = u.Id,
                                        Email = u.Email,
                                        Name = string.Format("{0} {1}", u.FirstName, u.LastName),
                                        NumberOfRatedMovies = u.MovieRatings.Count,
                                        NumberOfLogins = u.UserLoginHistory.Count,
                                        LastLoginDate = u.UserLoginHistory.Last().LoginDate.ToString("yyyy/MM/dd HH:mm:ss"),
                                        LastLoginIpAddress = u.UserLoginHistory.Last().IpAddress
                                    }).
                                    OrderByDescending(u => u.NumberOfRatedMovies).
                                    ToList()
                       };
        }

        public static MovieRatingsViewModel PopulateMovieRatingViewModel(List<Movie> movies)
        {
            return new MovieRatingsViewModel
                       {
                           MovieRatings = movies.
                            Select(m => new MovieRatingInfoViewModel
                                    {
                                        MovieId = m.Id,
                                        Name = m.Name,
                                        OriginalName = m.OriginalName,
                                        RatedByNumberOfUsers = m.MovieRatings.Count,
                                        AverageRating = m.MovieRatings.Average(r => r.Rating),
                                        KinopoiskLink = string.Format("http://www.kinopoisk.ru/film/{0}", m.KinopoiskMovieId),
                                    }).
                                    OrderByDescending(m => m.RatedByNumberOfUsers).
                                    ToList()
                       };
        }

        public static UserStatsViewModel PopulateUserStatsViewModel(User user)
        {
            return new UserStatsViewModel
                       {
                           Email = user.Email,
                           FirstName = user.FirstName,
                           LastName = user.LastName,
                           RatedMovies = user.
                                        MovieRatings.
                                        Select(r => new MovieRatingInfoViewModel
                                                        {
                                                            MovieId = r.Movie.Id,
                                                            Name = r.Movie.Name,
                                                            OriginalName = r.Movie.OriginalName,
                                                            UserRating = r.Rating,
                                                            KinopoiskLink = string.Format("http://www.kinopoisk.ru/film/{0}",
                                                                                            r.Movie.KinopoiskMovieId)
                                                        }).ToList()
                       };
        }

        public static MovieStatsViewModel PopulateMovieStatsViewModel(Movie movie)
        {
            return new MovieStatsViewModel
                       {
                           Id = movie.Id,
                           Name = movie.Name,
                           OriginalName = movie.OriginalName,
                           KinopoiskLink = string.Format("http://www.kinopoisk.ru/film/{0}", movie.KinopoiskMovieId),
                           UsersRated = movie.
                                        MovieRatings.
                                        Select(r => new UserRatingInfoViewModel
                                                        {
                                                            UserId = r.User.Id,
                                                            Email = r.User.Email,
                                                            Name = string.Format("{0} {1}", r.User.FirstName, r.User.LastName),
                                                            MovieRating = r.Rating
                                                        }).ToList()
                       };
        }
    }
}