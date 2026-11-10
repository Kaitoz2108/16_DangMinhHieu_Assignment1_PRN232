using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
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
    public IActionResult Get()
    {
        var articles = _newsArticleRepository.GetAll(includeInactive: true).AsQueryable();
        return Ok(articles);
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get([FromRoute] string key)
    {
        var article = _newsArticleRepository.GetById(key);
        if (article == null)
        {
            return NotFound($"News article with ID {key} not found.");
        }

        return Ok(article);
    }

    [HttpPost]
    public IActionResult Post([FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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
            CreatedById = dto.CreatedById,
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

    [HttpPut]
    public IActionResult Put([FromRoute] string key, [FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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
            UpdatedById = dto.UpdatedById,
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

    [HttpGet("api/NewsArticles/history")]
    public IActionResult GetMyHistory([FromQuery] short? accountId)
    {
        if (!accountId.HasValue || accountId.Value <= 0)
        {
            return BadRequest(new { message = "Account ID is required." });
        }

        var articles = _newsArticleRepository.GetByCreator(accountId.Value);
        return Ok(articles);
    }

    [HttpGet("api/NewsArticles/creator/{accountId}")]
    public IActionResult GetByCreator([FromRoute] short accountId)
    {
        var articles = _newsArticleRepository.GetByCreator(accountId);
        return Ok(articles);
    }

    [HttpGet("api/NewsArticles/report")]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _newsArticleRepository.GetReport(startDate, endDate);
        return Ok(report);
    }
}
