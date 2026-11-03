using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class CategoriesController : Controller
{
    private readonly IApiService _apiService;

    public CategoriesController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var categories = await _apiService.GetCategoriesAsync("$expand=ParentCategory");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            categories = categories.Where(c =>
                (c.CategoryName != null && c.CategoryName.ToLower().Contains(keyword)) ||
                (c.CategoryDesciption != null && c.CategoryDesciption.ToLower().Contains(keyword))
            ).ToList();
        }

        ViewBag.SearchKeyword = search;
        ViewBag.ParentCategories = categories.Where(c => c.IsActive == true).ToList();

        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategory(short id)
    {
        var category = await _apiService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        return Json(new
        {
            categoryId = category.CategoryId,
            categoryName = category.CategoryName,
            categoryDesciption = category.CategoryDesciption,
            parentCategoryId = category.ParentCategoryId,
            isActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            return BadRequest(new { message = "Category Name is required." });
        }
        if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
        {
            return BadRequest(new { message = "Category Description is required." });
        }

        var (success, data, error) = await _apiService.CreateCategoryAsync(category);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to create category." });
        }

        return Json(new { success = true, message = "Category created successfully!", data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] Category category)
    {
        if (category.CategoryId <= 0)
        {
            return BadRequest(new { message = "Invalid category ID." });
        }
        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            return BadRequest(new { message = "Category Name is required." });
        }
        if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
        {
            return BadRequest(new { message = "Category Description is required." });
        }

        var (success, data, error) = await _apiService.UpdateCategoryAsync(category.CategoryId, category);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to update category." });
        }

        return Json(new { success = true, message = "Category updated successfully!" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(short id)
    {
        var (success, error) = await _apiService.DeleteCategoryAsync(id);
        if (!success)
        {
            return BadRequest(new { message = error ?? "Failed to delete category." });
        }

        return Json(new { success = true, message = "Category deleted successfully!" });
    }
}
