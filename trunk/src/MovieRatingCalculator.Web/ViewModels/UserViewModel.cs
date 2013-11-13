using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieRatingCalculator.Web.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$",
            ErrorMessage = "Email is not correct")]
        
        public string Email { get; set; }

        [Display(Name = "First Name")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Enter only alphabets")]
        [StringLength(50, ErrorMessage = "{0} should be less than {1} characters")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Enter only alphabets")]
        [StringLength(50, ErrorMessage = "{0} should be less than {1} characters")]
        public string LastName { get; set; } 
    }
}