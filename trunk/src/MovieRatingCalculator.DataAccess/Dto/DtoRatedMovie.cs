using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieRatingCalculator.DataAccess.Dto
{
    public class DtoRatedMovie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public short ReleaseYear { get; set; }

        public int TotalRatesNumber { get; set; }
        public int UsersWithSameRating { get; set; }
        public short UsersRate { get; set; }
    }
}
