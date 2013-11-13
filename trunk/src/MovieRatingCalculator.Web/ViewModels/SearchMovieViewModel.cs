using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieRatingCalculator.Web.ViewModels
{
    public class SearchMovieViewModel
    {
        #region Search properties
        public string FilterField { 
            get { return f; }
            set { f = value; }
        }
        public string f { get; set; }

        public short? ReleaseYear
        {
            get { return y; }
            set { y = value; }
        }

        [Range(typeof(short), "1930", "2012", ErrorMessage = "Year should be between 1930 and 2012.")]
        [RegularExpression(@"^\d+(\.\d)?$", ErrorMessage = "Invalid character.")]
        public short? y { get; set; }
        #endregion

        public List<MovieViewModel> SearchResultMovieList { get; set; }

        public bool IsUserLoggedIn { get; set; }
        public string act { get; set; }
        #region Paging properties

        public int? page { get; set; }
        public int CurrentPageIndex
        {
            get { return page.HasValue ? page.Value : 1; }
            set { page = value; }
        }

        public int PageSize { get; set; }
        public int TotalRecordCount { get; set; }
        public int NumericPageCount { get; set; }

        public int PageCount { get; private set; }
        public int StartPageIndex { get; private set; }       
        public int EndIndexPage{ get; private set; }
        #endregion

        public SearchMovieViewModel()
        {
            SearchResultMovieList = new List<MovieViewModel>();
        }

        public void PopulatePagedMovieList(List<MovieViewModel> movies, int pageSize, int totalRecordCount)
        {
            SearchResultMovieList = movies;

            TotalRecordCount = totalRecordCount;
            PageSize = pageSize;
            NumericPageCount = 5;
            PageCount = Math.Max(Convert.ToInt32(Math.Ceiling(TotalRecordCount/(float) PageSize)), 1);

            if ((PageCount <= NumericPageCount + 1) || CurrentPageIndex <= NumericPageCount - 1)
            {
                StartPageIndex = 1;
                EndIndexPage = PageCount <= NumericPageCount + 1 ? PageCount : NumericPageCount;
            }
            else
            {
                if (CurrentPageIndex > PageCount - NumericPageCount + 1)
                {
                    StartPageIndex = PageCount - NumericPageCount + 1;
                    EndIndexPage = PageCount;
                }
                else
                {
                    StartPageIndex = CurrentPageIndex - NumericPageCount/2;
                    EndIndexPage = CurrentPageIndex + NumericPageCount/2;
                }
            }
        }
    }
}