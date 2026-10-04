using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;

    public AccountController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, data, error) = await _apiService.LoginAsync(model);
        if (!success || data == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Invalid email or password.");
            return View(model);
        }

        HttpContext.Session.SetString("JWToken", data.Token);
        HttpContext.Session.SetString("UserRole", data.Role);
        HttpContext.Session.SetString("UserName", data.AccountName);
        HttpContext.Session.SetString("AccountId", data.AccountId.ToString());

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, data.AccountId.ToString()),
            new Claim(ClaimTypes.Name, data.AccountName),
            new Claim(ClaimTypes.Email, data.AccountEmail),
            new Claim(ClaimTypes.Role, data.Role),
            new Claim("JWToken", data.Token)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (data.Role == "Admin")
        {
            return RedirectToAction("Index", "AdminAccounts");
        }
        if (data.Role == "Staff")
        {
            return RedirectToAction("Index", "NewsArticles");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var (success, data, error) = await _apiService.GetProfileAsync();
        if (!success || data == null)
        {
            TempData["ErrorMessage"] = error ?? "Unable to load profile.";
            return RedirectToAction("Index", "Home");
        }

        var model = new ProfileUpdateRequest
        {
            AccountName = data.AccountName ?? "",
            AccountEmail = data.AccountEmail ?? ""
        };

        ViewBag.AccountRole = data.RoleName;
        ViewBag.AccountId = data.AccountId;
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileUpdateRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _apiService.UpdateProfileAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update profile.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Profile");
    }

    [HttpGet]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> History()
    {
        var articles = await _apiService.GetMyHistoryAsync();
        return View(articles);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
