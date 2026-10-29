
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required]
    public int JobId { get; set; }

    public Job Job { get; set; } = null!;

    [Required]
    public string ApplicantId { get; set; } = string.Empty;

    public ApplicationUser Applicant { get; set; } = null!;

    [Required]
    [StringLength(3000, MinimumLength = 20,
        ErrorMessage = "Your cover letter must contain between 20 and 3000 characters.")]
    [Display(Name = "Cover letter")]
    public string CoverLetter { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
