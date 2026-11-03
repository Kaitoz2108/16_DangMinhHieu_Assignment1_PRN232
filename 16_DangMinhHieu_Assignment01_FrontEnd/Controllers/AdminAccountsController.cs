using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

[Authorize(Roles = "Admin")]
public class AdminAccountsController : Controller
{
    private readonly IApiService _apiService;

    public AdminAccountsController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? role)
    {
        var accounts = await _apiService.GetAccountsAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            accounts = accounts.Where(a =>
                (a.AccountName != null && a.AccountName.ToLower().Contains(keyword)) ||
                (a.AccountEmail != null && a.AccountEmail.ToLower().Contains(keyword))
            ).ToList();
        }

        if (role.HasValue)
        {
            accounts = accounts.Where(a => a.AccountRole == role.Value).ToList();
        }

        ViewBag.SearchKeyword = search;
        ViewBag.SelectedRole = role;

        return View(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> GetAccount(short id)
    {
        var account = await _apiService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return NotFound(new { message = "Account not found." });
        }
        return Json(new
        {
            accountId = account.AccountId,
            accountName = account.AccountName,
            accountEmail = account.AccountEmail,
            accountRole = account.AccountRole
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SystemAccount account)
    {
        if (string.IsNullOrWhiteSpace(account.AccountName))
        {
            return BadRequest(new { message = "Account name is required." });
        }
        if (string.IsNullOrWhiteSpace(account.AccountEmail))
        {
            return BadRequest(new { message = "Account email is required." });
        }
        if (string.IsNullOrWhiteSpace(account.AccountPassword))
        {
            return BadRequest(new { message = "Password is required." });
        }
        if (!account.AccountRole.HasValue)
        {
            return BadRequest(new { message = "Role is required." });
        }

        var (success, data, error) = await _apiService.CreateAccountAsync(account);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to create account." });
        }

        return Json(new { success = true, message = "Account created successfully!", data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] SystemAccount account)
    {
        if (account.AccountId <= 0)
        {
            return BadRequest(new { message = "Invalid account ID." });
        }
        if (string.IsNullOrWhiteSpace(account.AccountName))
        {
            return BadRequest(new { message = "Account name is required." });
        }
        if (string.IsNullOrWhiteSpace(account.AccountEmail))
        {
            return BadRequest(new { message = "Account email is required." });
        }
        if (!account.AccountRole.HasValue)
        {
            return BadRequest(new { message = "Role is required." });
        }

        var (success, data, error) = await _apiService.UpdateAccountAsync(account.AccountId, account);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to update account." });
        }

        return Json(new { success = true, message = "Account updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(short id)
    {
        var (success, error) = await _apiService.DeleteAccountAsync(id);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to delete account." });
        }

        return Json(new { success = true, message = "Account deleted successfully!" });
    }
}
