using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.DataAccess;

namespace MovieRatingCalculator.DataScraping.Interfaces
{
    public interface IMovieScraper
    {
        List<Movie> ScrapeMostPopularMovies(int year);
    }
}
