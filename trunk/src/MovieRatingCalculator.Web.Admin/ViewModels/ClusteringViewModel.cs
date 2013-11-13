using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieRatingCalculator.Web.Admin.ViewModels
{
    public enum DistanceType
    {
        PearsonCoefficient,
        CosDistance,
        SpearmanCoefficient
    }

    public enum ClusteringAlgType
    {
        AgglomerativeClustering,
        KMedoids,
        Dbscan
    }

    public enum ObjectType
    {
        Users,
        Movies
    }

    public class ClusteringViewModel
    {
        public DistanceType ClusteringDistanceType { get; set; }

        public ClusteringAlgType ClusteringAlgorithm { get; set; }

        public ObjectType ClusteringDataType { get; set; }

        [Range((typeof(int)), "0", "100", ErrorMessage = "Number of clusters must be between 2 and 100.")]
        [DisplayName("Number of clusters")]
        public int ClustersNumber { get; set; }

        [DisplayName("MCP for DBSCAN")]
        public int Mcp { get; set; }

        [DisplayName("Eps for DBSCAN")]
        public double Eps { get; set; }

        [Range((typeof(int)), "0", "1000", ErrorMessage = "Filter must be between 0 and 1000.")]
        [DisplayName("Filter for objects")]
        public int Filter { get; set; }

        public string Message { get; set; }
    }
}