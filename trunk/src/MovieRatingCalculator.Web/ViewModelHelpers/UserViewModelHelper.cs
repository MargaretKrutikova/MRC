using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.Web.ViewModels;

namespace MovieRatingCalculator.Web.ViewModelHelpers
{
    public class UserViewModelHelper
    {
        public static User PopulateUserFromUserViewModel(UserViewModel userViewModel)
        {
            return new User
                       {
                           Email = userViewModel.Email,
                           FirstName = userViewModel.FirstName,
                           LastName = userViewModel.LastName
                       };
        }

        public static User PopulateUserFromUserViewModel(UserViewModel userViewModel, string ipAddress)
        {
            var user = PopulateUserFromUserViewModel(userViewModel);
            user.UserLoginHistory = new List<UserLoginHistory> {PopulateUserLoginHistoryItem(ipAddress)};

            return user;
        }

        public static UserLoginHistory PopulateUserLoginHistoryItem(string ipAddress)
        {
            return new UserLoginHistory
                       {
                           IpAddress = ipAddress,
                           LoginDate = DateTime.Now
                       };
        }
    }
}