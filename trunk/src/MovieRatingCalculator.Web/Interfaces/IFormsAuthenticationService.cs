using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieRatingCalculator.Web.Interfaces
{
    public interface IFormsAuthenticationService
    {
        void SetAuthCookie(string userName, bool createPersistentCookie);
        void SignOut();
    }
}