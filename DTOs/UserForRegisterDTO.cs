using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(24, MinimumLength = 4, ErrorMessage = "you must specify a password of length greater than 4.")]
        public string Password { get; set; }
    }
}