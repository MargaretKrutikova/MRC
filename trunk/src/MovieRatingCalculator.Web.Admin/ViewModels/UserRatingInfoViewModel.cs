namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public class UserRatingInfoViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int NumberOfRatedMovies { get; set; }
        public int NumberOfLogins { get; set; }
        public string LastLoginDate { get; set; }
        public string LastLoginIpAddress { get; set; }

        public double MovieRating { get; set; }
    }
}