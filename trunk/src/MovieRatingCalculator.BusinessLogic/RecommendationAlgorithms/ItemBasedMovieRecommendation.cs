using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Dissimilarities;
using MovieRatingCalculator.BusinessLogic.Interfaces;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Repository;

namespace MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms
{
    public class ItemBasedMovieRecommendation : IRecommendationAlgorithm
    {
        public List<RecommendedMovie> GetTopNRecommendations(ClusterRatedItem user,
            CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity)
        {
            var movieRepository = new MovieRepository();

            List<int> movieIds = user.Ratings.Where(m => m.Rating != 0).Select(r => r.ElementId).ToList();
            List<short> clusterMovieIds = movieRepository.GetMovieClusterIdsByMovieIds(movieIds);

            //all movies, which have clusterid one of the clusterids of user rated movies
            var groupedItems = movieRepository.GetMoviesRatingsByClusterIds(clusterMovieIds);
            var currentUserMovies = movieRepository.GetMoviesRatingsByMovieIds(movieIds);

            var recommendedMovies = new List<RecommendedMovie>();

            for (int i = 0; i < clusterMovieIds.Count; i++)
            {
                var userRatedMovies = currentUserMovies.Where(m => m.ClusterId == clusterMovieIds[i]).ToList();
                var groupMovieIds = userRatedMovies.Select(m => m.Id).ToList();

                var groupMovies = groupedItems.Where(m => m.ClusterId == clusterMovieIds[i] && !groupMovieIds.Contains(m.Id)).ToList();

                foreach (var groupMovie in groupMovies)
                {
                    double sum1 = 0, sum2 = 0;
                    foreach (var movie in userRatedMovies)
                    {
                        double similarity = calculateSimilarity(groupMovie, movie);
                        int currentRating = user.Ratings.First(m => m.ElementId == movie.Id).Rating;

                        sum1 += currentRating * similarity;
                        sum2 += Math.Abs(similarity);
                    }

                    recommendedMovies.Add(new RecommendedMovie(sum1 / sum2, groupMovie.Id));
                }
            }

            return recommendedMovies.OrderByDescending(m => m.Rating).ToList();
        }

        public double? CalculateMAEForUserByItemClustering(ClusterRatedItem user, CalculateDistanceDelegate<ClusterRatedItem> calculateDistance,
           List<ClusterRatedItem> orginalItems)
        {
            List<ElementRating> userRatings = user.Ratings.Where(r => r.Rating != 0).ToList();
            var items = orginalItems.Select(i => new ClusterRatedItem(i.Id,
                                    i.Ratings.Select(r => new ElementRating(r.ElementId, r.Rating)).ToList())).ToList();
                                                                      
            var partMovies = new List<ElementRating>();
            int partSize = (int) Math.Ceiling(userRatings.Count*0.3);

            var rn = new Random();

            for (int i = 0; i < partSize; i++)
            {
                int index;
                ClusterRatedItem item;

                do
                {
                    index = rn.Next(0, userRatings.Count - 1);
                    item = items.SingleOrDefault(it => it.Id == userRatings[index].ElementId);
                } while (item == null);

                partMovies.Add(new ElementRating(userRatings[index].ElementId, userRatings[index].Rating));
                var userRating = item.Ratings.First(r => r.ElementId == user.Id);
                userRating.Rating = 0;

                userRatings.RemoveAt(index);
            }

            //perform clustering with new items
            var clustering = new DbscanClustering<ClusterRatedItem>(items, calculateDistance);
            List<List<ClusterRatedItem>> clusters = clustering.FindClusters(0.13, 3);

            var predictMovies = new List<RecommendedMovie>();
            var userRatedMovieIds = userRatings.Where(r => r.Rating != 0).Select(u => u.ElementId).ToList();

            foreach (var partMovie in partMovies)
            {
                var currentMovie = items.FirstOrDefault(m => m.Id == partMovie.ElementId);
                if (currentMovie != null)
                {
                    var groupMovies = clusters.FirstOrDefault(cl => cl.Select(c => c.Id).Contains(currentMovie.Id));

                    if (groupMovies != null)
                    {
                        var userRatedMovies = groupMovies.Where(m => userRatedMovieIds.Contains(m.Id)).ToList();


                        double sum1 = 0, sum2 = 0;
                        foreach (var movie in userRatedMovies)
                        {
                            double similarity = calculateDistance(currentMovie, movie);
                            int currentRating = user.Ratings.First(m => m.ElementId == movie.Id).Rating;

                            sum1 += currentRating*similarity;
                            sum2 += Math.Abs(similarity);
                        }

                        predictMovies.Add(new RecommendedMovie(sum1 / sum2, currentMovie.Id));

                    }
                }
            }

            //calculate MAE
            double mae = 0;
            int count = 0;

            for (int i = 0; i < partMovies.Count; i++)
            {
                if (!double.IsInfinity(predictMovies[i].Rating) && !double.IsNaN(predictMovies[i].Rating))
                {
                    mae += Math.Abs(predictMovies[i].Rating - partMovies[i].Rating);
                    count++;
                }
            }

            return mae / count;
        }

