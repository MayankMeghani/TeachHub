using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class Review
    {
        public int ReviewId { get; set; } // Primary Key
        
        [Required]
        public string Content { get; set; } // Review content

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } // Rating out of 5
        
        [Required]
        public DateTime CreatedAt { get; set; } // Review date

        public int CourseId { get; set; } // Foreign Key for the Course
        public Course? Course { get; set; } // Navigation property for the Course
        public string LearnerId { get; set; } // Foreign Key for the Student (Reviewer)
        public Learner? Learner { get; set; } // Navigation property for the Student
    }
}
