using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MovieRatingCalculator.DataAccess;

namespace MovieRatingCalculator.Web.Admin.DataSpaceHelpers
{
    public class DataSpaceHelper
    {
        public static DataTable GetRatingsForExport(List<MovieRating> ratings)
        {
            var userIds = ratings.Select(r => r.UserId).Distinct().ToList();
            var movieIds = ratings.Select(r => r.MovieId).Distinct().ToList();
            var resultTable = new DataTable();
            resultTable.TableName = "DataSpace";

            var firstColumn = new DataColumn {DataType = typeof (int), ColumnName = "Users"};
            resultTable.Columns.Add(firstColumn);
            foreach (var column in movieIds.Select(movieId =>
                                                   new DataColumn
                                                       {
                                                           DataType = typeof (int),
                                                           ColumnName = movieId.ToString()
                                                       }))
            {
                resultTable.Columns.Add(column);
            }

            foreach (var userId in userIds)
            {
                var row = resultTable.NewRow();
                row[0] = userId;
                resultTable.Rows.Add(row);
            }

            foreach (var rating in ratings)
            {
                int rowId = userIds.IndexOf(rating.UserId);
                int colId = movieIds.IndexOf(rating.MovieId) + 1;
                resultTable.Rows[rowId][colId] = rating.Rating;
            }

            return resultTable;
        }
    }
}