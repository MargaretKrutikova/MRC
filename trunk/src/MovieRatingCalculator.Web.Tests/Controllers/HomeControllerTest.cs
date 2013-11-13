using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovieRatingCalculator.Web;
using MovieRatingCalculator.Web.Controllers;
using MovieRatingCalculator.Web.ViewModels;
using Moq;
using MovieRatingCalculator.DataAccess.Interfaces;
using MovieRatingCalculator.DataAccess;

namespace MovieRatingCalculator.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private ControllerContext GetMockControllerContext()
        {
            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns((string)null);
            mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(false);
            
            return mockContext.Object;
        }

      /*  [TestMethod]
        public void SearchMovies_ShouldCall_MovieRepository_GetRatedMovies_IfFilterFieldIsNull()
        {
            // Arrange
            int count = 0;
            var mockMovieRepository = new Mock<IMovieRepository>();
            mockMovieRepository.Setup(r => r.GetRatedMovies(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), ref count)).Returns(new List<Movie>());
            
            var controller = new HomeController(mockMovieRepository.Object, new Mock<IUserRepository>().Object);
            controller.ControllerContext = GetMockControllerContext();
            controller.ViewData = new ViewDataDictionary();
            
            // Act
            var result = controller.SearchMovies(new SearchMovieViewModel()) as ViewResult;
            var viewModel = result.Model;

            // Assert
            Assert.IsInstanceOfType(viewModel, typeof(SearchMovieViewModel));
            mockMovieRepository.Verify(m => m.GetRatedMovies(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), ref count), Times.Once());
        }

        [TestMethod]
        public void SearchMovies_ViewModel_MovieList_ShouldContainMovies_IfFilterFieldIsNotNull()
        {
            
            // Arrange
            var mockMovieRepository = new Mock<IMovieRepository>();
            int count = 0;
            mockMovieRepository.Setup(r => r.FindMovies("MovieName", null, null, It.IsAny<int>(),
                                                        It.IsAny<int>(), ref count)).
                                                        Returns(new List<Movie>{new Movie()});
            
            var controller = new HomeController(mockMovieRepository.Object, new Mock<IUserRepository>().Object);
            controller.ControllerContext = GetMockControllerContext();
            var model = new SearchMovieViewModel() {FilterField = "MovieName"};
            // Act
            var result = controller.SearchMovies(model) as ViewResult;
            var viewModel = result.Model;

            // Assert
            Assert.IsInstanceOfType(viewModel, typeof(SearchMovieViewModel));
            Assert.IsTrue(((SearchMovieViewModel)viewModel).SearchResultMovieList.Any());
             
        }

        [TestMethod]
        public void SearchMovies_ShouldCall_MovieRepository_FindMovies_IfFilterFieldIsNotNull()
        {
            // Arrange
            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.User.Identity.Name).Returns((string)null);
            mockContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(false);

            var mockMovieRepository = new Mock<IMovieRepository>();
            int count = 0;
            mockMovieRepository.Setup(r => r.FindMovies("MovieName", null, null, It.IsAny<int>(), 
                It.IsAny<int>(), ref count)).Returns(new List<Movie> { new Movie() });
            
            var controller = new HomeController(mockMovieRepository.Object, new Mock<IUserRepository>().Object);
            controller.ControllerContext = mockContext.Object;
            var model = new SearchMovieViewModel() { FilterField = "MovieName" };
            // Act
            var result = controller.SearchMovies(model) as ViewResult;

            // Assert
            mockMovieRepository.Verify(m => m.FindMovies("MovieName", null, null, It.IsAny<int>(), It.IsAny<int>(), ref count),
                Times.Once());
          }*/
    }
}
