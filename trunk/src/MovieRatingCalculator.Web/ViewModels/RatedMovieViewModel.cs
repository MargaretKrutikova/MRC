using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieRatingCalculator.Web.ViewModels
{
    public class RatedMovieViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }

        [DisplayName("Year")]
        public short ReleaseYear { get; set; }

        public int TotalRatesNumber { get; set; }
        public int NumberUsersWithSameRating { get; set; }

        public List<SelectListItem> MovieRateList { get; set; }
        public int UserMovieRate { get; set; }
    }
}