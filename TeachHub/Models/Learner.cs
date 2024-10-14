namespace TeachHub.Models
{
    public class Learner
    {
        public int LearnerId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public IEnumerable<Review>? Reviews { get; set; } // Navigation property for Reviews
        public IEnumerable<Enrollment> ?Enrollments { get; set; }
    }
}
