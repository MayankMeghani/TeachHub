using Microsoft.EntityFrameworkCore;
using TeachHub.Models;

namespace TeachHub.Data 
{
    public class TeachHubContext : DbContext
    {
        public TeachHubContext(DbContextOptions<TeachHubContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Learner> Learners { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; } 
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Teacher>()
                .HasKey(t => t.TeacherId);  

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.UserId)  // Foreign key to User
                .OnDelete(DeleteBehavior.Cascade);  // Cascading delete

            modelBuilder.Entity<Teacher>()
                .HasMany<Course>(t => t.Courses)
                .WithOne(c => c.Teacher)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);  // If a teacher is deleted, courses are also deleted

            modelBuilder.Entity<Learner>()
                .HasKey(l => l.LearnerId);  

            modelBuilder.Entity<Learner>()
                .HasOne(l => l.User)
                .WithOne()
                .HasForeignKey<Learner>(l => l.UserId)  // Foreign key to User
                .OnDelete(DeleteBehavior.Cascade);  // Cascading delete

            modelBuilder.Entity<Learner>()
                .HasMany<Review>(l => l.Reviews)
                .WithOne(r => r.Learner)
                .HasForeignKey(r => r.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);  // If a learner is deleted, their reviews are deleted

            
            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId);  

            modelBuilder.Entity<Course>()
                .HasMany<Review>(c => c.Reviews)
                .WithOne(r => r.Course)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);  // If a course is deleted, related reviews are also deleted

            modelBuilder.Entity<Review>()
                .HasKey(r => r.ReviewId);  // Primary key

            modelBuilder.Entity<Review>()
                .HasOne<Learner>(r => r.Learner)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.LearnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne<Course>(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasKey(e => new {e.LearnerId,e.CourseId});  

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);  

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Learner)
                .WithMany(l => l.Enrollments)
                .HasForeignKey(e => e.LearnerId)
                .OnDelete(DeleteBehavior.Restrict);   

            base.OnModelCreating(modelBuilder);
        }
    }
}
