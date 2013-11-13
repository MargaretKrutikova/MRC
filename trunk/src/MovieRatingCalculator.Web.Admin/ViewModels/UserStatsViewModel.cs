using System.Collections.Generic;

namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class UserStatsViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public List<MovieRatingInfoViewModel> RatedMovies{ get; set; }

        public UserStatsViewModel()
        {
            RatedMovies = new List<MovieRatingInfoViewModel>();
        }
    }
}