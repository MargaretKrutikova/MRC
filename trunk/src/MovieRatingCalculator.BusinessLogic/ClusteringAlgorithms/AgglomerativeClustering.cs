using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusterItems;
using MovieRatingCalculator.BusinessLogic.Interfaces;

namespace MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms
{
    public delegate double CalculateDistanceDelegate<in T>(T obj1, T obj2);

    public class AgglomerativeClustering<T> : IClusteringAlgorithm<T>
    {
        public DissimilarityMatrixElement[,] DissimilarityMatrix { get; set; }
        public bool[] ActiveClusters { get; set; }
       
        public List<List<DissimilarityMatrixElement>> PriorityDissimMatrix { get; set; }

        public List<int>[] ClusterVertices { get; set; }

        public List<T> Data { get; set; }
        public int Size { get; set; }
        public int NumberOfClusters { get; set; }

        public CalculateDistanceDelegate<T> CalculateDistance { get; set; }

        public AgglomerativeClustering(List<T> data, CalculateDistanceDelegate<T> calculateDistance)
        {
            Data = data;
            Size = data.Count;

            CalculateDistance = calculateDistance;          
        }

        void ResetClustering()
        {
            DissimilarityMatrix = new DissimilarityMatrixElement[Size, Size];
            ActiveClusters = new bool[Size];

            PriorityDissimMatrix = new List<List<DissimilarityMatrixElement>>();
            ClusterVertices = new List<int>[Size];
        }

        public void CalculateMatrixSimilarity()
        {
            ActiveClusters[Size - 1] = true;
            for (int i = 0; i < Size - 1; i++)
            {
                var priorityList = new List<DissimilarityMatrixElement>();
                ActiveClusters[i] = true;

                DissimilarityMatrix[i, i] = new DissimilarityMatrixElement(0, i);

                for (int j = i + 1; j < Size; j++)
                {
                    DissimilarityMatrix[i, j] = new DissimilarityMatrixElement(CalculateDistance(Data[i], Data[j]), j);

                    DissimilarityMatrix[j, i] = new DissimilarityMatrixElement(DissimilarityMatrix[i, j].Dissimilarity, i);
                    InsertNewMatrixElement(DissimilarityMatrix[i, j], priorityList);
                }

                PriorityDissimMatrix.Add(priorityList);
            }
        }

        public double CalculateSimAmongClasters(int k1, int k2)
        {
            if (ClusterVertices[k1] != null && ClusterVertices[k1].Any())
            {
                double maxDist = 0;
                for (int i = 0; i < ClusterVertices[k1].Count; i++)
                {
                    double dist = ClusterVertices[k2]
                        .Select(v => DissimilarityMatrix[ClusterVertices[k1][i], v].Dissimilarity).Max();

                    if (dist > maxDist)
                    {
                        maxDist = dist;
                    }
                }

                return maxDist;
            }

            return ClusterVertices[k2].Select(v => DissimilarityMatrix[k1, v].Dissimilarity).Max();//Math.Min(DissimMatrix[i, k1].Dissimilarity, DissimMatrix[i, k2].Dissimilarity);
        }

        public List<List<T>> FindClusters(int numberOfClusters)
        {
            NumberOfClusters = numberOfClusters;
            ResetClustering();

            CalculateMatrixSimilarity();

            for (int i = 0; i < Size - 1; i++)
            {
                Iteration();
                if (ActiveClusters.Count(elem => elem) == NumberOfClusters)
                {
                    break;
                }
            }

            var clusters = ActiveClusters.Select((active, ind) => new { Index = ind, Active = active })
                .Where(cl => cl.Active).Select(cl => new { Cluster = ClusterVertices[cl.Index], Vertex = cl.Index}).ToList();//ClusterVertices.Where(cl => cl != null && cl.Any()).ToList();

            var clustersElements = new List<List<T>>();
            for (int i = 0; i < clusters.Count; i++)
            {
                var currentCluster = new List<T>();
                if (clusters[i].Cluster != null && clusters[i].Cluster.Any())
                {
                    for (int j = 0; j < clusters[i].Cluster.Count; j++)
                    {
                        currentCluster.Add(Data[clusters[i].Cluster[j]]);
                    }
                }
                else
                {
                    currentCluster.Add(Data[clusters[i].Vertex]);
                }
                clustersElements.Add(currentCluster);
            }

            return clustersElements;
        }

        private void Iteration()
        {
            int k1 = FindNewClusterIndex();
            if (k1 == -1)
            {
                return;
            }

            int k2 = PriorityDissimMatrix[k1][0].VertexIndex;          
            ActiveClusters[k2] = false;

            var newVertices = new List<int> { k1, k2 };

            if (ClusterVertices[k2] != null)
            {
                newVertices.AddRange(ClusterVertices[k2]);
                ClusterVertices[k2] = null;
            }

            if (ClusterVertices[k1] == null)
            {
                ClusterVertices[k1] = new List<int>(newVertices);
            }
            else
            {
                ClusterVertices[k1].AddRange(newVertices);
            }
            ClusterVertices[k1] = ClusterVertices[k1].Distinct().ToList();

           //recalculate distances
            for (int i = 0; i < k1; i++)
            {
                PriorityDissimMatrix[i].Remove(DissimilarityMatrix[i, k1]);
                PriorityDissimMatrix[i].Remove(DissimilarityMatrix[i, k2]);
                if (ActiveClusters[i])
                {
                    DissimilarityMatrix[i, k1].Dissimilarity = CalculateSimAmongClasters(i, k1);
                    DissimilarityMatrix[k1, i].Dissimilarity = DissimilarityMatrix[i, k1].Dissimilarity;

                    InsertNewMatrixElement(DissimilarityMatrix[i, k1], PriorityDissimMatrix[i]);
                }
            }
            for (int i = k1 + 1; i < k2; i++)
            {
                PriorityDissimMatrix[i].Remove(DissimilarityMatrix[i, k2]);
            }

            var newPriorityList = new List<DissimilarityMatrixElement>();
            for (int j = k1 + 1; j < Size; j++)
            {
                if (j != k2 && ActiveClusters[j])
                {
                    DissimilarityMatrix[k1, j].Dissimilarity = CalculateSimAmongClasters(j, k1);
                    DissimilarityMatrix[j, k1].Dissimilarity = DissimilarityMatrix[k1, j].Dissimilarity;

                    InsertNewMatrixElement(DissimilarityMatrix[k1, j], newPriorityList);
                }
            }
            PriorityDissimMatrix[k1] = newPriorityList;

            if (k2 < PriorityDissimMatrix.Count)
            {
                PriorityDissimMatrix[k2].Clear();
            }

        }

        int FindNewClusterIndex()
        {
            double minSim = double.MaxValue;
            int index = -1;
            for (int i = 0; i < PriorityDissimMatrix.Count; i++)
            {
                if (PriorityDissimMatrix[i].Any() && PriorityDissimMatrix[i][0].Dissimilarity < minSim)
                {
                    minSim = PriorityDissimMatrix[i][0].Dissimilarity;
                    index = i;
                }
            }

            return index;
        }

        void InsertNewMatrixElement(DissimilarityMatrixElement elem, List<DissimilarityMatrixElement> list)
        {
            int i;
            for (i = 0; i < list.Count; i++)
            {
                if (elem.Dissimilarity < list[i].Dissimilarity)
                {
                    list.Insert(i, elem);
                    break;
                }
            }

            if (i == list.Count)
            {
                list.Add(elem);
            }
        }
    }
}
