
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models.ViewModels;

public class JobViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(150)]
    [Display(Name = "Job title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(150)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select an employment type.")]
    [StringLength(50)]
    [Display(Name = "Employment type")]
    public string EmploymentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job description is required.")]
    [StringLength(5000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999999)]
    [Display(Name = "Minimum annual salary (ZAR)")]
    public decimal? SalaryMin { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Maximum annual salary (ZAR)")]
    public decimal? SalaryMax { get; set; }

    [Display(Name = "Application closing date")]
    [DataType(DataType.Date)]
    public DateTime? ClosingDate { get; set; }

    [Display(Name = "Publish immediately")]
    public bool IsPublished { get; set; }
}
