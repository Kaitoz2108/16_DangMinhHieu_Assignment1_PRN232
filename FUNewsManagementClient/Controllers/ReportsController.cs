using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using FUNewsManagementClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagementClient.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly IApiService _apiService;

    public ReportsController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var reportData = await _apiService.GetReportAsync(startDate, endDate);

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.TotalArticles = reportData.Count;
        ViewBag.ActiveCount = reportData.Count(a => a.NewsStatus == true);
        ViewBag.InactiveCount = reportData.Count(a => a.NewsStatus != true);

        return View(reportData);
    }
}
