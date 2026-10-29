using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an account type.")]
    [RegularExpression(
        "^(Applicant|Recruiter)$",
        ErrorMessage = "Please select a valid account type.")]
    [Display(Name = "Account type")]
    public string AccountType { get; set; } = "Applicant";

    [StringLength(100)]
    [Display(Name = "Company name")]
    public string? CompanyName { get; set; }

    [StringLength(500)]
    [Display(Name = "Professional summary (optional)")]
    public string? ProfileSummary { get; set; }

    [Required(ErrorMessage = "Please create a password.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "Password must contain at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}