         public double? CalculateMAEForUserByItemClustering2(int movieId,
             CalculateDistanceDelegate<ClusterRatedItem> calculateDistance,
            CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity,
             List<ClusterRatedItem> items, int numberOfClusters, int? mcp = null, double? eps = null)
         {
             var movie = items.SingleOrDefault(u => u.Id == movieId);
             if (movie == null)
             {
                 return null;
             }

             List<ElementRating> originalMovieRatings = movie.Ratings.Select(r => new ElementRating(r.ElementId, r.Rating)).ToList();

             List<ElementRating> movieRatings = movie.Ratings.Where(r => r.Rating != 0).ToList();

             var partUsers = new List<ElementRating>();
             int partSize = (int)Math.Ceiling(movieRatings.Count * 0.3);

             var rn = new Random();

             for (int i = 0; i < partSize; i++)
             {
                 int index = rn.Next(0, movieRatings.Count - 1);
                 partUsers.Add(new ElementRating(movieRatings[index].ElementId, movieRatings[index].Rating));

                 movieRatings[index].Rating = 0;
                 movieRatings.RemoveAt(index);
             }

             //perform clustering with new user
             List<List<ClusterRatedItem>> clusters;
             if (mcp.HasValue && eps.HasValue)
             {
                 var clustering = new DbscanClustering<ClusterRatedItem>(items, calculateDistance);
                 clusters = clustering.FindClusters(eps.Value, mcp.Value);
             }
             else
             {
                 var clustering = new KMedoidsClustering<ClusterRatedItem>(items, calculateDistance);
                 clusters = clustering.FindClusters(numberOfClusters);
             }

             List<ClusterRatedItem> groupMovies = clusters.FirstOrDefault(cl => cl.Select(u => u.Id).Contains(movie.Id));

             if (groupMovies == null)
             {
                 return null;
             }

             var predictMovies = PredictRatingsByItemClustering(movie, groupMovies, calculateSimilarity,
                 partUsers.Select(m => m.ElementId).ToList()).OrderBy(m => m.Id).ToList();

             partUsers = partUsers.OrderBy(m => m.ElementId).ToList();

             double mae = 0;
             int count = 0;

             for (int i = 0; i < partUsers.Count; i++)
             {
                 if (!double.IsInfinity(predictMovies[i].Rating) && !double.IsNaN(predictMovies[i].Rating))
                 {
                     mae += Math.Abs(predictMovies[i].Rating - partUsers[i].Rating);
                     count++;
                 }
             }

             movie.Ratings = originalMovieRatings;
             
             return mae / count;
         }

         public List<RecommendedMovie> PredictRatingsByItemClustering(ClusterRatedItem movie,
             List<ClusterRatedItem> groupMovies, CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity, 
             List<int> userIds)
         {
             var predictMovies = new List<RecommendedMovie>();

             for (int i = 0; i < userIds.Count; i++)
             {
                 var userId = userIds[i];
                 var currentGroupMovies = groupMovies
                                         .Where(u => u.Ratings.First(
                                             r => r.ElementId == userId).Rating != 0).ToList();

                 double sum1 = 0, sum2 = 0;

                 foreach (var currentGroupMovie in currentGroupMovies)
                 {
                     double similarity = calculateSimilarity(movie, currentGroupMovie);
                     int currentRating = currentGroupMovie.Ratings.First(r => r.ElementId == userId).Rating;

                     if (currentRating != 0)
                     {
                         sum1 += (currentRating)*similarity;
                         sum2 += Math.Abs(similarity);
                     }
                 }

                 predictMovies.Add(new RecommendedMovie(sum1 / sum2, userIds[i]));
             }

             return predictMovies;
         }
    }
}
