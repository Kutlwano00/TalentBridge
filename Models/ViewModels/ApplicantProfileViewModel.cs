
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TalentBridge.Models.ViewModels;

public class ApplicantProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    [StringLength(500)]
    [Display(Name = "Professional summary")]
    public string? ProfileSummary { get; set; }

    [StringLength(200)]
    public string? Education { get; set; }

    [StringLength(1000)]
    [Display(Name = "Skills")]
    public string? Skills { get; set; }

    [Display(Name = "Upload CV (PDF)")]
    public IFormFile? CvFile { get; set; }

    public string? CvFileName { get; set; }
}
