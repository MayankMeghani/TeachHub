namespace TeachHub.ViewModels
{
    public class MyEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Teacher { get; set; }
        public DateTime EnrollDate { get; set; }
        public float Amount { get; set; } // Optional: Amount paid for the course
    }

}
