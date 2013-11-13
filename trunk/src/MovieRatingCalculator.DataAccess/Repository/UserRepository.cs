using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Interfaces;

namespace MovieRatingCalculator.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        public User GetUserByEmail(string email)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Users.FirstOrDefault(u => u.Email == email);
            }
        }

        public void Add(User user)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        public void AddLoginHistoryItem(UserLoginHistory historyItem)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                context.UserLoginHistory.Add(historyItem);
                context.SaveChanges();
            }
        }

        public List<User> GetUsersWithRatings()
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.Users.Include(u => u.MovieRatings).Include(u => u.UserLoginHistory).ToList();
            }
        }

        public User GetUserStats(int userId)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return
                    context.Users
                           .Include(u => u.UserLoginHistory)
                           .Include(u => u.MovieRatings.Select(r => r.Movie))
                           .SingleOrDefault(u => u.Id == userId);
            }
        }

        public int GetNumberOfRatedMovies(string email)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                return context.MovieRatings.Count(r => r.User.Email == email);
            }
        }

        public List<ClusterRatedItem> GetUsersByClusterId(short clusterId)
        {
            return GetUsersRatingsForClustering(clusterId);
        }

        public ClusterRatedItem GetUserRatingsById(int userId)
        {
            return GetUsersRatingsForClustering(null, userId).SingleOrDefault();
        }

        public ClusterRatedItem GetUserRatingsByEmail(string email)
        {
            return GetUsersRatingsForClustering(null, null, email).SingleOrDefault();
        }

        public List<ClusterRatedItem> GetUsersRatingsForClustering(short? clusterId = null, int? userId = null, string email = null)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var allmovies = context.Movies.Where(u => u.MovieRatings.Any())
                    .Select(u => new ElementRating { ElementId = u.Id,
                                                     Rating = 0 }).ToList();
                
                IQueryable<User> query = context.Users.Where(m => m.MovieRatings.Any());

                if (clusterId != null)
                {
                    query = query.Where(u => u.ClusterId != null && (short)u.ClusterId == clusterId);
                }

                if (userId != null)
                {
                    query = query.Where(u => u.Id == userId);
                }
                else
                {
                    if (email != null)
                    {
                        query = query.Where(u => u.Email == email);
                    }
                }

                var users = 
                    query.GroupJoin(context.MovieRatings, m => m.Id, mr => mr.UserId,(user, userRatings) => new
                    {
                        Id = user.Id,
                        ClusterId = user.ClusterId,
                        Ratings = userRatings.Select(r => new
                        {
                            ElementId = r.MovieId,
                            Rating = r.Rating,
                        })
                    }).ToList();

                var ratings = new List<ClusterRatedItem>();

                foreach (var user in users)
                {
                    var allRatings = allmovies.GroupJoin(user.Ratings, m => m.ElementId, u => u.ElementId,
                                                      (m, u) => u.Select(mr => new ElementRating(m.ElementId, mr.Rating))
                                                                 .DefaultIfEmpty(new ElementRating(m.ElementId, 0))
                                                                 .Single()).OrderBy(r => r.ElementId).ToList();

                    ratings.Add(new ClusterRatedItem(user.Id, allRatings, user.ClusterId ?? 0));
                }

                return ratings;
            }
        }

        public void UpdateClusters(List<List<ClusterRatedItem>> clusters)
        {
            using (var context = new MovieRatingCalculatorEntities())
            {
                var allUsers = context.Users.Where(u => u.MovieRatings.Any())
                                            .ToList();
                short clusterId = 1;
                var clusterUserIds = new List<int>();

                foreach (var cluster in clusters)
                {
                    var ids = cluster.Select(u => u.Id).ToList();
                    clusterUserIds.AddRange(ids);

                    var clusterUsers = allUsers.Where(u => ids.Contains(u.Id)).ToList();

                    foreach (var clusterUser in clusterUsers)
                    {
                        clusterUser.ClusterId = clusterId;
                        context.Entry(clusterUser).State = EntityState.Modified;
                    }

                    clusterId++;
                }

                var usersWithNoCluster = allUsers.Where(u => !clusterUserIds.Contains(u.Id)).ToList();

                foreach (var user in usersWithNoCluster)
                {
                    user.ClusterId = null;
                    context.Entry(user).State = EntityState.Modified;
                }

                context.SaveChanges();
            }
        }
    }
}
