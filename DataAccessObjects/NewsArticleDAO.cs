using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class NewsArticleDAO
{
    private static NewsArticleDAO? _instance = null;
    private static readonly object _instanceLock = new object();

    public static NewsArticleDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new NewsArticleDAO();
                    }
                }
            }
            return _instance;
        }
    }

    private NewsArticleDAO() { }

    public List<NewsArticle> GetAll(bool includeInactive = true)
    {
        using var context = new FUNewsManagementDbContext();
        var query = context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(a => a.NewsStatus == true);
        }

        return query.OrderByDescending(a => a.CreatedDate).ToList();
    }

    public List<NewsArticle> GetActiveArticles()
    {
        return GetAll(includeInactive: false);
    }

    public NewsArticle? GetById(string id)
    {
        using var context = new FUNewsManagementDbContext();
        return context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .AsNoTracking()
            .FirstOrDefault(a => a.NewsArticleId == id);
    }

    public List<NewsArticle> GetByCreator(short accountId)
    {
        using var context = new FUNewsManagementDbContext();
        return context.NewsArticles
            .Where(a => a.CreatedById == accountId)
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .OrderByDescending(a => a.CreatedDate)
            .AsNoTracking()
            .ToList();
    }

    public List<NewsArticle> GetReport(DateTime? startDate, DateTime? endDate)
    {
        using var context = new FUNewsManagementDbContext();
        var query = context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .AsNoTracking();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(a => a.CreatedDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.CreatedDate <= end);
        }

        return query.OrderByDescending(a => a.CreatedDate).ToList();
    }

    public NewsArticle Create(NewsArticle article, List<int>? tagIds = null)
    {
        using var context = new FUNewsManagementDbContext();

        if (string.IsNullOrWhiteSpace(article.NewsArticleId))
        {
            // Compute next numeric ID
            int nextId = 1;
            var existingIds = context.NewsArticles
                .Select(a => a.NewsArticleId)
                .ToList();

            var numericIds = existingIds
                .Select(id => int.TryParse(id, out var num) ? num : 0)
                .Where(num => num > 0)
                .ToList();

            if (numericIds.Any())
            {
                nextId = numericIds.Max() + 1;
            }
            article.NewsArticleId = nextId.ToString();
        }

        if (article.CreatedDate == null)
        {
            article.CreatedDate = DateTime.Now;
        }

        article.Category = null;
        article.CreatedBy = null;

        if (tagIds != null && tagIds.Any())
        {
            var tags = context.Tags.Where(t => tagIds.Contains(t.TagId)).ToList();
            article.Tags = tags;
        }
        else
        {
            article.Tags.Clear();
        }

        context.NewsArticles.Add(article);
        context.SaveChanges();
        return article;
    }

    public NewsArticle Update(NewsArticle article, List<int>? tagIds = null)
    {
        using var context = new FUNewsManagementDbContext();
        var existing = context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefault(a => a.NewsArticleId == article.NewsArticleId);

        if (existing == null)
        {
            throw new KeyNotFoundException($"NewsArticle with ID {article.NewsArticleId} not found.");
        }

        existing.NewsTitle = article.NewsTitle;
        existing.Headline = article.Headline;
        existing.NewsContent = article.NewsContent;
        existing.NewsSource = article.NewsSource;
        existing.CategoryId = article.CategoryId;
        existing.NewsStatus = article.NewsStatus;
        existing.UpdatedById = article.UpdatedById;
        existing.ModifiedDate = DateTime.Now;

        if (tagIds != null)
        {
            existing.Tags.Clear();
            var tags = context.Tags.Where(t => tagIds.Contains(t.TagId)).ToList();
            foreach (var tag in tags)
            {
                existing.Tags.Add(tag);
            }
        }

        context.SaveChanges();
        return existing;
    }

    public (bool Success, string Message) Delete(string id)
    {
        using var context = new FUNewsManagementDbContext();
        var article = context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefault(a => a.NewsArticleId == id);

        if (article == null)
        {
            return (false, "News article not found.");
        }

        article.Tags.Clear();
        context.NewsArticles.Remove(article);
        context.SaveChanges();
        return (true, "News article deleted successfully.");
    }
}
