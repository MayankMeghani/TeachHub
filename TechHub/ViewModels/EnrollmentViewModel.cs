namespace TeachHub.ViewModels
{
    public class EnrollmentViewModel
    {
        public string StripeToken { get; set; }
        public long Amount { get; set; }
        public int CourseId { get; set; }
        public int LearnerId { get; set; }
    }
}
