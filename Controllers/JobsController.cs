
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Models;

namespace TalentBridge.Controllers;

public class JobsController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Jobs
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? location,
        string? employmentType)
    {
        var jobs = _context.Jobs
            .AsNoTracking()
            .Where(job =>
                job.IsPublished &&
                (job.ClosingDate == null ||
                 job.ClosingDate >= DateTime.UtcNow));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            jobs = jobs.Where(job =>
                job.Title.Contains(searchTerm) ||
                job.CompanyName.Contains(searchTerm) ||
                job.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            location = location.Trim();

            jobs = jobs.Where(job =>
                job.Location.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(employmentType))
        {
            jobs = jobs.Where(job =>
                job.EmploymentType == employmentType);
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Location = location;
        ViewBag.EmploymentType = employmentType;

        ViewBag.EmploymentTypes = new[]
        {
            "Full-time",
            "Part-time",
            "Contract",
            "Internship",
            "Learnership",
            "Remote"
        };

        var results = await jobs
            .OrderByDescending(job => job.PostedAt)
            .ToListAsync();

        return View(results);
    }

    // GET: /Jobs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(job =>
                job.Id == id.Value &&
                job.IsPublished &&
                (job.ClosingDate == null ||
                 job.ClosingDate >= DateTime.UtcNow));

        if (job == null)
        {
            return NotFound();
        }

        return View(job);
    }
}
