using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms;
using MovieRatingCalculator.DataAccess.Dto;

namespace MovieRatingCalculator.BusinessLogic.Interfaces
{
    public interface IRecommendationAlgorithm
    {
        List<RecommendedMovie> GetTopNRecommendations(ClusterRatedItem user,
                          CalculateDistanceDelegate<ClusterRatedItem> calculateSimilarity);
    }
}
