using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;


namespace DogDay.Models

{
    public class LoginViewModel : BaseEntity
    {
        
        [Display(Name="Email: ")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        
        [Display(Name="Password: ")]
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}