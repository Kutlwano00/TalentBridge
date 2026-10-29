
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Models;

namespace TalentBridge.Controllers;

[Authorize(Roles = "Applicant")]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        ViewBag.DebugUserId = userId;
        ViewBag.DebugNotificationCount = notifications.Count;

        return View(notifications);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id && n.UserId == userId);

        if (notification == null)
        {
            return NotFound();
        }

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Notification marked as read.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = _userManager.GetUserId(User);

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "All notifications have been marked as read.";

        return RedirectToAction(nameof(Index));
    }
}
