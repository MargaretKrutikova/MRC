namespace MovieRatingCalculator.BusinessLogic.ClusterItems
{
    public class DissimilarityMatrixElement
    {
        public double Dissimilarity { get; set; }
        public int VertexIndex { get; set; }

        public DissimilarityMatrixElement() { }

        public DissimilarityMatrixElement(double dissim, int ind)
        {
            Dissimilarity = dissim;
            VertexIndex = ind;
        }
    }
}
