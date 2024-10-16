namespace TeachHub.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public List<EnrollmentTransactionViewModel> Enrollments { get; set; }
    }

}
