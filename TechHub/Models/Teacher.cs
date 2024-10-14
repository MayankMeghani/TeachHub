using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; } // Primary Key

        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters.")]
        public string? Bio { get; set; }
        public ICollection<Course>? Courses { get; set; } // Navigation property for related courses

    }
}

