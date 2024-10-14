using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? ProfilePicture { get; set; }


        [Required]
        public UserRole Role {  get; set; } 
        public enum UserRole
        {
            Teacher = 1,
            Learner = 2
        }


    }
}
