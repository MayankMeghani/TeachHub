using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class Course
    {
        public int CourseId { get; set; }  
        [Required]
        public string Title { get; set; }   
        [Required]
        public string Description { get; set; } 

        [Required] 
        public float Price { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } 

        public string TeacherId { get; set; } 
        public Teacher? Teacher { get; set; }
        
        public IEnumerable<Review>? Reviews { get; set; } 
        
        public IEnumerable<Enrollment> ?Enrollments { get; set; }

        public IEnumerable<Video>? Videos { get; set; }
        public bool IsActive { get; set; } = true; 

        public float Rating { get; set; }



    }
}
