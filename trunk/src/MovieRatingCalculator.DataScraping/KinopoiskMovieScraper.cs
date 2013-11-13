using System.IO;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataScraping.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MovieRatingCalculator.DataScraping
{
    public class KinopoiskMovieScraper : IMovieScraper
    {
        public List<Movie> ScrapeMostPopularMovies(int year)
        {
            string url =
                "http://www.kinopoisk.ru/s/type/film/list/1/order/rating/m_act%5Byear%5D/{0}/m_act%5Btype%5D/film/perpage/200/page/{1}";

            var movieList = new List<Movie>();
            var parser = new KinopoiskMovieListParser();

            for (int i = 1; i <= 5; i++)
            {
                string movieListHtml = GetPageHtml(String.Format(url, year, i));
                var movies = parser.ParseMovieListHtml(movieListHtml);
                
                if (movies.Any())
                {
                    movieList.AddRange(movies);
                }
            }

            return movieList;
        }

        public string GetPageHtml(string url)
        {
            string pageHtml = "";

            var kinopoiskRequest = (HttpWebRequest) WebRequest.Create(url);
            kinopoiskRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)";
            kinopoiskRequest.Headers.Add("Accept-Language", "ru-Ru");
            kinopoiskRequest.Accept =
                "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            kinopoiskRequest.Method = "GET";

            var kinopoiskResponse = (HttpWebResponse)kinopoiskRequest.GetResponse();
            if (kinopoiskResponse.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = kinopoiskResponse.GetResponseStream())
                {
                    if (stream != null)
                    {
                        pageHtml = new StreamReader(stream, Encoding.GetEncoding("windows-1251")).ReadToEnd();
                    }
                }
            }

            return pageHtml;
        }
    }
}
