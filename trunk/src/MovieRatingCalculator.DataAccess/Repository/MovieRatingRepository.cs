using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.DataAccess.Interfaces;

namespace MovieRatingCalculator.DataAccess.Repository
{
    public class MovieRatingRepository : IMovieRatingRepository
    {
        public List<MovieRating> GetMoviesRatings()
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.MovieRatings.ToList();
            }
        }
    }
}