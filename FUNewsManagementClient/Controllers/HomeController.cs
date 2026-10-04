using System;
using System.Linq;
using System.Threading.Tasks;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;

    public HomeController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, short? categoryId)
    {
        var articles = await _apiService.GetNewsArticlesAsync("$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc");
        var categories = await _apiService.GetCategoriesAsync("$filter=IsActive eq true");

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

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            articles = articles.Where(a => a.CategoryId == categoryId.Value).ToList();
        }

        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = categoryId;
        ViewBag.SearchKeyword = search;

        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var article = await _apiService.GetNewsArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        return View(article);
    }

    public IActionResult Error()
    {
        return View();
    }
}
