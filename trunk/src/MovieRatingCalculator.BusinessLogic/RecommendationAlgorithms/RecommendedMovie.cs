
namespace MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms
{
    public class RecommendedMovie
    {
        public int Id { get; set; }
        public double Rating { get; set; }

        public RecommendedMovie(double rating, int id)
        {
            Rating = rating;
            Id = id;
        }
    }
}
