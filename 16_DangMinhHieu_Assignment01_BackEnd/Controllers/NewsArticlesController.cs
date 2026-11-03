using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

public class NewsArticlesController : ODataController
{
    private readonly INewsArticleRepository _newsArticleRepository;
    private readonly ITagRepository _tagRepository;

    public NewsArticlesController(INewsArticleRepository newsArticleRepository, ITagRepository tagRepository)
    {
        _newsArticleRepository = newsArticleRepository;
        _tagRepository = tagRepository;
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        // Guest (No login): Can view active news articles (NewsStatus == true / 1)
        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        bool isStaffOrAdmin = isAuthenticated && (User.IsInRole("Staff") || User.IsInRole("Admin"));

        var articles = _newsArticleRepository.GetAll(includeInactive: isStaffOrAdmin).AsQueryable();
        return Ok(articles);
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get([FromRoute] string key)
    {
        var article = _newsArticleRepository.GetById(key);
        if (article == null)
        {
            return NotFound($"News article with ID {key} not found.");
        }

        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        bool isStaffOrAdmin = isAuthenticated && (User.IsInRole("Staff") || User.IsInRole("Admin"));

        // If inactive and not staff/admin, return NotFound
        if (article.NewsStatus != true && !isStaffOrAdmin)
        {
            return NotFound($"News article with ID {key} not found or inactive.");
        }

        return Ok(article);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public IActionResult Post([FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        short? currentUserId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var parsedId) && parsedId > 0)
        {
            currentUserId = parsedId;
        }

        var article = new NewsArticle
        {
            NewsArticleId = dto.NewsArticleId,
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            CreatedDate = DateTime.Now,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus ?? true,
            CreatedById = dto.CreatedById ?? currentUserId,
            UpdatedById = null,
            ModifiedDate = null
        };

        try
        {
            var created = _newsArticleRepository.Create(article, dto.TagIds);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut]
    public IActionResult Put([FromRoute] string key, [FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        short? currentUserId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var parsedId) && parsedId > 0)
        {
            currentUserId = parsedId;
        }

        var article = new NewsArticle
        {
            NewsArticleId = key,
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus,
            UpdatedById = currentUserId ?? dto.UpdatedById,
            ModifiedDate = DateTime.Now
        };

        try
        {
            var updated = _newsArticleRepository.Update(article, dto.TagIds);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"News article with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete]
    public IActionResult Delete([FromRoute] string key)
    {
        var result = _newsArticleRepository.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("api/NewsArticles/history")]
    public IActionResult GetMyHistory()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !short.TryParse(idClaim, out var accountId))
        {
            return Unauthorized();
        }

        var articles = _newsArticleRepository.GetByCreator(accountId);
        return Ok(articles);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("api/NewsArticles/creator/{accountId}")]
    public IActionResult GetByCreator([FromRoute] short accountId)
    {
        var articles = _newsArticleRepository.GetByCreator(accountId);
        return Ok(articles);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/NewsArticles/report")]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _newsArticleRepository.GetReport(startDate, endDate);
        return Ok(report);
    }
}
