using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DogDay;
 
namespace DogDay.Models
{
    public class Cohab : BaseEntity
    {
        public int CohabId { get; set; }
        public int DogId { get; set; }

        public Dog Dog { get; set; }

        [ForeignKey("Animal")]
        public int Animal_Id { get; set; }
        public Animal Animal { get; set; }
    }
}