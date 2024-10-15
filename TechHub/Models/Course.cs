using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class Course
    {
        public int CourseId { get; set; }  // Primary Key
        [Required]
        public string Title { get; set; }   // Title of the course
        [Required]
        public string Description { get; set; } // Description of the course

        [Required] 
        public float Price { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } // Date when the course was created

        public int TeacherId { get; set; } // Foreign Key for the Instructor
        public Teacher? Teacher { get; set; } // Navigation property
        
        public IEnumerable<Review>? Reviews { get; set; } // Navigation property for Reviews
        
        public IEnumerable<Enrollment> ?Enrollments { get; set; }

        public IEnumerable<Video>? Videos { get; set; } // Navigation property for Videos


    }
}
