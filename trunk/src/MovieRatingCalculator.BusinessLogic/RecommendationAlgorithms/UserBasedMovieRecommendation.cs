using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Interfaces;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Repository;

namespace MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms
{
    public class UserBasedMovieRecommendation : IRecommendationAlgorithm
    {
        public List<RecommendedMovie> GetTopNRecommendations(ClusterRatedItem user,
            CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity)
        {
            var groupUsers = (new UserRepository()).GetUsersByClusterId(user.ClusterId);
            if (!groupUsers.Any())
            {
                return null;
            }

            List<int> currentUserMoviesIds = user.Ratings.Where(r => r.Rating != 0)
                                                         .Select(r => r.ElementId).ToList();

            List<ElementRating> ratings = groupUsers.SelectMany(u => u.Ratings).ToList();
            List<ElementRating> movies = ratings.Where(m => m.Rating != 0 && !currentUserMoviesIds.Contains(m.ElementId)).ToList();

            List<int> movieIds = movies.Select(m => m.ElementId).Distinct().ToList();
           var popularMovieIds = (new MovieRepository()).GetMoviesRatingsByMovieIds(movieIds)
                .Where(m => m.Ratings.Count(r => r.Rating != 0) >= 10).Select(m => m.Id).ToList();

           var recommendedMovies = PredictRatingsByUserClustering(user, groupUsers, calculateSimilarity, popularMovieIds);

            return recommendedMovies.OrderByDescending(m => m.Rating).ToList();
        }

        public List<RecommendedMovie> PredictRatingsByUserClustering(ClusterRatedItem user, List<ClusterRatedItem> groupUsers,
            CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity, List<int> movieIds)
        {
            double avgRating = user.Ratings.Where(r => r.Rating != 0).Average(r => r.Rating);
            var predictMovies = new List<RecommendedMovie>();

            for (int i = 0; i < movieIds.Count; i++)
            {
                var currentGroupUsers = groupUsers
                                        .Where(u => u.Ratings.First(
                                            r => r.ElementId == movieIds[i]).Rating != 0).ToList();

                double sum1 = 0, sum2 = 0;

                foreach (var currentGroupUser in currentGroupUsers)
                {
                    double similarity = calculateSimilarity(user, currentGroupUser),
                           avgCurrentUserRating = currentGroupUser.Ratings.Where(r => r.Rating != 0).Average(r => r.Rating);
                    int currentRating = currentGroupUser.Ratings.First(r => r.ElementId == movieIds[i]).Rating;

                    if (currentRating != 0)
                    {
                        sum1 += (currentRating - avgCurrentUserRating)*similarity;
                        sum2 += Math.Abs(similarity);
                    }
                }

                predictMovies.Add(new RecommendedMovie(avgRating + sum1 / sum2, movieIds[i]));
            }

            return predictMovies;
        }

        public double? CalculateMAEForUserByUserClustering(int userId,
            CalculateDistanceDelegate<ClusterRatedItem> calculateDistance, 
            CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity,
            List<ClusterRatedItem> users, int numberOfClusters)
        {
            var user = users.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            List<ElementRating> originalUserRatings = user.Ratings.Select(r => new ElementRating(r.ElementId, r.Rating)).ToList();

            List<ElementRating> userRatings = user.Ratings.Where(r => r.Rating != 0).ToList();

            var partMovies = new List<ElementRating>();
            int partSize = (int)Math.Ceiling(userRatings.Count * 0.3);

            var rn = new Random();

            for (int i = 0; i < partSize; i++)
            {
                int index = rn.Next(0, userRatings.Count - 1);
                partMovies.Add(new ElementRating(userRatings[index].ElementId, userRatings[index].Rating));

                userRatings[index].Rating = 0;
                userRatings.RemoveAt(index);
            }

            //perform clustering with new user
            var clustering = new AgglomerativeClustering<ClusterRatedItem>(users, calculateDistance);
            List<List<ClusterRatedItem>> clusters = clustering.FindClusters(numberOfClusters);

            List<ClusterRatedItem> groupUsers = clusters.FirstOrDefault(cl => cl.Select(u => u.Id).Contains(user.Id));

            if (groupUsers == null)
            {
                return null;
            }

            var predictMovies = PredictRatingsByUserClustering(user, groupUsers, calculateSimilarity,
                partMovies.Select(m => m.ElementId).ToList()).OrderBy(m => m.Id).ToList();

            partMovies = partMovies.OrderBy(m => m.ElementId).ToList();

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

            user.Ratings = originalUserRatings;

            return mae / count;
        }
    }
}
