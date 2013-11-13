using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.BusinessLogic.Interfaces
{
    public interface IClusteringAlgorithm<T>
    {
        List<List<T>> FindClusters(int numbersOfClusters);
    }
}
