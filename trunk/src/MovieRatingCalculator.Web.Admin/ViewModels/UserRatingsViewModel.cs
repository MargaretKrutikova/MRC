using System.Collections.Generic;
using System.Linq;

namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class UserRatingsViewModel
    {
        public List<UserRatingInfoViewModel> UserRatings { get; set; }

        public int NumberOfUsers { get { return UserRatings.Count; } }
        public int NumberOfRatings { get { return UserRatings.Sum(r => r.NumberOfRatedMovies); } }

        public UserRatingsViewModel()
        {
            UserRatings = new List<UserRatingInfoViewModel>();
        }
    }
}