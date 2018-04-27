using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DogDay;
 
namespace DogDay.Models
{
    public class Dog : BaseEntity
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
        public string PhotoPath { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        
        [NotMapped]
        public double MatchPercent { get; set; }

        [NotMapped]
        public double ReverseMatchPercent { get; set; }
        
        public List<DogInterest> Interests { get; set; }
        public List<Family> Humans { get; set; }
        public List<Cohab> Animals { get; set; }
        public List<Preference> Preferences { get; set; }
        
        [InverseProperty("DogBlocking")]
        public List<Block> BlockedDogs { get; set; }

        [InverseProperty("BlockedDog")]
        public List<Block> BlockingMe { get; set; }

        public Dog()
        {
            Interests = new List<DogInterest>();
            Humans = new List<Family>();
            Animals = new List<Cohab>();
            Preferences = new List<Preference>();
            BlockedDogs = new List<Block>();
            BlockingMe = new List<Block>();
        }
    }
}