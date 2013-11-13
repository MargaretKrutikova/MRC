using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieRatingCalculator.DataAccess.Dto;

namespace MovieRatingCalculator.BusinessLogic.Dissimilarities
{
    public class ElementsDissimilarities
    {
        public static double HighDimEuclideanDistance<T>(T p1, T p2)
            where T : ClusterRatedItem
        {
            double distance = 0;
            bool existCommonRatings = false;

            for (int i = 0; i < p1.Ratings.Count; i++)
            {
                if (p1.Ratings[i].Rating != 0 && p2.Ratings[i].Rating != 0)
                {
                    distance += Math.Pow(p1.Ratings[i].Rating - p2.Ratings[i].Rating, 2);
                    existCommonRatings = true;
                }
            }

            return existCommonRatings ? Math.Sqrt(distance) : double.PositiveInfinity;
        }

        public static double CosSimilarity<T>(T p1, T p2) where T : ClusterRatedItem
        {
            double distance = 0, len1 = 0, len2 = 0;
            bool existCommonRatings = false;

            for (int i = 0; i < p1.Ratings.Count; i++)
            {
                if (p1.Ratings[i].Rating != 0 && p2.Ratings[i].Rating != 0)
                {
                    distance += p1.Ratings[i].Rating * p2.Ratings[i].Rating;
                    len1 += p1.Ratings[i].Rating * p1.Ratings[i].Rating;
                    len2 += p2.Ratings[i].Rating * p2.Ratings[i].Rating;

                    existCommonRatings = true;
                }
            }

            return existCommonRatings ? distance / (Math.Sqrt(len1 * len2)) : double.PositiveInfinity;
        }

        public static double CosDistance<T>(T p1, T p2)
            where T : ClusterRatedItem
        {
            var cosSimilarity = CosSimilarity(p1, p2);

            return !double.IsInfinity(cosSimilarity) ? 1 - cosSimilarity : double.PositiveInfinity; 
        }

        public static double PearsonCorrelationDissimilarity<T>(T p1, T p2)
            where T : ClusterRatedItem
        {
            var pearsonCoefficient = PearsonCoefficient(p1, p2);
            if (double.IsInfinity(pearsonCoefficient))
            {
                return double.PositiveInfinity;
            }

            return (1 - pearsonCoefficient)/2; //1 - Math.Abs(pearsonCoefficient);//
        }

        public static double SpearmanCorrelationDissimilarity<T>(T p1, T p2)
            where T : ClusterRatedItem
        {
            var spearmanCoefficient = SpearmanRankCorrelation(p1, p2);
            if (double.IsInfinity(spearmanCoefficient))
            {
                return double.PositiveInfinity;
            }

            return 1 - Math.Abs(spearmanCoefficient);
            // (1 - spearmanCoefficient) / 2; ////
        }

