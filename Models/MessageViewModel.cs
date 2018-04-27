using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;

namespace DogDay.Models

{
    public class MessageViewModel : BaseEntity
    {
        
        [Display(Name="Title: ")]
        [Required(ErrorMessage = "Give your message a title.")]
        public string MessageTitle { get; set; }

        [Display(Name="Message: ")]
        [Required(ErrorMessage = "You must have something to say!")]
        public string MessageContent { get; set; }

    }
}