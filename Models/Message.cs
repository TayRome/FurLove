using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using DogDay;
 
namespace DogDay.Models
{
    public class Message : BaseEntity
    {
        public int MessageId { get; set; }
        public string MessageContent { get; set; }
        public int SenderId { get; set; }

        public Dog Sender{get;set;}
        public int ReceiverId { get; set; }

        public Dog Receiver{get;set;}

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}