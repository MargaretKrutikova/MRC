using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Dissimilarities;
using MovieRatingCalculator.BusinessLogic.Interfaces;
using MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Repository;
using MovieRatingCalculator.Web.Filters;
using MovieRatingCalculator.Web.ViewModelHelpers;
using MovieRatingCalculator.Web.ViewModels;
using MovieRatingCalculator.DataAccess.Interfaces;
using System.Configuration;

namespace MovieRatingCalculator.Web.Controllers
{
    public class HomeController : Controller
    {
        public const int PageSize = 10;

        private readonly IMovieRepository movieRepository;
        private readonly IUserRepository userRepository;
        private readonly IRecommendationAlgorithm recommendationAlgorithm;

        public HomeController()
        {
            this.movieRepository = new MovieRepository();
            this.userRepository = new UserRepository();
            recommendationAlgorithm = new UserBasedMovieRecommendation();
        }

        // Dependency Injection = DI
        // Constructor Injection
        public HomeController(IMovieRepository movieRepository, IUserRepository userRepository)
        {
            this.movieRepository = movieRepository;
            this.userRepository = userRepository;
        }

        [HttpGet]
        [NoCache]
        public ActionResult SearchMovies(SearchMovieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ModelState.Clear();
            model.IsUserLoggedIn = User.Identity.IsAuthenticated;
            var movies = new List<Movie>();
            int moviesCount = 0;

            if (string.Compare(model.act, "search") == 0 || !string.IsNullOrWhiteSpace(model.FilterField)) 
            {
                movies = movieRepository.FindMovies(model.FilterField, model.ReleaseYear, User.Identity.Name,
                                    model.CurrentPageIndex, PageSize, ref moviesCount);
                model.act = "search";
            }
            else
            {
                movies = movieRepository.FindSuggestedMovies(User.Identity.Name, model.CurrentPageIndex,
                                                                     PageSize, ref moviesCount, model.ReleaseYear);
                model.act = "suggest";
            }
            var movieViewModelList = MovieViewModelHelper.PopulateMovieList(movies, User.Identity.IsAuthenticated);            
            model.PopulatePagedMovieList(movieViewModelList, PageSize, moviesCount);

            return View(model);
        }

        [NoCache]
        public ActionResult Index()
        {
            return RedirectToAction("SearchMovies");
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public ActionResult RatedMovies(UserRatedMoviesViewModel model)
        {
            var ratedMovies = movieRepository.GetUserRatedMovies(User.Identity.Name, model.ReleaseYear);

            model.RatedMovies = RatedMovieViewModelHelper.PopulateListRatedMovieViewModel(ratedMovies);
            return View(model);
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public ActionResult RecommendMovies()
        {
            ClusterRatedItem authorizedUserRatings = userRepository
                                                .GetUserRatingsByEmail(User.Identity.Name);
            if (authorizedUserRatings == null)
            {
                return View(new List<MovieViewModel>());
            }

            List<RecommendedMovie> topNMoviesRecommendations = recommendationAlgorithm.
                GetTopNRecommendations(authorizedUserRatings, ElementsDissimilarities.PearsonCoefficient);

            if (!topNMoviesRecommendations.Any())
            {
                return View(new List<MovieViewModel>());
            }

            List<int> topNMoviesIds = topNMoviesRecommendations.OrderByDescending(m => m.Rating).Take(30).
                Select(m => m.Id).ToList();

            List<Movie> recommendedMovies = movieRepository.GetMoviesByIds(topNMoviesIds);
            List<MovieViewModel> recommendedMovieViewModel = MovieViewModelHelper.PopulateMovieList(recommendedMovies).
                OrderByDescending(m => m.ReleaseYear).ToList();

            return View(recommendedMovieViewModel);
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult SaveMovieRate(int movieId, short rating, bool doUpdateMovieInfo = false)
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                var loggedInUser = userRepository.GetUserByEmail(User.Identity.Name);
                if (loggedInUser != null)
                {
                    var movieRating = new MovieRating
                                          {
                                              UserId = loggedInUser.Id,
                                              MovieId = movieId,
                                              Rating = rating,
                                              UpdateDate = DateTime.Now
                                          };
                    if (rating == 0)
                    {
                        movieRepository.DeleteMovieRating(movieRating);
                        return Json("success-delete", JsonRequestBehavior.AllowGet);
                    }

                    if (doUpdateMovieInfo)
                    {
                        var updateInfo = movieRepository.UpdateRatedMovieInfo(movieRating);
                        return Json(new
                                        {
                                            result = "success-add",
                                            totalNumber = updateInfo.TotalRatesNumber,
                                            sameRatingNumber = updateInfo.UsersWithSameRating,
                                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        movieRepository.ChangeMovieRating(movieRating);
                        return Json("success-add", JsonRequestBehavior.AllowGet);
                    }                    
                }
            }

            return Json("error-unathorized-access", JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult GetMovieRatingDistribution(int movieId)
        {
            return Json(movieRepository.GetMovieRatingDistribution(movieId), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult GetUserRatingStatus()
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                return Json(new
                            {
                                NumberOfMoviesRated = userRepository.GetNumberOfRatedMovies(User.Identity.Name),
                                // TODO: Move this hardcoded value to web.config
                                MinMoviesToRate = 50
                            },
                            JsonRequestBehavior.AllowGet);
            }

            return Json("error-unathorized-access", JsonRequestBehavior.AllowGet);
        }
    }
}
