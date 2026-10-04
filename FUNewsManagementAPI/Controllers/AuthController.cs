using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
    [AllowAnonymous]
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
            request.Password == adminPassword)
        {
            var token = GenerateJwtToken(0, "Administrator", adminEmail, adminRole);
            return Ok(new LoginResponse
            {
                Token = token,
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

        var dbToken = GenerateJwtToken(account.AccountId, account.AccountName ?? "User", account.AccountEmail ?? "", role);

        return Ok(new LoginResponse
        {
            Token = dbToken,
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "User",
            AccountEmail = account.AccountEmail ?? "",
            Role = role
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(idClaim))
        {
            return Unauthorized();
        }

        if (short.TryParse(idClaim, out var accountId))
        {
            if (accountId == 0 && roleClaim == "Admin")
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

            var account = _accountRepository.GetById(accountId);
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

        return BadRequest(new { message = "Invalid account identifier." });
    }

    [HttpPut("profile")]
    [Authorize]
    public IActionResult UpdateProfile([FromBody] ProfileUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(idClaim) || !short.TryParse(idClaim, out var accountId))
        {
            return Unauthorized();
        }

        if (accountId == 0 && roleClaim == "Admin")
        {
            return BadRequest(new { message = "Default Admin account is configured via appsettings.json and cannot be edited in database." });
        }

        var existing = _accountRepository.GetById(accountId);
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

    private string GenerateJwtToken(short accountId, string name, string email, string role)
    {
        var keyStr = _configuration["Jwt:Key"] ?? "SuperSecretKeyForFUNewsManagementSystemPRN232Assignment1DangMinhHieu2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var expireHours = 24;
        if (int.TryParse(_configuration["Jwt:ExpireHours"], out var h))
        {
            expireHours = h;
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "FUNewsManagementAPI",
            audience: _configuration["Jwt:Audience"] ?? "FUNewsManagementClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
