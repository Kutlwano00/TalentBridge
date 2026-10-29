
using System.ComponentModel.DataAnnotations;

namespace TalentBridge.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(50)]
    public string Type { get; set; } = "Information";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? JobApplicationId { get; set; }

    public JobApplication? JobApplication { get; set; }
}
