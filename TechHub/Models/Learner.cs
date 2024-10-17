namespace TeachHub.Models
{
    public class Learner : UserProfile
    {
        public string LearnerId { get; set; }
        public IEnumerable<Review>? Reviews { get; set; } // Navigation property for Reviews
        public IEnumerable<Enrollment> ?Enrollments { get; set; }
        public virtual User ?User { get; set; }

    }
}
