
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models.ViewModels;

public class JobApplicationViewModel
{
    public int JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please write a cover letter.")]
    [StringLength(3000, MinimumLength = 20,
        ErrorMessage = "Your cover letter must contain between 20 and 3000 characters.")]
    [Display(Name = "Cover letter")]
    public string CoverLetter { get; set; } = string.Empty;
}
