using System.IO;
using System.Net;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Interfaces;
using MovieRatingCalculator.DataAccess.Repository;
using MovieRatingCalculator.DataScraping.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.DataScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            IMovieScraper movieScraper = new KinopoiskMovieScraper();
            IMovieRepository movieRepository = new MovieRepository();
            List<string> kinopoiskIds = movieRepository.GetKinopoiskMovieIds();

            for (int year = 1930; year <= 1969; year++)
            {
                List<Movie> movieList = movieScraper.ScrapeMostPopularMovies(year);
                Console.WriteLine("{0} films of the year {1} were scraped from kinopoisk.", movieList.Count, year);
                var clearedMovieList = new List<Movie>();

                foreach (var movie in movieList)
                {
                    if (!kinopoiskIds.Contains(movie.KinopoiskMovieId))
                    {
                        kinopoiskIds.Add(movie.KinopoiskMovieId);
                        clearedMovieList.Add(movie);
                    }
                }

                movieRepository.AddMovieList(clearedMovieList);
                Console.WriteLine("Films of the year {0} were saved to the database.\n", year);
            }

            Console.WriteLine("Movie scraping finished.");
            Console.ReadKey();

        }
    }
}
