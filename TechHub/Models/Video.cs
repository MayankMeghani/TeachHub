using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
        public class Video
        {
            public int VideoId { get; set; }  // Primary Key

            [Required]
            public string Title { get; set; }  // Title of the video

            [Required]
            public string VideoUrl { get; set; }  // URL of the uploaded video

            [Required]
            public DateTime UploadedAt { get; set; }  // Date when the video was uploaded

            public int CourseId { get; set; }  // Foreign Key for the Course
            public Course? Course { get; set; }  // Navigation property for Course
        }
    
}
