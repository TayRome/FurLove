using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;
 
namespace DogDay.Models
{
    public class Family : BaseEntity
    {
        public int FamilyId { get; set; }
        public int DogId { get; set; }
        public int HumanId { get; set; }
        public Human Human { get; set; }
    }
}