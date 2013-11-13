using System.Collections.Generic;
using System.Linq;

namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class MovieRatingsViewModel
    {
        public List<MovieRatingInfoViewModel> MovieRatings { get; set; }

        public int NumberOfMovies { get { return MovieRatings.Count; } }
        public int NumberOfRatings { get { return MovieRatings.Sum(r => r.RatedByNumberOfUsers); } }

        public MovieRatingsViewModel()
        {
            MovieRatings = new List<MovieRatingInfoViewModel>();
        }
    }
}