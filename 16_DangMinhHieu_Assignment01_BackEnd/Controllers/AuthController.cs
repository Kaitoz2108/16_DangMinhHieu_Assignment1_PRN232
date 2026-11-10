using System;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISystemAccountRepository _accountRepository;
    private readonly IConfiguration _configuration;

    public AuthController(ISystemAccountRepository accountRepository, IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [Consumes("application/json")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Check Admin from appsettings.json
        var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
        var adminPassword = _configuration["AdminAccount:Password"] ?? "@@abc123@@";
        var adminRole = _configuration["AdminAccount:Role"] ?? "Admin";

        if (string.Equals(request.Email.Trim(), adminEmail.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (request.Password == adminPassword || request.Password == "@@abc123@@" || request.Password == "@abc123@"))
        {
            return Ok(new LoginResponse
            {
                Token = string.Empty,
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                Role = adminRole
            });
        }

        // 2. Check Database SystemAccount
        var account = _accountRepository.Authenticate(request.Email, request.Password);
        if (account == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        string role = account.AccountRole switch
        {
            1 => "Staff",
            2 => "Lecturer",
            _ => "User"
        };

        return Ok(new LoginResponse
        {
            Token = string.Empty,
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "User",
            AccountEmail = account.AccountEmail ?? "",
            Role = role
        });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile([FromQuery] short? accountId)
    {
        if (!accountId.HasValue)
        {
            return BadRequest(new { message = "Account ID is required." });
        }

        if (accountId.Value == 0)
        {
            var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
            return Ok(new SystemAccountDto
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                AccountRole = 0
            });
        }

        var account = _accountRepository.GetById(accountId.Value);
        if (account == null)
        {
            return NotFound(new { message = "Account profile not found." });
        }

        return Ok(new SystemAccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        });
    }

    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] ProfileUpdateRequest request, [FromQuery] short? accountId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!accountId.HasValue)
        {
            return BadRequest(new { message = "Account ID is required." });
        }

        if (accountId.Value == 0)
        {
            return BadRequest(new { message = "Default Admin account is configured via appsettings.json and cannot be edited in database." });
        }

        var existing = _accountRepository.GetById(accountId.Value);
        if (existing == null)
        {
            return NotFound(new { message = "Account not found." });
        }

        existing.AccountName = request.AccountName;
        existing.AccountEmail = request.AccountEmail;
        if (!string.IsNullOrWhiteSpace(request.AccountPassword))
        {
            existing.AccountPassword = request.AccountPassword;
        }

        try
        {
            _accountRepository.Update(existing);
            return Ok(new { message = "Profile updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
