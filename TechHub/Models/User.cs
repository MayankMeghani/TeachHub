using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class User
    {
        [Required]public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }

    }
}
