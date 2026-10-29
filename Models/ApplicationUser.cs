
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? CompanyName { get; set; }

    [StringLength(500)]
    [Display(Name = "Professional Summary")]
    public string? ProfileSummary { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? Education { get; set; }

    [StringLength(1000)]
    public string? Skills { get; set; }

    [StringLength(500)]
    public string? CvFileName { get; set; }

    [StringLength(1000)]
    public string? CvFilePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
