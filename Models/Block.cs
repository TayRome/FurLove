using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DogDay;
 
namespace DogDay.Models
{
    public class Block : BaseEntity
    {
        public int BlockId { get; set; }
        public int DogBlockingId { get; set; }
        [ForeignKey ("DogBlockingId")]
        public Dog DogBlocking { get; set; }
        public int BlockedDogId { get; set; }
        [ForeignKey ("BlockedDogId")]
        public Dog BlockedDog { get; set; }

    }
}