using System.Collections.Generic;

namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class MovieStatsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string KinopoiskLink { get; set; }
        
        public List<UserRatingInfoViewModel> UsersRated { get; set; }

        public MovieStatsViewModel()
        {
            UsersRated = new List<UserRatingInfoViewModel>();
        }
    }
}