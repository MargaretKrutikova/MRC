using System.Collections.Generic;

namespace MovieRatingCalculator.DataAccess.Interfaces
{
    public interface IMovieRatingRepository
    {
        List<MovieRating> GetMoviesRatings();
    }
}