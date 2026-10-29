using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalentBridge.Models;
using TalentBridge.Models.ViewModels;

namespace TalentBridge.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        // Never allow public registration as Admin or any other role.
        if (model.AccountType != "Applicant" &&
            model.AccountType != "Recruiter")
        {
            ModelState.AddModelError(
                nameof(model.AccountType),
                "Please select a valid account type.");
        }

        // Recruiters must provide a company name.
        if (model.AccountType == "Recruiter" &&
            string.IsNullOrWhiteSpace(model.CompanyName))
        {
            ModelState.AddModelError(
                nameof(model.CompanyName),
                "Please enter your company name.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "An account with this email address already exists.");

            return View(model);
        }

        var isRecruiter = model.AccountType == "Recruiter";

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = model.FullName.Trim(),
            CompanyName = isRecruiter
                ? model.CompanyName?.Trim()
                : null,
            ProfileSummary = isRecruiter
                ? null
                : model.ProfileSummary?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(
            user,
            model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // Assign only the role explicitly selected from the
        // two permitted public registration options.
        var roleResult = await _userManager.AddToRoleAsync(
            user,
            model.AccountType);

        if (!roleResult.Succeeded)
        {
            // Avoid leaving an account without its intended role.
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        TempData["SuccessMessage"] = isRecruiter
            ? "Your recruiter account has been created successfully."
            : "Your TalentBridge account has been created successfully.";

        return RedirectToRoleDashboard(model.AccountType);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(
            model.Email.Trim());

        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Respect a valid local return URL.
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Send each account type to its appropriate dashboard.
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (await _userManager.IsInRoleAsync(user, "Recruiter"))
            {
                return RedirectToAction("Index", "Recruiter");
            }

            if (await _userManager.IsInRoleAsync(user, "Applicant"))
            {
                return RedirectToAction("Index", "Home");
            }

            // Fallback for accounts without an expected role.
            await _signInManager.SignOutAsync();

            ModelState.AddModelError(
                string.Empty,
                "Your account does not have an assigned role. Please contact support.");

            return View(model);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is temporarily locked. Please try again later.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid login attempt.");
        }

        return View(model);
    }

    private IActionResult RedirectToRoleDashboard(string role)
    {
        if (role == "Recruiter")
        {
            return RedirectToAction("Index", "Recruiter");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}