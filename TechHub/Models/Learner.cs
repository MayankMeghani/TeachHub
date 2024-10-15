namespace TeachHub.Models
{
    public class Learner : User
    {
        public int LearnerId { get; set; }
        public IEnumerable<Review>? Reviews { get; set; } // Navigation property for Reviews
        public IEnumerable<Enrollment> ?Enrollments { get; set; }
    }
}
