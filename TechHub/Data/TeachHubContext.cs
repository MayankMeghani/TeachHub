using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TeachHub.Models;

namespace TeachHub.Data 
{
    public class TeachHubContext : IdentityDbContext
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
        //public DbSet<User> Users { get; set; }
        public DbSet<Video> Videos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teacher>()
          .HasKey(t => t.TeacherId); // Set Id as the primary key

            modelBuilder.Entity<Teacher>()
                .HasOne<User>(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.TeacherId) // Id is a foreign key
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete

            // Configure the Learner entity
            modelBuilder.Entity<Learner>()
                .HasKey(l => l.LearnerId); // Set Id as the primary key

            modelBuilder.Entity<Learner>()
                .HasOne<User>(l => l.User)
                .WithOne(u => u.Learner)
                .HasForeignKey<Learner>(l => l.LearnerId) // Id is a foreign key
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete


            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId);  

            modelBuilder.Entity<Course>()
                .HasMany<Review>(c => c.Reviews)
                .WithOne(r => r.Course)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);  

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
