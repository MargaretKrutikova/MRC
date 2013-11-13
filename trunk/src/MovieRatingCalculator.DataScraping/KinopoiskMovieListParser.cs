using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Enums;

namespace MovieRatingCalculator.DataScraping
{
    public class KinopoiskMovieListParser
    {
        public List<Movie> ParseMovieListHtml(string movieListHtml)
        {
            var movieList = new List<Movie>();
            var doc = new HtmlDocument();
            doc.LoadHtml(movieListHtml);

            IEnumerable<HtmlNode> searchResultNodes = doc.DocumentNode.SelectNodes(
                "//div[@class='search_results search_results_last']/div[contains(@class, 'element')]");

            if (searchResultNodes == null || !searchResultNodes.Any())
            {
                return movieList;
            }

            foreach (var movieNode in searchResultNodes)
            {
                var infoNode = movieNode.SelectSingleNode(".//div[@class='info']"); 

                if(infoNode != null)
                {
                    var movie = new Movie();

                    //kinopoisk rating
                    var ratingNode = movieNode.SelectSingleNode(".//div[@class='rating']");
                    if (ratingNode != null && ratingNode.Attributes.Contains("title"))
                    {
                        var movieRate = ratingNode.Attributes["title"].Value.Trim().Split();
                        movie.KinopoiskMovieRating = Double.Parse(movieRate[0], CultureInfo.InvariantCulture);
                        movie.KinopoiskNumberUsersRate = Int32.Parse(Regex.Replace(movieRate[1], "[^0-9]", ""));

                        if (movie.KinopoiskNumberUsersRate < 100)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var nameNode = infoNode.SelectSingleNode(".//*[@class='name']");
                    if (nameNode != null)
                    {
                        movie.Name = nameNode.FirstChild.InnerText.Replace("&nbsp;", " ");
                        movie.KinopoiskMovieId = nameNode.FirstChild.Attributes.Contains("href")
                                                     ? nameNode.FirstChild.Attributes["href"].Value.Split('/')[4] : "";
                        movie.ReleaseYear = Int16.Parse(nameNode.LastChild.InnerText);
                    }
                    else
                    {
                        Console.WriteLine("ERROR. No information about movie name.");
                        continue;
                    }

                    var spanNodes = infoNode.ChildNodes.Where(node => node.Name == "span").ToList();
                    if (spanNodes.Count == 3)
                    {
                        ParseAdditionalMovieInfo(spanNodes, movie);
                    }
                    else
                    {
                        Console.WriteLine("ERROR IN FILM \"{0}\", ID = {1}. Unexpected number of spans, count = {2}.",
                            movie.Name, movie.KinopoiskMovieId, spanNodes.Count);
                    }

                    movieList.Add(movie);
                }
            }

            return movieList;
        }

        public void ParseAdditionalMovieInfo(List<HtmlNode> spanNodes, Movie movie)
        {
            //first span with original name and duration
            string firstSpanInfo = spanNodes[0].InnerText;
            if (firstSpanInfo.Contains("мин"))
            {
                int ind = firstSpanInfo.LastIndexOf(',');
                if (ind != -1)
                {
                    movie.OriginalName = firstSpanInfo.Substring(0, ind).Replace("&nbsp;", " ");
                    movie.Duration = Int16.Parse(firstSpanInfo.Substring(ind + 1).Replace("мин", "").Trim());
                }
                else
                {
                    movie.OriginalName = String.Copy(movie.Name);
                    movie.Duration = Int16.Parse(firstSpanInfo.Replace("мин", "").Trim());
                }
            }
            else
            {
                movie.OriginalName = !String.IsNullOrWhiteSpace(firstSpanInfo.Trim())
                                         ? firstSpanInfo.Trim().Replace("&nbsp;", " ")
                                         : String.Copy(movie.Name);
            }

            //second span with country, director and genres
            if (!String.IsNullOrWhiteSpace(spanNodes[1].FirstChild.InnerText))
            {
                movie.Countries.Add(new Country { Name = spanNodes[1].FirstChild.InnerText.
                                                            Replace("...", "").Trim(new[] {',', ' '}) });
            }

            movie.MovieProductionParticipants = new List<MovieProductionParticipant>();

            var directorNode = spanNodes[1].SelectSingleNode(".//*[@class='director']");
            if (directorNode != null && !String.IsNullOrWhiteSpace(directorNode.InnerText) 
                && directorNode.InnerText.Contains("реж."))
            {
                AddNewMovieParticipants(movie, directorNode.InnerText.Replace("реж.", "").Replace("...", "").Trim(),
                                        ParticipantTypeEnum.Director);
            }
            
            if (!String.IsNullOrWhiteSpace(spanNodes[1].LastChild.InnerText))
            {
                var genres = spanNodes[1].LastChild.InnerText.Replace("...", "").Trim(new[] {')', '(', ' ', '\r', '\n'}).Split(',');
                foreach (var genre in genres)
                {
                    movie.Genres.Add(new Genre { Name = genre.Trim()});
                }
            }

            //third span with actors
            if (!String.IsNullOrWhiteSpace(spanNodes[2].InnerText))
            {
                var actors = spanNodes[2].InnerText.Replace("...", "").Trim(new[] { ',', ' ', '\r', '\n' }).Split(',');
                foreach(var actor in actors)
                {
                    AddNewMovieParticipants(movie, actor.Trim(), ParticipantTypeEnum.Actor);
                }
            }
        }

        public void AddNewMovieParticipants(Movie movie, string name, ParticipantTypeEnum type)
        {
            var participant = new MovieParticipant
                                  {
                                      Name = name
                                  };
            var movieParticipant = new MovieParticipantType
            {
                MovieParticipant = participant,
                ParticipantTypeId = (int)type
            };
            movie.MovieProductionParticipants.Add(new MovieProductionParticipant
            {
                MovieParticipantType = movieParticipant
            });
        }
    }
}
