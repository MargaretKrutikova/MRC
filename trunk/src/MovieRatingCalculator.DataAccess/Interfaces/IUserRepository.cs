using System.Collections.Generic;
using MovieRatingCalculator.DataAccess.Dto;

namespace MovieRatingCalculator.DataAccess.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByEmail(string email);
        void Add(User user);
        void AddLoginHistoryItem(UserLoginHistory historyItem);
        
        List<User> GetUsersWithRatings();
        User GetUserStats(int userId);

        int GetNumberOfRatedMovies(string email);

        List<ClusterRatedItem> GetUsersByClusterId(short clusterId);
        ClusterRatedItem GetUserRatingsById(int userId);
        List<ClusterRatedItem> GetUsersRatingsForClustering(short? clusterId = null, 
                                                int? userId = null, string email = null);

        void UpdateClusters(List<List<ClusterRatedItem>> clusters);
        ClusterRatedItem GetUserRatingsByEmail(string email);
    }
}
