using System;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Interfaces;
using MovieRatingCalculator.Web.Controllers;
using MovieRatingCalculator.Web.Interfaces;
using MovieRatingCalculator.Web.ViewModels;

namespace MovieRatingCalculator.Web.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void LogIn_ShouldRedirectToIndex_IfModelIsValid()
        {
            //Arrange
            var accountController = new AccountController(new Mock<IUserRepository>().Object,
                                                          new Mock<IFormsAuthenticationService>().Object,
                                                          new Mock<IRequestService>().Object);
            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            
            //Act
            var result = accountController.Login(new UserViewModel(), null);

            //Assert
            Assert.IsNotNull(result as RedirectToRouteResult);
            Assert.AreEqual((result as RedirectToRouteResult).RouteValues["controller"], "Home");
            Assert.AreEqual((result as RedirectToRouteResult).RouteValues["action"], "Index");
        }

        [TestMethod]
        public void LogIn_ShouldRedirectToLogInView_IfModelIsInValid()
        {
            //Arrange
            var accountController = new AccountController(new Mock<IUserRepository>().Object,
                                              new Mock<IFormsAuthenticationService>().Object,
                                              new Mock<IRequestService>().Object);
            accountController.ModelState.AddModelError("Email", "required");

            //Act
            var result = accountController.Login(new UserViewModel(), null);

            //Assert
            Assert.IsNotNull(result as ViewResult);
            Assert.AreEqual((result as ViewResult).ViewName, "");
        }

        [TestMethod]
        public void LogIn_ShouldCall_UserRepository_GetUserByEmail()
        {
            //Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var formsAuthenticationMock = new Mock<IFormsAuthenticationService>();
            var accountController = new AccountController(userRepositoryMock.Object, formsAuthenticationMock.Object,
                                                          new Mock<IRequestService>().Object);
            
            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            //Act
            var result = accountController.Login(new UserViewModel(), null);

            //Assert
            userRepositoryMock.Verify( r => r.GetUserByEmail(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        public void LogIn_ShouldCall_AuthenticationService_SetAuthCookie_IfGetUserByEmail_ReturnsUser()
        {
            //Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns(new User());
            var formsAuthenticationMock = new Mock<IFormsAuthenticationService>();
            
            var accountController = new AccountController(userRepositoryMock.Object, formsAuthenticationMock.Object,
                                                          new Mock<IRequestService>().Object);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            //Act
            var result = accountController.Login(new UserViewModel(), null);

            //Assert
            formsAuthenticationMock.Verify(r => r.SetAuthCookie(It.IsAny<string>(), false), Times.Once());
        }

        [TestMethod]
        public void LogIn_ShouldCall_AuthenticationService_SetAuthCookie_IfGetUserByEmail_ReturnsNull()
        {
            //Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail(It.IsAny<string>())).Returns((User)null);
            
            var formsAuthenticationMock = new Mock<IFormsAuthenticationService>();
            var accountController = new AccountController(userRepositoryMock.Object, formsAuthenticationMock.Object,
                                                          new Mock<IRequestService>().Object);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            //Act
            var result = accountController.Login(new UserViewModel(), null);

            //Assert
            formsAuthenticationMock.Verify(r => r.SetAuthCookie(It.IsAny<string>(), false), Times.Once());
        }

        [TestMethod]
        public void LogIn_ShouldCall_UserRepository_Add_IfGetUserByEmail_ReturnsNull()
        {
            //Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail("test@test.test")).Returns((User)null);

            var userViewModel = new UserViewModel
                                    {
                                        Email = "test@test.test",
                                        FirstName = "Name1",
                                        LastName = "Name2"
                                    };
            var accountController = new AccountController(userRepositoryMock.Object,
                                                          new Mock<IFormsAuthenticationService>().Object,
                                                          new Mock<IRequestService>().Object);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            //Act
            var result =
                accountController.Login(userViewModel, null);

            //Assert
            userRepositoryMock.Verify(
                r => r.Add(It.Is<User>(user =>
                                       user.Email == userViewModel.Email &&
                                       user.FirstName == userViewModel.FirstName &&
                                       user.LastName == userViewModel.LastName
                               )), Times.Once());
        }

        [TestMethod]
        public void LogOff_ShouldCall_AuthenticationService_SignOut()
        {
            //Arrange
            var formsAuthenticationMock = new Mock<IFormsAuthenticationService>();
            var accountController = new AccountController(new Mock<IUserRepository>().Object,
                                                          formsAuthenticationMock.Object,
                                                          new Mock<IRequestService>().Object);

            var mockContext = new Mock<ControllerContext>();
            mockContext.SetupGet(p => p.HttpContext.Request).Returns((HttpRequestBase)null);
            accountController.ControllerContext = mockContext.Object;
            //Act
            var result = accountController.LogOff();

            //Assert
            formsAuthenticationMock.Verify(r => r.SignOut(), Times.Once());
        }
    }
}
