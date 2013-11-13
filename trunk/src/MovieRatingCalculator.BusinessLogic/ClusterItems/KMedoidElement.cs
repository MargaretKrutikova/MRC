namespace MovieRatingCalculator.BusinessLogic.ClusterItems
{
    public class KMedoidElement<T>
    {
        public T Data { get; set; }
        public int Index { get; set; }
        public int MedoidIndex { get; set; }
        public bool IsMedoid { get; set; }
        public double SumDistances { get; set; }

        public KMedoidElement() { }

        public KMedoidElement(T data, int ind)
        {
            Data = data;
            IsMedoid = false;
            Index = ind;
            SumDistances = 0;
        }
    }
}
