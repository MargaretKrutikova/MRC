using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieRatingCalculator.Web.ViewModels
{
    public class MovieViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string OriginalName { get; set; }

        [DisplayName("Year")]
        public short ReleaseYear { get; set; }

        [DisplayName("Length")]
        public short? Duration { get; set; }

        [DisplayName("Link")]
        public string KinopoiskMovieLink { get; set; }

        [DisplayName("Rate")]
        public double? KinopoiskMovieRating { get; set; }

        [DisplayName("Count")]
        public int? KinopoiskNumberUsersRate { get; set; }

        public List<string> Genres { get; set; }

        public List<string> Countries { get; set; }

        [DisplayName("Director")]
        public string DirectorName { get; set; }

        public List<string> Actors { get; set; }

        public List<SelectListItem> MovieRateList { get; set; }
        public int LoggedInUserMovieRate { get; set; }
    }
}