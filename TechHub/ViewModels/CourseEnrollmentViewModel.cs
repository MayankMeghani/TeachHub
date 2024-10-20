namespace TeachHub.ViewModels
{
    public class CourseEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StudentCount { get; set; }

        public bool isActive { get; set; }
    }
}
