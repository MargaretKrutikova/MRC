using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms
{
    public class ClusteringEvaluation
    {
        public List<double> SilhouetteCoefficientForClusters<T>(List<List<T>> clusters,
                                                                       CalculateDistanceDelegate<T>
                                                                           calculateDistanceFunction)
        {
            var silhouetteWidth = new List<double>();

            for (int i = 0; i < clusters.Count; i++)
            {
                double avgClusterCoeff = 0;
                if (clusters[i].Count == 1)
                {
                    continue;
                }

                foreach (var currentObj in clusters[i])
                {
                    double a = clusters[i].Select(o => calculateDistanceFunction(o, currentObj))
                        .Where(dist => !double.IsInfinity(dist)).Average(),
                           b = double.MaxValue;

                    for (int j = 0; j < clusters.Count; j++)
                    {
                        if (j != i)
                        {
                            var nearObjects = clusters[j].Select(o => calculateDistanceFunction(o, currentObj))
                                .Where(dist => !double.IsInfinity(dist)).ToList();

                            if (!nearObjects.Any())
                            {
                                continue;
                            }

                            double avgDiss = nearObjects.Average();
                            if (avgDiss < b)
                            {
                                b = avgDiss;
                            }
                        }
                    }

                    double scPoint = (b - a)/Math.Max(a, b);

                    if (double.IsNaN(scPoint))
                    {
                        continue;
                    }

                    avgClusterCoeff += scPoint;
                }

                avgClusterCoeff /= clusters[i].Count;
                silhouetteWidth.Add(avgClusterCoeff);
            }

            return silhouetteWidth;
        }

    }
}