        public static double SpearmanRankCorrelation<T>(T p1, T p2)
            where T : ClusterRatedItem
        {
            p1.Ratings = p1.Ratings.OrderBy(r => r.ElementId).ToList();
            p2.Ratings = p2.Ratings.OrderBy(r => r.ElementId).ToList();

            var ranks = new List<SpearmanRank>();
            int i, newRank = 1;
            for (i = 0; i < p1.Ratings.Count; i++)
            {
                if (p1.Ratings[i].Rating != 0 && p2.Ratings[i].Rating != 0)
                {
                    ranks.Add(new SpearmanRank(p1.Ratings[i].Rating, 0, p2.Ratings[i].Rating, 0));
                }
            }

            ranks = ranks.OrderBy(r => r.FirstRating).ToList();
            double avgFirst = 0, avgSecond = 0;
            i = 0;

            while (i < ranks.Count)
            {
                int j = i, currentRating = ranks[i].FirstRating, count = 0;
                double sum = 0;
                while (j < ranks.Count && ranks[j].FirstRating == currentRating)
                {
                    sum += newRank;
                    count++;
                    newRank++;
                    j++;
                }

                double currentRank = sum/count;

                for (int k = i; k < i + count; k++)
                {
                    avgFirst += currentRank;
                    ranks[k].FirstRatingRank = currentRank;
                }

                i += count;
            }

            ranks = ranks.OrderBy(r => r.SecondRating).ToList();
            i = 0;
            newRank = 1;
            while (i < ranks.Count)
            {
                int j = i, currentRating = ranks[i].SecondRating, count = 0;
                double sum = 0;
                while (j < ranks.Count && ranks[j].SecondRating == currentRating)
                {
                    sum += newRank;
                    count++;
                    newRank++;
                    j++;
                }

                double currentRank = sum/count;

                for (int k = i; k < i + count; k++)
                {
                    avgSecond += currentRank;
                    ranks[k].SecondRatingRank = currentRank;
                }

                i += count;
            }

            avgFirst /= ranks.Count;
            avgSecond /= ranks.Count;


            //calculate correlation

            double lenU = 0, lenV = 0, prMoment = 0, sum1, sum2;
            for (int j = 0; j < ranks.Count; j++)
            {
                sum1 = ranks[j].FirstRatingRank - avgFirst;
                sum2 = ranks[j].SecondRatingRank - avgSecond;

                prMoment += sum1*sum2;
                lenU += sum1*sum1;
                lenV += sum2*sum2;
            }

            double dist = prMoment/(Math.Sqrt(lenU*lenV));
            if (double.IsNaN(dist))
            {
                return double.PositiveInfinity;
            }

            return dist;

        }

        public static double PearsonCoefficient(ClusterRatedItem u, ClusterRatedItem v)
        {
            /* var commonRatings = u.Ratings
                     .Select((rating, ind) => new {firstRating = rating, secondRating = v.Ratings[ind]})
                     .Where(coratings => coratings.firstRating != 0 && coratings.secondRating != 0).ToList();

                 if (!commonRatings.Any())
                 {
                     return double.PositiveInfinity;
                 }

                 double avgU = 0, avgV = 0;
                 for (int i = 0; i < commonRatings.Count; i++)
                 {

                     avgU += (commonRatings[i].firstRating - 1)/(double) 9;
                     avgV += (commonRatings[i].secondRating - 1)/(double) 9;
                 }

                 avgU /= commonRatings.Count;
                 avgV /= commonRatings.Count;

                 double lenU = 0, lenV = 0, sum = 0, subU, subV;
                 for (int i = 0; i < commonRatings.Count; i++)
                 {
                     subU = (commonRatings[i].firstRating - 1)/(double) 9 - avgU;
                     subV = (commonRatings[i].secondRating - 1)/(double) 9 - avgV;

                     sum += subU*subV;
                     lenU += subU*subU;
                     lenV += subV*subV;
                 }

                 double dist = sum/(Math.Sqrt(lenU*lenV));
                 if (double.IsNaN(dist))
                 {
                     return double.PositiveInfinity;
                 }

                 return dist;*/

            int coRatedCount = 0;
            double avgU = 0, avgV = 0;
            for (int i = 0; i < u.Ratings.Count; i++)
            {
                if (u.Ratings[i].Rating != 0 && v.Ratings[i].Rating != 0)
                {
                    avgU += u.Ratings[i].Rating;
                    avgV += v.Ratings[i].Rating;
                    coRatedCount++;
                }
            }

            if (coRatedCount == 0)
            {
                return double.PositiveInfinity;
            }

            avgU /= coRatedCount;
            avgV /= coRatedCount;

            double lenU = 0, lenV = 0, sum = 0, subU, subV;
            for (int i = 0; i < u.Ratings.Count; i++)
            {
                if (u.Ratings[i].Rating != 0 && v.Ratings[i].Rating != 0)
                {
                    subU = u.Ratings[i].Rating - avgU;
                    subV = v.Ratings[i].Rating - avgV;

                    sum += subU * subV;
                    lenU += subU * subU;
                    lenV += subV * subV;
                }
            }

            double dist = sum/(Math.Sqrt(lenU*lenV));
            if (double.IsNaN(dist))
            {
                return double.PositiveInfinity;
            }

            return dist;

        }
    }
}

