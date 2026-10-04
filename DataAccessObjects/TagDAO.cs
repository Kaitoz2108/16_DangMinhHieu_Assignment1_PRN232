using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class TagDAO
{
    private static TagDAO? _instance = null;
    private static readonly object _instanceLock = new object();

    public static TagDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new TagDAO();
                    }
                }
            }
            return _instance;
        }
    }

    private TagDAO() { }

    public List<Tag> GetAll()
    {
        using var context = new FUNewsManagementDbContext();
        return context.Tags
            .AsNoTracking()
            .ToList();
    }

    public Tag? GetById(int id)
    {
        using var context = new FUNewsManagementDbContext();
        return context.Tags
            .Include(t => t.NewsArticles)
            .AsNoTracking()
            .FirstOrDefault(t => t.TagId == id);
    }

    public Tag Create(Tag tag)
    {
        using var context = new FUNewsManagementDbContext();
        if (tag.TagId <= 0)
        {
            int nextId = 1;
            if (context.Tags.Any())
            {
                nextId = context.Tags.Max(t => t.TagId) + 1;
            }
            tag.TagId = nextId;
        }
        tag.NewsArticles.Clear();

        context.Tags.Add(tag);
        context.SaveChanges();
        return tag;
    }

    public Tag Update(Tag tag)
    {
        using var context = new FUNewsManagementDbContext();
        var existing = context.Tags.FirstOrDefault(t => t.TagId == tag.TagId);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Tag with ID {tag.TagId} not found.");
        }

        existing.TagName = tag.TagName;
        existing.Note = tag.Note;

        context.SaveChanges();
        return existing;
    }

    public (bool Success, string Message) Delete(int id)
    {
        using var context = new FUNewsManagementDbContext();
        var tag = context.Tags
            .Include(t => t.NewsArticles)
            .FirstOrDefault(t => t.TagId == id);

        if (tag == null)
        {
            return (false, "Tag not found.");
        }

        if (tag.NewsArticles.Any())
        {
            return (false, "Cannot delete tag because it is used in news articles.");
        }

        context.Tags.Remove(tag);
        context.SaveChanges();
        return (true, "Tag deleted successfully.");
    }
}
