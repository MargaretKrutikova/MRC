namespace MovieRatingCalculator.BusinessLogic.ClusterItems
{
    public class DbscanClusterElement<T>
    {
        public T Data { get; set; }
        public int ClusterId { get; set; }
        public int ElementIndex { get; set; }

        public DbscanClusterElement(T data, int index, 
            int clusterId = (int)DbscanClusterElementType.Unclassified)
        {
            Data = data;
            ElementIndex = index;
            ClusterId = clusterId;
        }
    }

    public enum DbscanClusterElementType
    {
        Noise = -1,
        Unclassified = 0
    }
}
