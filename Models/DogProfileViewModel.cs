using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;
 
namespace DogDay.Models
{
    public class DogProfileViewModel : BaseEntity
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }
        public string BodyType { get; set; }
        public string HighestEducation { get; set; }
        public string Barking { get; set; }
        public string Accidents { get; set; }
        public string Description { get; set; }
        public List<int> Interests { get; set; }
        public List<int> Humans { get; set; }
        public List<int> Animals { get; set; }
        public List<int> Preferences { get; set; }

        public DogProfileViewModel()
        {
            Age = 5;
            Interests = new List<int>();
            Humans = new List<int>();
            Animals = new List<int>();
            Preferences = new List<int>();
        }
    }
}