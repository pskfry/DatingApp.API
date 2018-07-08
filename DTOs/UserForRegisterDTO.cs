using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        public string UserName { get; set; }
        [StringLength(24, MinimumLength = 4, ErrorMessage = "You must specify a password of length greater than 4.")]
        public string Password { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Gender { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public UserForRegisterDTO()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}