
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TalentBridge.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Notification>()
    .HasOne(notification => notification.User)
    .WithMany()
    .HasForeignKey(notification => notification.UserId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(notification => notification.JobApplication)
            .WithMany()
            .HasForeignKey(notification => notification.JobApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasIndex(notification => new
            {
                notification.UserId,
                notification.IsRead,
                notification.CreatedAt
            });
        base.OnModelCreating(builder);

        builder.Entity<Job>()
            .Property(job => job.SalaryMin)
            .HasPrecision(18, 2);

        builder.Entity<Job>()
            .Property(job => job.SalaryMax)
            .HasPrecision(18, 2);

        builder.Entity<Job>()
            .HasOne(job => job.Recruiter)
            .WithMany()
            .HasForeignKey(job => job.RecruiterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<JobApplication>()
            .HasIndex(application => new
            {
                application.JobId,
                application.ApplicantId
            })
            .IsUnique();

        builder.Entity<JobApplication>()
            .HasOne(application => application.Job)
            .WithMany()
            .HasForeignKey(application => application.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobApplication>()
            .HasOne(application => application.Applicant)
            .WithMany()
            .HasForeignKey(application => application.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

    }
    
}
