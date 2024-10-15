namespace TeachHub.Models
{
    public class Enrollment
    {
        public int CourseId { get; set; }      // Foreign Key for Course
        public int LearnerId { get; set; }     // Foreign Key for Student
        public Course ?Course { get; set; }     // Navigation property for Course
        public Learner ?Learner { get; set; }   // Navigation property for Student
    }
}

