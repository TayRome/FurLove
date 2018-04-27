using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;
 
namespace DogDay.Models
{
    public class SearchWrapper : BaseEntity
    {
        public IEnumerable<Dog> Dogs { get; set; }
        public List<Filter> Filters { get; set; }

        public SearchWrapper(IEnumerable<Dog> d, List<Filter> f){
            Dogs = d;
            Filters = f;
        }
    }
}