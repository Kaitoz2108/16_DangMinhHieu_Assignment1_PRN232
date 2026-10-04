using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class NewsArticlesController : Controller
{
    private readonly IApiService _apiService;

    public NewsArticlesController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, short? categoryId, bool? status)
    {
        var articles = await _apiService.GetNewsArticlesAsync("$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc");
        var categories = await _apiService.GetCategoriesAsync("$filter=IsActive eq true");
        var tags = await _apiService.GetTagsAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            articles = articles.Where(a =>
                (a.NewsTitle != null && a.NewsTitle.ToLower().Contains(keyword)) ||
                (a.Headline != null && a.Headline.ToLower().Contains(keyword)) ||
                (a.NewsContent != null && a.NewsContent.ToLower().Contains(keyword)) ||
                (a.NewsSource != null && a.NewsSource.ToLower().Contains(keyword)) ||
                (a.Tags != null && a.Tags.Any(t => t.TagName != null && t.TagName.ToLower().Contains(keyword)))
            ).ToList();
        }

        if (categoryId.HasValue)
        {
            articles = articles.Where(a => a.CategoryId == categoryId.Value).ToList();
        }

        if (status.HasValue)
        {
            articles = articles.Where(a => a.NewsStatus == status.Value).ToList();
        }

        ViewBag.SearchKeyword = search;
        ViewBag.SelectedCategory = categoryId;
        ViewBag.SelectedStatus = status;
        ViewBag.Categories = categories;
        ViewBag.Tags = tags;

        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> GetArticle(string id)
    {
        var article = await _apiService.GetNewsArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound(new { message = "Article not found." });
        }

        var tagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new();

        return Json(new
        {
            newsArticleId = article.NewsArticleId,
            newsTitle = article.NewsTitle,
            headline = article.Headline,
            newsContent = article.NewsContent,
            newsSource = article.NewsSource,
            categoryId = article.CategoryId,
            newsStatus = article.NewsStatus,
            createdById = article.CreatedById,
            tagIds = tagIds
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] NewsArticleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Headline))
        {
            return BadRequest(new { message = "Headline is required." });
        }
        if (!dto.CategoryId.HasValue || dto.CategoryId <= 0)
        {
            return BadRequest(new { message = "Please select a Category." });
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var accountId) && accountId > 0)
        {
            dto.CreatedById = accountId;
        }

        var (success, data, error) = await _apiService.CreateNewsArticleAsync(dto);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to create news article." });
        }

        return Json(new { success = true, message = "News article created successfully!", data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] NewsArticleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewsArticleId))
        {
            return BadRequest(new { message = "Invalid article ID." });
        }
        if (string.IsNullOrWhiteSpace(dto.Headline))
        {
            return BadRequest(new { message = "Headline is required." });
        }
        if (!dto.CategoryId.HasValue || dto.CategoryId <= 0)
        {
            return BadRequest(new { message = "Please select a Category." });
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var accountId) && accountId > 0)
        {
            dto.UpdatedById = accountId;
        }

        var (success, data, error) = await _apiService.UpdateNewsArticleAsync(dto.NewsArticleId, dto);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to update news article." });
        }

        return Json(new { success = true, message = "News article updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _apiService.DeleteNewsArticleAsync(id);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to delete news article." });
        }

        return Json(new { success = true, message = "News article deleted successfully!" });
    }
}
