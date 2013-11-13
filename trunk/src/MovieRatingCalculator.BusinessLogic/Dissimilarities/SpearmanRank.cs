using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.BusinessLogic.Dissimilarities
{
    public class SpearmanRank
    {
        public int FirstRating { get; set; }
        public double FirstRatingRank { get; set; }

        public int SecondRating { get; set; }
        public double SecondRatingRank { get; set; }

        public SpearmanRank(int firstRating, double firstRank, int secondRating, double secondRank)
        {
            FirstRating = firstRating;
            FirstRatingRank = firstRank;

            SecondRating = secondRating;
            SecondRatingRank = secondRank;
        }
    }
}
