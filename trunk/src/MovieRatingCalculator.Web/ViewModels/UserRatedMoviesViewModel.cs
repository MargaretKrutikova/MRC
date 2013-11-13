using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieRatingCalculator.Web.ViewModels
{
    public class UserRatedMoviesViewModel
    {
        public List<RatedMovieViewModel> RatedMovies { get; set; } 

        public short? ReleaseYear
        {
            get { return y; }
            set { y = value; }
        }

        [Range(typeof(short), "1930", "2012", ErrorMessage = "Year should be between 1930 and 2012.")]
        [RegularExpression(@"^\d+(\.\d)?$", ErrorMessage = "Invalid character.")]
        public short? y { get; set; }

        public UserRatedMoviesViewModel()
        {
            RatedMovies = new List<RatedMovieViewModel>();
        }
    }
}