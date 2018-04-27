using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;

namespace DogDay.Models
{
    	public class DogContext : DbContext
    	{
        // base() calls the parent class' constructor passing the "options" parameter along
        public DogContext(DbContextOptions<DogContext> options) : base(options) { }

        public DbSet<Animal> Animals { get; set; }
        public DbSet<Cohab> Cohabs { get; set; }
        public DbSet<Dog> dogs { get; set; }
        public DbSet<DogInterest> DogInterests { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<Human> Humans { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<Block> Blocks { get; set; }
    	
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>()
                .Property(d => d.AnimalId)
                .HasColumnName("AnimalId");
        }
        }

}
