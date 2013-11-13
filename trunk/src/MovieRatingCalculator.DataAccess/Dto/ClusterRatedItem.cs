using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.DataAccess.Dto
{
    public class ElementRating
    {
        public int ElementId { get; set; }
        public short Rating { get; set; }

        public ElementRating() { }

        public ElementRating(int id, short rating)
        {
            ElementId = id;
            Rating = rating;
        }
    }

    public class ClusterRatedItem
    {
        public int Id { get; set; }
        public List<ElementRating> Ratings { get; set; }
        public short ClusterId { get; set; }

        public ClusterRatedItem() { }

        public ClusterRatedItem(int id, List<ElementRating> ratings, short clusterId = 0)
        {
            Id = id;
            Ratings = ratings;
            ClusterId = clusterId;
        }
    }
}
