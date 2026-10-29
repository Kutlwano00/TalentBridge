
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using TalentBridge.Models;
using TalentBridge.Models.ViewModels;

namespace TalentBridge.Controllers;

[Authorize(Roles = "Applicant")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
     UserManager<ApplicationUser> userManager,
     ApplicationDbContext context,
     IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCv()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null ||
            string.IsNullOrWhiteSpace(user.CvFilePath))
        {
            return NotFound("No CV has been uploaded.");
        }

        var safeFileName = Path.GetFileName(user.CvFilePath);

        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "PrivateUploads",
            "CVs",
            safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("The CV file could not be found.");
        }

        var downloadName = string.IsNullOrWhiteSpace(user.CvFileName)
            ? "CV.pdf"
            : Path.GetFileName(user.CvFileName);

        return PhysicalFile(
            filePath,
            "application/pdf",
            downloadName);
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var model = new ApplicantProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Location = user.Location,
            ProfileSummary = user.ProfileSummary,
            Education = user.Education,
            Skills = user.Skills,
            CvFileName = user.CvFileName
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        ApplicantProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        // Validate the uploaded CV.
        if (model.CvFile != null)
        {
            var extension = Path.GetExtension(model.CvFile.FileName);

            if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.CvFile),
                    "Please upload your CV as a PDF file.");
            }

            if (model.CvFile.Length == 0 ||
                model.CvFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.CvFile),
                    "Your CV must be a non-empty file no larger than 5 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.CvFileName = user.CvFileName;
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.Location = model.Location;
        user.ProfileSummary = model.ProfileSummary;
        user.Education = model.Education;
        user.Skills = model.Skills;


        if (model.CvFile != null)
        {
            var extension = Path.GetExtension(model.CvFile.FileName);

            if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.CvFile),
                    "Please upload a PDF file.");
            }

            if (model.CvFile.Length == 0 ||
                model.CvFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.CvFile),
                    "The CV must be non-empty and no larger than 5 MB.");
            }

            if (ModelState.IsValid)
            {
                // Store CVs outside wwwroot so they cannot be accessed directly.
                var uploadsFolder = Path.Combine(
                    _environment.ContentRootPath,
                    "PrivateUploads",
                    "CVs");

                Directory.CreateDirectory(uploadsFolder);

                // Use a generated filename instead of the original filename.
                var storedFileName = $"{Guid.NewGuid():N}.pdf";
                var filePath = Path.Combine(uploadsFolder, storedFileName);

                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    await model.CvFile.CopyToAsync(stream);
                }

                // Delete the previous CV only after the new file is saved.
                if (!string.IsNullOrWhiteSpace(user.CvFilePath))
                {
                    var oldFilePath = Path.Combine(
                        uploadsFolder,
                        Path.GetFileName(user.CvFilePath));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.CvFileName = Path.GetFileName(model.CvFile.FileName);
                user.CvFilePath = storedFileName;
            }
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.CvFileName = user.CvFileName;
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your profile has been updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}
