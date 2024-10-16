using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class Enrollment
    {
        public int CourseId { get; set; }      // Foreign Key for Course
        public string LearnerId { get; set; }     // Foreign Key for Student
        public Course ?Course { get; set; }     // Navigation property for Course
        public Learner ?Learner { get; set; }   // Navigation property for Student

        [Required]
        public float Amount { get; set; }
        [Required]
        public string TransactionId { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}

