using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class SystemAccountDAO
{
    private static SystemAccountDAO? _instance = null;
    private static readonly object _instanceLock = new object();

    public static SystemAccountDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new SystemAccountDAO();
                    }
                }
            }
            return _instance;
        }
    }

    private SystemAccountDAO() { }

    public List<SystemAccount> GetAll()
    {
        using var context = new FUNewsManagementDbContext();
        return context.SystemAccounts
            .AsNoTracking()
            .ToList();
    }

    public SystemAccount? GetById(short id)
    {
        using var context = new FUNewsManagementDbContext();
        return context.SystemAccounts
            .Include(a => a.NewsArticles)
            .AsNoTracking()
            .FirstOrDefault(a => a.AccountId == id);
    }

    public SystemAccount? GetByEmail(string email)
    {
        using var context = new FUNewsManagementDbContext();
        return context.SystemAccounts
            .AsNoTracking()
            .FirstOrDefault(a => a.AccountEmail != null && a.AccountEmail.ToLower() == email.Trim().ToLower());
    }

    public SystemAccount? Authenticate(string email, string password)
    {
        using var context = new FUNewsManagementDbContext();
        return context.SystemAccounts
            .AsNoTracking()
            .FirstOrDefault(a => a.AccountEmail != null &&
                                 a.AccountEmail.ToLower() == email.Trim().ToLower() &&
                                 a.AccountPassword == password);
    }

    public SystemAccount Create(SystemAccount account)
    {
        using var context = new FUNewsManagementDbContext();

        // Check duplicate email
        if (!string.IsNullOrWhiteSpace(account.AccountEmail))
        {
            var exists = context.SystemAccounts.Any(a => a.AccountEmail != null && a.AccountEmail.ToLower() == account.AccountEmail.Trim().ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"An account with email '{account.AccountEmail}' already exists.");
            }
        }

        // Manual generation (Max(AccountID) + 1) requirement
        short nextId = 1;
        if (context.SystemAccounts.Any())
        {
            var maxId = context.SystemAccounts.Max(a => (int)a.AccountId);
            nextId = (short)(maxId + 1);
        }

        account.AccountId = nextId;
        account.NewsArticles.Clear();

        context.SystemAccounts.Add(account);
        context.SaveChanges();
        return account;
    }

    public SystemAccount Update(SystemAccount account)
    {
        using var context = new FUNewsManagementDbContext();
        var existing = context.SystemAccounts.FirstOrDefault(a => a.AccountId == account.AccountId);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Account with ID {account.AccountId} not found.");
        }

        // If email changed, ensure uniqueness
        if (!string.IsNullOrWhiteSpace(account.AccountEmail) &&
            !string.Equals(existing.AccountEmail, account.AccountEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = context.SystemAccounts.Any(a => a.AccountId != account.AccountId &&
                                                              a.AccountEmail != null &&
                                                              a.AccountEmail.ToLower() == account.AccountEmail.Trim().ToLower());
            if (emailExists)
            {
                throw new InvalidOperationException($"An account with email '{account.AccountEmail}' already exists.");
            }
        }

        existing.AccountName = account.AccountName;
        existing.AccountEmail = account.AccountEmail;
        if (!string.IsNullOrWhiteSpace(account.AccountPassword))
        {
            existing.AccountPassword = account.AccountPassword;
        }
        if (account.AccountRole.HasValue)
        {
            existing.AccountRole = account.AccountRole;
        }

        context.SaveChanges();
        return existing;
    }

    public (bool Success, string Message) Delete(short accountId)
    {
        using var context = new FUNewsManagementDbContext();
        var account = context.SystemAccounts
            .Include(a => a.NewsArticles)
            .FirstOrDefault(a => a.AccountId == accountId);

        if (account == null)
        {
            return (false, "Account not found.");
        }

        // Delete Restriction: CANNOT delete an account if NewsArticles.Any(a => a.CreatedByID == accountId)
        if (account.NewsArticles.Any() || context.NewsArticles.Any(n => n.CreatedById == accountId))
        {
            return (false, "Cannot delete this account because it has created existing news articles.");
        }

        context.SystemAccounts.Remove(account);
        context.SaveChanges();
        return (true, "Account deleted successfully.");
    }
}
