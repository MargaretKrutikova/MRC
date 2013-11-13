using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using MovieRatingCalculator.Web.Interfaces;

namespace MovieRatingCalculator.Web.Services
{
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SetAuthCookie(string userName, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}