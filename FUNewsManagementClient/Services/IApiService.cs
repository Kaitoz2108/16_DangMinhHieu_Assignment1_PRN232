using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;

namespace FUNewsManagementClient.Services;

public interface IApiService
{
    // Auth
    Task<(bool Success, LoginResponse? Data, string? ErrorMessage)> LoginAsync(LoginRequest request);
    Task<(bool Success, SystemAccountDto? Data, string? ErrorMessage)> GetProfileAsync();
    Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(ProfileUpdateRequest request);

    // Categories
    Task<List<Category>> GetCategoriesAsync(string? odataQuery = null);
    Task<Category?> GetCategoryByIdAsync(short id);
    Task<(bool Success, Category? Data, string? ErrorMessage)> CreateCategoryAsync(Category category);
    Task<(bool Success, Category? Data, string? ErrorMessage)> UpdateCategoryAsync(short id, Category category);
    Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(short id);

    // Accounts
    Task<List<SystemAccount>> GetAccountsAsync(string? odataQuery = null);
    Task<SystemAccount?> GetAccountByIdAsync(short id);
    Task<(bool Success, SystemAccount? Data, string? ErrorMessage)> CreateAccountAsync(SystemAccount account);
    Task<(bool Success, SystemAccount? Data, string? ErrorMessage)> UpdateAccountAsync(short id, SystemAccount account);
    Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(short id);

    // News Articles
    Task<List<NewsArticle>> GetNewsArticlesAsync(string? odataQuery = null);
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
    Task<(bool Success, NewsArticle? Data, string? ErrorMessage)> CreateNewsArticleAsync(NewsArticleDto dto);
    Task<(bool Success, NewsArticle? Data, string? ErrorMessage)> UpdateNewsArticleAsync(string id, NewsArticleDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteNewsArticleAsync(string id);
    Task<List<NewsArticle>> GetMyHistoryAsync();
    Task<List<NewsArticle>> GetReportAsync(DateTime? startDate, DateTime? endDate);

    // Tags
    Task<List<Tag>> GetTagsAsync();
}
