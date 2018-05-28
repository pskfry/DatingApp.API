using System;

namespace DatingApp.API.Models
{
    public class Photo
    {
        public string Url { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool isMainPhoto { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}