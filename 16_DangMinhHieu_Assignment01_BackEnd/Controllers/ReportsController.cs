using System;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly INewsArticleRepository _newsArticleRepository;

    public ReportsController(INewsArticleRepository newsArticleRepository)
    {
        _newsArticleRepository = newsArticleRepository;
    }

    [HttpGet]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _newsArticleRepository.GetReport(startDate, endDate);
        return Ok(report);
    }
}
