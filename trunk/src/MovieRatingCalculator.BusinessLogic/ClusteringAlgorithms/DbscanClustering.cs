using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.BusinessLogic.ClusterItems;
using MovieRatingCalculator.BusinessLogic.Interfaces;

namespace MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms
{
    public class DbscanClustering<T> 
    {
        public List<T> OriginalData { get; set; }
        public List<DbscanClusterElement<T>> Clusters { get; set; }
        public List<DbscanClusterElement<T>>[] DensityReachablePoints { get; set; }

        public double Eps { get; set; }
        public int MCP { get; set; }
        public int Size { get; set; }

        public CalculateDistanceDelegate<T> CalculateDistance { get; set; }

        public DbscanClustering(List<T> points, CalculateDistanceDelegate<T> calculateDistance)
        {
            Size = points.Count;
            OriginalData = points;
            CalculateDistance = calculateDistance;            
        }

        public void SetParemeters(double eps, int minP)
        {
            Eps = eps;
            MCP = minP;
        }

        private void SetDensityReachablePoints()
        {
            for (int i = 0; i < Size; i++)
            {
                DensityReachablePoints[Clusters[i].ElementIndex] = GetRegion(Clusters[i], Eps);
            }
        }

        public double[] Kdist(List<T> points, int k)
        {
            int size = points.Count;
            var kDistValues = new double[size];
            var distances = new List<List<double>>();

            for (int i = 0; i < size; i++)
            {
                var kDistances = new List<double>();
                for (int j = 0; j < size; j++)
                {
                    if (j != i)
                    {
                        kDistances.Add(CalculateDistance(points[i], points[j]));
                    }
                }
                distances.Add(kDistances.OrderBy(elem => elem).ToList());

                if (kDistances.Count > k - 1)
                {
                    kDistValues[i] = kDistances.OrderBy(elem => elem).Skip(k - 1).First();
                }
                else
                {
                    kDistValues[i] = double.MaxValue;
                }
            }

            return kDistValues;
        }

        public List<List<T>> FindClusters(double eps, int minP)
        {
            SetParemeters(eps, minP);

            Clusters = new List<DbscanClusterElement<T>>();
            for (int i = 0; i < Size; i++)
            {
                Clusters.Add(new DbscanClusterElement<T>(OriginalData[i], i));
            }

            DensityReachablePoints = new List<DbscanClusterElement<T>>[Size];
            SetDensityReachablePoints();

            int clusterId = 1;
            for (int i = 0; i < Clusters.Count; i++)
            {
                DbscanClusterElement<T> p = Clusters[i];
                if (p.ClusterId == (int)DbscanClusterElementType.Unclassified)
                {
                    if (ExpandCluster(p, clusterId))
                    {
                        clusterId++;
                    }
                }
            }
            // sort out points into their clusters, if any

            if (clusterId == 1)
                return new List<List<T>>(); // no clusters, so list is empty

            clusterId--;
            var clusters = new List<List<DbscanClusterElement<T>>>(clusterId);
            for (int i = 0; i < clusterId; i++)
            {
                clusters.Add(new List<DbscanClusterElement<T>>());
            }
            foreach (var p in Clusters)
            {
                if (p.ClusterId > 0)
                {
                    clusters[p.ClusterId - 1].Add(p);
                }
            }
            
            var clusterData = new List<List<T>>();

            foreach (var cl in clusters)
            {
                clusterData.Add(cl.Select(el => el.Data).ToList());
            }

            return clusterData;
        }

        private List<DbscanClusterElement<T>> GetRegion(DbscanClusterElement<T> p, double eps)
        {
            var region = new List<DbscanClusterElement<T>>();
            for (int i = 0; i < Clusters.Count; i++)
            {
                if (CalculateDistance(p.Data, Clusters[i].Data) <= eps)
                {
                    region.Add(Clusters[i]);
                }
            }
            return region;
        }

        private bool ExpandCluster(DbscanClusterElement<T> p, int clusterId)
        {
            List<DbscanClusterElement<T>> seeds = DensityReachablePoints[p.ElementIndex];//GetRegion(p, eps);
            if (seeds.Count < MCP) // no core point
            {
                p.ClusterId = (int)DbscanClusterElementType.Noise;
                return false;
            }
            // all points in seeds are density reachable from point 'p'

            for (int i = 0; i < seeds.Count; i++)
            {
                seeds[i].ClusterId = clusterId;
            }
            seeds.Remove(p);
            while (seeds.Any())
            {
                DbscanClusterElement<T> currentP = seeds[0];
                List<DbscanClusterElement<T>> result = DensityReachablePoints[currentP.ElementIndex];//GetRegion(currentP, eps);
                if (result.Count >= MCP)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        DbscanClusterElement<T> resultP = result[i];
                        if (resultP.ClusterId == (int)DbscanClusterElementType.Unclassified ||
                            resultP.ClusterId == (int)DbscanClusterElementType.Noise)
                        {
                            if (resultP.ClusterId == (int)DbscanClusterElementType.Unclassified)
                            {
                                seeds.Add(resultP);
                            }
                            resultP.ClusterId = clusterId;
                        }
                    }
                }
                seeds.Remove(currentP);
            }
            return true;
        }
    }
}
