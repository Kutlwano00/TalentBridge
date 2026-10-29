
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Models;

namespace TalentBridge.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        var applicantCount = 0;
        var recruiterCount = 0;
        var adminCount = 0;

        foreach (var user in users)
        {
            if (await _userManager.IsInRoleAsync(user, "Applicant"))
                applicantCount++;

            if (await _userManager.IsInRoleAsync(user, "Recruiter"))
                recruiterCount++;

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                adminCount++;
        }

        ViewBag.TotalUsers = users.Count;
        ViewBag.ApplicantCount = applicantCount;
        ViewBag.RecruiterCount = recruiterCount;
        ViewBag.AdminCount = adminCount;
        ViewBag.TotalJobs = await _context.Jobs.CountAsync();
        ViewBag.TotalApplications =
            await _context.JobApplications.CountAsync();

        return View();
    }
}
