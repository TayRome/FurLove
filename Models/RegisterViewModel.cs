using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;

namespace DogDay.Models

{
    public class RegisterViewModel : BaseEntity
    {
        [Display(Name = "Name: ")]
        [Required(ErrorMessage = "Please tell us your name!")]
        public string Name { get; set; }

        [Display(Name = "Email Address: ")]
        [Required(ErrorMessage = "All dogs must have a valid email address.")]
        [EmailAddress]
        public string Email { get; set; }
 
        [Display(Name = "Password: ")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password must not be blank.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 digits long.")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password: ")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string PassConf { get; set; }
    }
}