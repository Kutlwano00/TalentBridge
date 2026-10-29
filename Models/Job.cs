
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models;

public class Job
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Job title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Employment type")]
    public string EmploymentType { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999999)]
    [Display(Name = "Minimum salary")]
    public decimal? SalaryMin { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Maximum salary")]
    public decimal? SalaryMax { get; set; }

    [Display(Name = "Date posted")]
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Application closing date")]
    public DateTime? ClosingDate { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = false;

    // The recruiter who owns this vacancy.
    public string? RecruiterId { get; set; }

    public ApplicationUser? Recruiter { get; set; }
}
