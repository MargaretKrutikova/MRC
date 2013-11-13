using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.Web.ViewModels;

namespace MovieRatingCalculator.Web.ViewModelHelpers
{
    public class RatedMovieViewModelHelper
    {
        public static RatedMovieViewModel PopulateRatedMovieViewModel(DtoRatedMovie movie)
        {
            var ratingList = new List<SelectListItem>();
            ratingList.Add(new SelectListItem { Text = "-", Value = "0" });

            for (int i = 1; i <= 10; i++)
            {
                ratingList.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ratingList.First(r => string.Compare(r.Value, movie.UsersRate.ToString()) == 0).Selected = true;

            return new RatedMovieViewModel
                       {
                           Id = movie.Id,
                           Name = WebUtility.HtmlDecode(movie.Name),
                           OriginalName = WebUtility.HtmlDecode(movie.OriginalName),
                           ReleaseYear = movie.ReleaseYear,
                           TotalRatesNumber = movie.TotalRatesNumber,
                           NumberUsersWithSameRating = movie.UsersWithSameRating,
                           UserMovieRate = movie.UsersRate,
                           MovieRateList = ratingList
                       };
        }

        public static List<RatedMovieViewModel> PopulateListRatedMovieViewModel(List<DtoRatedMovie> movieList)
        {
            var movieViewModelList = new List<RatedMovieViewModel>();

            foreach (var movie in movieList)
            {
                movieViewModelList.Add(PopulateRatedMovieViewModel(movie));
            }

            return movieViewModelList;
        }
    }
}