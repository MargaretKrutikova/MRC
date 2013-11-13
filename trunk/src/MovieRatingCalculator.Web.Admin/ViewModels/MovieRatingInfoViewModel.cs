namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class MovieRatingInfoViewModel
    {
        public int MovieId { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public string KinopoiskLink { get; set; }
        public double UserRating { get; set; }
        public double AverageRating { get; set; }
        public int RatedByNumberOfUsers { get; set; }
    }
}