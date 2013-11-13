using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusterItems;
using MovieRatingCalculator.BusinessLogic.Interfaces;

namespace MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms
{
    public class KMedoidsClustering<T> : IClusteringAlgorithm<T>
    {
        public List<KMedoidElement<T>> Data { get; set; }
        public List<KMedoidElement<T>> Medoids { get; set; }
        public List<T> OriginalData { get; set; }
        public int K { get; set; }

        public double[,] DistanceMatrix { get; set; }

        public CalculateDistanceDelegate<T> CalculateDistance { get; set; }

        public KMedoidsClustering(List<T> data, CalculateDistanceDelegate<T> calculateDistance)
        {
            OriginalData = data;
            CalculateDistance = calculateDistance;                   
        }

        void ResetClustering()
        {
            Data = OriginalData.Select((p, ind) => new KMedoidElement<T>(p, ind)).ToList();
            DistanceMatrix = new double[Data.Count, Data.Count];
        }

        public List<List<T>> FindClusters(int numberOfclusters)
        {
            K = numberOfclusters;

            ResetClustering();
            InitializeClusterization();

            Medoids = SelectInitialMedoidsPAMAlg(K);
            AssignObjectsToMedoids();

            int count = K;
            while (count != 0)
            {
                count = Iteration();
                AssignObjectsToMedoids();
                // count++;
            }

            var clustersIds = Medoids.Select(m => m.MedoidIndex).ToList();
            var clusters = new List<List<T>>();

            foreach (var clusterId in clustersIds)
            {
                var clusterElements = Data.Where(p => p.MedoidIndex == clusterId).Select(p => p.Data).ToList();
                clusters.Add(clusterElements);
            }

            return clusters;
        }

        void InitializeClusterization()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                DistanceMatrix[i, i] = 0;
                for (int j = i + 1; j < Data.Count; j++)
                {
                    DistanceMatrix[i, j] = CalculateDistance(Data[i].Data, Data[j].Data);
                    DistanceMatrix[j, i] = DistanceMatrix[i, j];

                    Data[i].SumDistances += DistanceMatrix[i, j];
                    Data[j].SumDistances += DistanceMatrix[i, j];
                }
            }
        }

        void AssignObjectsToMedoids()
        {
            if (!Medoids.Any())
            {
                return;
            }

            for (int i = 0; i < Data.Count; i++)
            {
                if (!Data[i].IsMedoid)
                {
                    Data[i].MedoidIndex = Medoids.Select(el => new
                    {
                        index = el.Index,
                        dist = DistanceMatrix[i, el.Index]
                    }).OrderBy(e => e.dist).First().index;
                }
            }
        }

        int Iteration()
        {
            var newMedoids = new List<KMedoidElement<T>>();
            int newCount = 0;
            for (int i = 0; i < K; i++)
            {
                var clusterElements = Data.Where(p => p.MedoidIndex == Medoids[i].Index).ToList();
                double minDist = clusterElements.Select(el => DistanceMatrix[el.Index, Medoids[i].Index]).Sum();

                int index = -1;

                for (int j = 0; j < clusterElements.Count; j++)
                {
                    if (!clusterElements[j].IsMedoid)
                    {
                        double dist = clusterElements.Select(el => DistanceMatrix[clusterElements[j].Index, el.Index]).Sum();

                        if (dist < minDist)
                        {
                            minDist = dist;
                            index = j;
                        }
                    }
                }

                //Change medoid
                if (index != -1)
                {
                    newCount++;
                    Medoids[i].IsMedoid = false;
                    newMedoids.Add(clusterElements[index]);
                }
                else
                {
                    newMedoids.Add(Medoids[i]);
                }

            }

            //after
            for (int i = 0; i < K; i++)
            {
                newMedoids[i].IsMedoid = true;
                newMedoids[i].MedoidIndex = newMedoids[i].Index;
            }

            Medoids = newMedoids;

            return newCount;
        }


        List<KMedoidElement<T>> SelectInitialMedoidsPAMAlg(int k)
        {
            var elements = Data.ToList();
            var medoids = new List<KMedoidElement<T>>();
            var firstMedoid = elements.OrderBy(el => el.SumDistances).First();

            elements.Remove(firstMedoid);
            firstMedoid.MedoidIndex = firstMedoid.Index;
            firstMedoid.IsMedoid = true;
            medoids.Add(firstMedoid);

            for (int l = 1; l < k; l++)
            {
                double maxDist = 0;
                var nextMedoid = new KMedoidElement<T>();

                for (int i = 0; i < elements.Count; i++)
                {
                    double c = 0;
                    for (int j = 0; j < elements.Count; j++)
                    {
                        var distances = medoids
                            .Select((el, ind) => DistanceMatrix[elements[j].Index, medoids[ind].Index])
                            .Where(dist => !double.IsInfinity(dist)).ToList();
                        if (!distances.Any())
                        {
                            continue;
                        }
                        double d = distances.Min();

                        if (j != i)
                        {
                            if (!double.IsInfinity(DistanceMatrix[elements[i].Index, elements[j].Index]))
                            {
                                c += Math.Max(d - DistanceMatrix[elements[i].Index, elements[j].Index], 0);
                            }
                        }
                    }

                    if (c > maxDist)
                    {
                        maxDist = c;
                        nextMedoid = elements[i];
                    }

                }
                medoids.Add(nextMedoid);
                nextMedoid.MedoidIndex = nextMedoid.Index;
                nextMedoid.IsMedoid = true;
                elements.Remove(nextMedoid);

            }

            return medoids;
        }
    }
}
