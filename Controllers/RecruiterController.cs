using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Models;
using Microsoft.AspNetCore.Hosting;
using TalentBridge.Models.ViewModels;

namespace TalentBridge.Controllers;

[Authorize(Roles = "Recruiter,Admin")]
public class RecruiterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    // GET: /Recruiter/Applications/5
    [HttpGet]
    public async Task<IActionResult> Applications(int id)
    {
        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound();
        }

        // Recruiters may only view applicants for their own vacancies.
        // Administrators may view applicants for any vacancy.
        if (!CanManageJob(job))
        {
            return Forbid();
        }

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Include(a => a.Applicant)
            .Where(a => a.JobId == id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        ViewBag.JobId = job.Id;
        ViewBag.JobTitle = job.Title;
        ViewBag.CompanyName = job.CompanyName;

        return View(applications);
    }


    // POST: /Recruiter/UpdateApplicationStatus
  

    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> UpdateApplicationStatus(
    int applicationId,
    string status)
    {
        var allowedStatuses = new[]
        {
        "Pending",
        "Under Review",
        "Shortlisted",
        "Rejected",
        "Accepted"
    };

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(status))
        {
            TempData["ErrorMessage"] =
                "Please select a valid application status.";

            return RedirectToAction(nameof(Index));
        }

        var application = await _context.JobApplications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
        {
            return NotFound();
        }

        if (!CanManageJob(application.Job))
        {
            return Forbid();
        }

        var previousStatus = application.Status;

        if (previousStatus == status)
        {
            TempData["SuccessMessage"] =
                "The application already has that status.";

            return RedirectToAction(
                nameof(Applications),
                new { id = application.JobId });
        }

        // Update the application status.
        application.Status = status;

        // Create a notification for the applicant.
        var notification = new Notification
        {
            UserId = application.ApplicantId,
            Title = "Application Status Updated",
            Message = $"Your application for '{application.Job.Title}' " +
                      $"at {application.Job.CompanyName} has changed " +
                      $"from '{previousStatus}' to '{status}'.",
            Type = status,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            JobApplicationId = application.Id
        };

        _context.Notifications.Add(notification);

        // Save both changes to the database.
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Application status updated to {status}. " +
            "An applicant notification has been created.";

        return RedirectToAction(
            nameof(Applications),
            new { id = application.JobId });
    }



    private static readonly string[] EmploymentTypes =
    {
        "Full-time",
        "Part-time",
        "Contract",
        "Internship",
        "Learnership",
        "Remote"
    };

    public RecruiterController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }
    [HttpGet]
    public async Task<IActionResult> DownloadApplicantCv(int applicationId)
    {
        // Load the application, vacancy and applicant.
        var application = await _context.JobApplications
            .AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
        {
            return NotFound("Application not found.");
        }

        // Verify that this recruiter owns the vacancy.
        // Your existing helper also allows Admin users.
        if (!CanManageJob(application.Job))
        {
            return Forbid();
        }

        var applicant = application.Applicant;

        if (string.IsNullOrWhiteSpace(applicant.CvFilePath))
        {
            return NotFound("This applicant has not uploaded a CV.");
        }

        // CvFilePath contains the generated stored filename.
        // Never accept a physical path from the request.
        var storedFileName = Path.GetFileName(applicant.CvFilePath);

        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "PrivateUploads",
            "CVs",
            storedFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("The applicant's CV file could not be found.");
        }

        // Use the original filename only as the download name.
        var downloadName = string.IsNullOrWhiteSpace(applicant.CvFileName)
            ? "Applicant-CV.pdf"
            : Path.GetFileName(applicant.CvFileName);

        return PhysicalFile(
            filePath,
            "application/pdf",
            downloadName);
    }

    // GET: /Recruiter
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var jobs = _context.Jobs
            .AsNoTracking()
            .AsQueryable();

        // Admins can see every vacancy.
        // Recruiters can see only their own vacancies.
        if (!User.IsInRole("Admin"))
        {
            jobs = jobs.Where(job => job.RecruiterId == userId);
        }

        var results = await jobs
            .OrderByDescending(job => job.PostedAt)
            .ToListAsync();

        return View(results);
    }

    // GET: /Recruiter/Create
    [HttpGet]
    public IActionResult Create()
    {
        SetEmploymentTypes();
        return View(new JobViewModel());
    }

    // POST: /Recruiter/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobViewModel model)
    {
        ValidateJob(model);

        if (!ModelState.IsValid)
        {
            SetEmploymentTypes();
            return View(model);
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var job = new Job
        {
            Title = model.Title.Trim(),
            CompanyName = model.CompanyName.Trim(),
            Location = model.Location.Trim(),
            EmploymentType = model.EmploymentType,
            Description = model.Description.Trim(),
            SalaryMin = model.SalaryMin,
            SalaryMax = model.SalaryMax,
            ClosingDate = model.ClosingDate.HasValue
                ? DateTime.SpecifyKind(
                    model.ClosingDate.Value.Date,
                    DateTimeKind.Utc)
                : null,
            PostedAt = DateTime.UtcNow,
            IsPublished = model.IsPublished,
            RecruiterId = userId
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = job.IsPublished
            ? "Your vacancy has been published successfully."
            : "Your vacancy has been saved as a draft.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Recruiter/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(job => job.Id == id.Value);

        if (job == null)
        {
            return NotFound();
        }

        if (!CanManageJob(job))
        {
            return Forbid();
        }

        SetEmploymentTypes();

        var model = new JobViewModel
        {
            Id = job.Id,
            Title = job.Title,
            CompanyName = job.CompanyName,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            Description = job.Description,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            ClosingDate = job.ClosingDate,
            IsPublished = job.IsPublished
        };

        return View(model);
    }

    // POST: /Recruiter/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        JobViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var job = await _context.Jobs
            .FirstOrDefaultAsync(job => job.Id == id);

        if (job == null)
        {
            return NotFound();
        }

        if (!CanManageJob(job))
        {
            return Forbid();
        }

        ValidateJob(model);

        if (!ModelState.IsValid)
        {
            SetEmploymentTypes();
            return View(model);
        }

        // Update editable fields only.
        // Keep RecruiterId and PostedAt unchanged.
        job.Title = model.Title.Trim();
        job.CompanyName = model.CompanyName.Trim();
        job.Location = model.Location.Trim();
        job.EmploymentType = model.EmploymentType;
        job.Description = model.Description.Trim();
        job.SalaryMin = model.SalaryMin;
        job.SalaryMax = model.SalaryMax;
        job.ClosingDate = model.ClosingDate.HasValue
            ? DateTime.SpecifyKind(
                model.ClosingDate.Value.Date,
                DateTimeKind.Utc)
            : null;
        job.IsPublished = model.IsPublished;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "The vacancy has been updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Recruiter/TogglePublication/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublication(int id)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(job => job.Id == id);

        if (job == null)
        {
            return NotFound();
        }

        if (!CanManageJob(job))
        {
            return Forbid();
        }

        if (!job.IsPublished)
        {
            if (job.ClosingDate.HasValue &&
                job.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            {
                TempData["ErrorMessage"] =
                    "This vacancy has an expired closing date. " +
                    "Edit the vacancy before publishing it.";

                return RedirectToAction(nameof(Index));
            }

            job.IsPublished = true;
            TempData["SuccessMessage"] =
                "The vacancy has been published.";
        }
        else
        {
            job.IsPublished = false;
            TempData["SuccessMessage"] =
                "The vacancy has been unpublished.";
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /Recruiter/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var job = await _context.Jobs
            .FirstOrDefaultAsync(job => job.Id == id);

        if (job == null)
        {
            return NotFound();
        }

        if (!CanManageJob(job))
        {
            return Forbid();
        }

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "The vacancy has been deleted.";

        return RedirectToAction(nameof(Index));
    }

    private bool CanManageJob(Job job)
    {
        return User.IsInRole("Admin") ||
               job.RecruiterId == _userManager.GetUserId(User);
    }

    private void SetEmploymentTypes()
    {
        ViewBag.EmploymentTypes = EmploymentTypes;
    }

    private void ValidateJob(JobViewModel model)
    {
        if (!EmploymentTypes.Contains(model.EmploymentType))
        {
            ModelState.AddModelError(
                nameof(model.EmploymentType),
                "Please select a valid employment type.");
        }

        if (model.SalaryMin.HasValue &&
            model.SalaryMax.HasValue &&
            model.SalaryMin.Value > model.SalaryMax.Value)
        {
            ModelState.AddModelError(
                nameof(model.SalaryMax),
                "Maximum salary must be greater than or equal to minimum salary.");
        }

        if (model.ClosingDate.HasValue &&
            model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(
                nameof(model.ClosingDate),
                "The closing date cannot be in the past.");
        }
    }
}