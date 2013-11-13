using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Enums;

namespace MovieRatingCalculator.DataScraping.Tests
{
    [TestClass]
    public class KinopoiskMovieListParserTests
    {
        [TestMethod]
        public void ParseMovieListHtml_ShouldReturn100Movies()
        {
            // Arrange
            var kinopoiskMovieListParser = new KinopoiskMovieListParser();

            // Act
            List<Movie> movieList =  kinopoiskMovieListParser.ParseMovieListHtml(
                File.ReadAllText(@"TestData\kinopoisk.search.results.2012.100movies.htm", Encoding.GetEncoding(1251)));

            // Assert
            Assert.AreEqual(100, movieList.Count);
        }

        [TestMethod]
        public void ParseMovieListHtml_ShouldIncludeHobbitMovie()
        {
            // Arrange
            var kinopoiskMovieListParser = new KinopoiskMovieListParser();

            // Act
            List<Movie> movieList = kinopoiskMovieListParser.ParseMovieListHtml(
                    File.ReadAllText(@"TestData\kinopoisk.search.results.2012.100movies.htm", Encoding.GetEncoding(1251)));

            // Assert
            Assert.IsTrue(movieList.Any(m => m.OriginalName == "The Hobbit: An Unexpected Journey"));
        }

        [TestMethod]
        public void ParseMovieListHtml_ShouldIncludeCorrectInfoAboutHobbitMovie()
        {
            // Arrange
            var kinopoiskMovieListParser = new KinopoiskMovieListParser();

            // Act
            List<Movie> movieList =  kinopoiskMovieListParser.ParseMovieListHtml(
                File.ReadAllText(@"TestData\kinopoisk.search.results.2012.100movies.htm", Encoding.GetEncoding(1251)));
            Movie hobbitMovie = movieList.Single(m => m.OriginalName == "The Hobbit: An Unexpected Journey");

            // Assert
            Assert.AreEqual("Хоббит: Нежданное путешествие", hobbitMovie.Name);
            Assert.AreEqual(2012, hobbitMovie.ReleaseYear);
            // etc...
        }

        [TestMethod]
        public void ParseMovieListHtml_ShouldIncludeParisManhattanMovie()
        {
            // Arrange
            var kinopoiskMovieListParser = new KinopoiskMovieListParser();

            // Act
            List<Movie> movieList = 
            kinopoiskMovieListParser.ParseMovieListHtml(
                File.ReadAllText(@"TestData\kinopoisk.search.results.2012.100movies.htm", Encoding.GetEncoding(1251)));

            // Assert
            Assert.IsTrue(movieList.Any(m => m.OriginalName == "Paris-Manhattan"));
        }

        [TestMethod]
        public void ParseMovieListHtml_ShouldIncludeMovieFantasticFlyingBooksWithNoActors()
        {
            //Arrange
            var kinopoiskMovieListParser = new KinopoiskMovieListParser();

            //Act
            List<Movie> movieList =
            kinopoiskMovieListParser.ParseMovieListHtml(
                File.ReadAllText(@"TestData\kinopoisk.search.results.2011.200movies.htm", Encoding.GetEncoding(1251)));

            //Assert
           Assert.IsTrue(!movieList.Single(movie => movie.OriginalName == "The Fantastic Flying Books of Mr. Morris Lessmore").
                    MovieProductionParticipants.Any(
                        part => part.MovieParticipantType.ParticipantTypeId == (int) ParticipantTypeEnum.Actor));
        }
    }
}
