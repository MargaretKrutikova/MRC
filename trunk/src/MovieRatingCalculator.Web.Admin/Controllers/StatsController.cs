using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Dissimilarities;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Interfaces;
using MovieRatingCalculator.DataAccess.Repository;
using MovieRatingCalculator.DataExport;
using MovieRatingCalculator.Web.Admin.DataSpaceHelpers;
using MovieRatingCalculator.Web.Admin.ViewModelHelpers;
using System.Web.UI;
using MovieRatingCalculator.Web.Admin.ViewModels;

namespace MovieRatingCalculator.Web.Admin.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IMovieRepository movieRepository;
        private readonly IMovieRatingRepository movieRatingRepository;

        public StatsController()
        {
            this.movieRepository = new MovieRepository();
            this.userRepository = new UserRepository();
            this.movieRatingRepository = new MovieRatingRepository();
        }

        public StatsController(IUserRepository userRepository, IMovieRepository movieRepository)
        {
            this.userRepository = userRepository;
            this.movieRepository = movieRepository;
        }

        public ActionResult UserList()
        {
            List<User> users = userRepository.GetUsersWithRatings();

            return View(ViewModelHelper.PopulateUserRatingViewModel(users));
        }

        public ActionResult MovieList()
        {
            List<Movie> movies = movieRepository.GetMoviesWithRatings();

            return View(ViewModelHelper.PopulateMovieRatingViewModel(movies));
        }

        public ActionResult UserStats(int userId)
        {
            User user = userRepository.GetUserStats(userId);

            return View(ViewModelHelper.PopulateUserStatsViewModel(user));
        }

        public ActionResult MovieStats(int movieId)
        {
            Movie movie = movieRepository.GetMovieStats(movieId);

            return View(ViewModelHelper.PopulateMovieStatsViewModel(movie));
        }

        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
        public JsonResult GetMovieRatingDistribution(int movieId)
        {
            return Json(movieRepository.GetMovieRatingDistribution(movieId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportStats()
        {
            var ratings = movieRatingRepository.GetMoviesRatings();
            string fileName = "DataSpace.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(ExcelManager.GetBytes(DataSpaceHelper.GetRatingsForExport(ratings)), contentType, fileName);
        }

        public ActionResult RunClusteringAlgs(ClusteringViewModel model, string runAlgBtn)
        {
            if (runAlgBtn == null)
            {
                return View(model);
            }

            switch (model.ClusteringAlgorithm)
            {
               case ClusteringAlgType.AgglomerativeClustering:
               case ClusteringAlgType.KMedoids:
                   if (model.ClustersNumber <= 0)
                   {
                       ModelState.AddModelError("ClustersNumber", "Numbers of clusters must be posistive");
                   }
                  break;

               case ClusteringAlgType.Dbscan:
                   if (model.Mcp <= 0)
                   {
                       ModelState.AddModelError("Mcp", "Paremeter Mcp must be posistive");
                   }
                   if (model.Eps <= 0)
                   {
                       ModelState.AddModelError("Eps", "Paremeter Eps must be posistive");
                   }
                  break;
            }

            if (!ModelState.IsValid)
            {
                model.Message = "There were some errors";
                return View(model);
            }

            List<ClusterRatedItem> data = GetClusteringData(model.ClusteringDataType, model.Filter);
            if (data == null || !data.Any())
            {
                ModelState.AddModelError("Data", "Data is empty.");
                return View(model);
            }

            var distanceFunction = GetDistanceFunction(model.ClusteringDistanceType);
            if (distanceFunction == null)
            {
                ModelState.AddModelError("Distance", "No distance function found.");
                return View(model);
            }

            var clusters = new List<List<ClusterRatedItem>>();

            switch (model.ClusteringAlgorithm)
            {
                case ClusteringAlgType.AgglomerativeClustering:
                    var agglomerativeAlg = new AgglomerativeClustering<ClusterRatedItem>(data, distanceFunction);
                    clusters = agglomerativeAlg.FindClusters(model.ClustersNumber);
                    break;

                case ClusteringAlgType.KMedoids:
                    var kmedoidsAlg = new KMedoidsClustering<ClusterRatedItem>(data, distanceFunction);
                    clusters = kmedoidsAlg.FindClusters(model.ClustersNumber);
                    break;

                 case ClusteringAlgType.Dbscan:
                     var dbscanAlg = new DbscanClustering<ClusterRatedItem>(data, distanceFunction);
                     clusters = dbscanAlg.FindClusters(model.Eps, model.Mcp);
                    break;
            }

            switch (model.ClusteringDataType)
            {
                case ObjectType.Users:
                    userRepository.UpdateClusters(clusters);
                    break;
                case ObjectType.Movies:
                    movieRepository.UpdateClusters(clusters);
                    break;
            }

            model.Message = "Clustering was performed successfully.";
            return View(model);
        }

        private List<ClusterRatedItem> GetClusteringData(ObjectType dataType, int filter)
        {
            switch (dataType)
            {
                 case ObjectType.Users:
                    return userRepository.GetUsersRatingsForClustering().
                        Where(u => u.Ratings.Count(r => r.Rating != 0) >= filter).ToList();
                    
                case ObjectType.Movies:
                    return movieRepository.GetMoviesRatingsForClustering().
                        Where(u => u.Ratings.Count(r => r.Rating != 0) >= filter).ToList();
            }

            return null;
        }

        private CalculateDistanceDelegate<ClusterRatedItem> GetDistanceFunction(DistanceType distanceType)
        {
            switch (distanceType)
            {
                    case DistanceType.PearsonCoefficient:
                          return ElementsDissimilarities.PearsonCorrelationDissimilarity;
                    case DistanceType.CosDistance:
                          return ElementsDissimilarities.CosDistance;
                    case DistanceType.SpearmanCoefficient:
                          return ElementsDissimilarities.SpearmanCorrelationDissimilarity;
            }

            return null;
        }
    }
}
