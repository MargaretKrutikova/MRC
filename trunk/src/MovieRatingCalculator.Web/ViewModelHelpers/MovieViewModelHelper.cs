using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Enums;
using MovieRatingCalculator.DataAccess.Repository;
using MovieRatingCalculator.Web.ViewModels;
using System.Diagnostics.Contracts;

namespace MovieRatingCalculator.Web.ViewModelHelpers
{
    public class MovieViewModelHelper
    {
        public static List<MovieViewModel> PopulateMovieList(List<Movie> movieList, bool isLoggedIn = false)
        {
            Contract.Assert(movieList != null);

            var movieViewModelList = new List<MovieViewModel>();

            foreach (var movie in movieList)
            {
                movieViewModelList.Add(PopulateMovieViewModel(movie, isLoggedIn));
            }
            
            return movieViewModelList;
        }

        public static MovieViewModel PopulateMovieViewModel(Movie movie, bool isLoggedIn = false)
        {
            var movieViewModel = new MovieViewModel()
                                     {
                                         Id = movie.Id,
                                         Name = WebUtility.HtmlDecode(movie.Name),
                                         OriginalName = WebUtility.HtmlDecode(movie.OriginalName),
                                         ReleaseYear = movie.ReleaseYear,
                                         Duration = movie.Duration,
                                         KinopoiskMovieLink = String.Format("http://www.kinopoisk.ru/film/{0}", 
                                         movie.KinopoiskMovieId),
                                         KinopoiskMovieRating = movie.KinopoiskMovieRating,
                                         KinopoiskNumberUsersRate = movie.KinopoiskNumberUsersRate,
                                         Genres = movie.Genres.Select(genre => genre.Name).ToList(),
                                         Countries = movie.Countries.Select(genre => genre.Name).ToList()
                                     };
            MovieProductionParticipant director = movie.MovieProductionParticipants.FirstOrDefault(
                pr => pr.MovieParticipantType.ParticipantTypeId == (int) ParticipantTypeEnum.Director);

            movieViewModel.DirectorName = director != null ? director.MovieParticipantType.MovieParticipant.Name : "";

            List<MovieProductionParticipant> actors = movie.MovieProductionParticipants.Where(
                pr => pr.MovieParticipantType.ParticipantTypeId == (int)ParticipantTypeEnum.Actor).ToList();

            movieViewModel.Actors = actors.Any() ? actors.Select(actor => 
                actor.MovieParticipantType.MovieParticipant.Name).ToList() : null;

            if (isLoggedIn)
            {
                UpdateMovieViewModelRatings(movieViewModel, movie);
            }

            return movieViewModel;
        }

        public static void UpdateMovieViewModelRatings(MovieViewModel movieViewModel, Movie movie)
        {
            movieViewModel.MovieRateList = new List<SelectListItem>();
            movieViewModel.MovieRateList.Add(new SelectListItem { Text = "-", Value = "0" });

            for (int i = 1; i <= 10; i++)
            {
                movieViewModel.MovieRateList.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            if (movie.MovieRatings!= null && movie.MovieRatings.Any())
            {
                var selectedValue = movie.MovieRatings.Single().Rating.ToString();
                movieViewModel.MovieRateList.First(r => string.Compare(r.Value, selectedValue) == 0).Selected = true;
            }
            else
            {
                movieViewModel.MovieRateList[0].Selected = true;
            }
        }
    }
}