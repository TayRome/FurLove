using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DogDay;
using Microsoft.EntityFrameworkCore;

namespace DogDay.Models
{
    public class Human : BaseEntity
    {
        public int HumanId { get; set; }
        public string Relationship { get; set; }
    }
}