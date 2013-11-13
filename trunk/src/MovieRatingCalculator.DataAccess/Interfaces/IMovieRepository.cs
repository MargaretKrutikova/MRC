using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.DataAccess.Dto;

namespace MovieRatingCalculator.DataAccess.Interfaces
{
    public interface IMovieRepository
    {
        void Add(Movie movie);
        List<Movie> FindMovies(string searchString, short? releaseYear, string email,
                               int page, int pageSize, ref int moviesCount);

        List<Movie> FindSuggestedMovies(string email, int page, int pageSize,
                                        ref int moviesCount, short? releaseYear = null);
        void AddMovieList(List<Movie> movieList);
        void ChangeMovieRating(MovieRating movieRate);
        DtoRatedMovie UpdateRatedMovieInfo(MovieRating movieRate);
        void DeleteMovieRating(MovieRating movieRate);

        List<Movie> GetMoviesWithRatings();
        Movie GetMovieStats(int movieId);
        List<string> GetKinopoiskMovieIds();
        List<DtoRatedMovie> GetUserRatedMovies(string email, short? releaseYear = null);
        int[] GetMovieRatingDistribution(int movieId);

        List<ClusterRatedItem> GetMoviesRatingsForClustering();
        List<short> GetMovieClusterIdsByMovieIds(List<int> movieIds);
        List<ClusterRatedItem> GetMoviesRatingsByClusterIds(List<short> clusterIds);
        List<ClusterRatedItem> GetMoviesRatingsByMovieIds(List<int> movieIds);

        void UpdateClusters(List<List<ClusterRatedItem>> clusters);
        List<Movie> GetMoviesByIds(List<int> movieIds);
    }
}
