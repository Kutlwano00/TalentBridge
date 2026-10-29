
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Models;
using TalentBridge.Models.ViewModels;

namespace TalentBridge.Controllers;

[Authorize(Roles = "Applicant")]
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Applications
    // Displays applications belonging to the signed-in applicant.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var applicantId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(applicantId))
        {
            return Challenge();
        }

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Include(application => application.Job)
            .Where(application => application.ApplicantId == applicantId)
            .OrderByDescending(application => application.AppliedAt)
            .ToListAsync();

        return View(applications);
    }

    // GET: /Applications/Apply?jobId=1
    [HttpGet]
    public async Task<IActionResult> Apply(int? jobId)
    {
        if (jobId == null)
        {
            return NotFound();
        }

        var job = await FindOpenJobAsync(jobId.Value);

        if (job == null)
        {
            return NotFound();
        }

        var applicantId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(applicantId))
        {
            return Challenge();
        }

        var alreadyApplied = await _context.JobApplications
            .AnyAsync(application =>
                application.JobId == job.Id &&
                application.ApplicantId == applicantId);

        if (alreadyApplied)
        {
            TempData["ErrorMessage"] =
                "You have already applied for this vacancy.";

            return RedirectToAction(nameof(Index));
        }

        var model = new JobApplicationViewModel
        {
            JobId = job.Id,
            JobTitle = job.Title,
            CompanyName = job.CompanyName
        };

        return View(model);
    }

    // POST: /Applications/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(
        JobApplicationViewModel model)
    {
        var applicantId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(applicantId))
        {
            return Challenge();
        }

        // Always reload the job from the database.
        // Do not trust job details supplied by the browser.
        var job = await FindOpenJobAsync(model.JobId);

        if (job == null)
        {
            return NotFound();
        }

        var alreadyApplied = await _context.JobApplications
            .AnyAsync(application =>
                application.JobId == job.Id &&
                application.ApplicantId == applicantId);

        if (alreadyApplied)
        {
            TempData["ErrorMessage"] =
                "You have already applied for this vacancy.";

            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            model.JobTitle = job.Title;
            model.CompanyName = job.CompanyName;

            return View(model);
        }

        var application = new JobApplication
        {
            JobId = job.Id,
            ApplicantId = applicantId,
            CoverLetter = model.CoverLetter.Trim(),
            Status = "Pending",
            AppliedAt = DateTime.UtcNow
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Your application has been submitted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<Job?> FindOpenJobAsync(int jobId)
    {
        return await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(job =>
                job.Id == jobId &&
                job.IsPublished &&
                (job.ClosingDate == null ||
                 job.ClosingDate >= DateTime.UtcNow.Date));
    }
}
