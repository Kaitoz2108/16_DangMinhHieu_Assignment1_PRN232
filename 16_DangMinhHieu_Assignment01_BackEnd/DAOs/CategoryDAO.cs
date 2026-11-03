using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class CategoryDAO
{
    private static CategoryDAO? _instance = null;
    private static readonly object _instanceLock = new object();

    public static CategoryDAO Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new CategoryDAO();
                    }
                }
            }
            return _instance;
        }
    }

    private CategoryDAO() { }

    public List<Category> GetAll()
    {
        using var context = new FUNewsManagementDbContext();
        return context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .ToList();
    }

    public List<Category> GetActiveCategories()
    {
        using var context = new FUNewsManagementDbContext();
        return context.Categories
            .Where(c => c.IsActive == true)
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .ToList();
    }

    public Category? GetById(short id)
    {
        using var context = new FUNewsManagementDbContext();
        return context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.NewsArticles)
            .AsNoTracking()
            .FirstOrDefault(c => c.CategoryId == id);
    }

    public Category Create(Category category)
    {
        using var context = new FUNewsManagementDbContext();
        category.ParentCategory = null;
        category.NewsArticles.Clear();
        category.InverseParentCategory.Clear();
        
        context.Categories.Add(category);
        context.SaveChanges();
        return category;
    }

    public Category Update(Category category)
    {
        using var context = new FUNewsManagementDbContext();
        var existing = context.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Category with ID {category.CategoryId} not found.");
        }

        existing.CategoryName = category.CategoryName;
        existing.CategoryDesciption = category.CategoryDesciption;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;

        context.SaveChanges();
        return existing;
    }

    public (bool Success, string Message) Delete(short id)
    {
        using var context = new FUNewsManagementDbContext();
        var category = context.Categories
            .Include(c => c.NewsArticles)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefault(c => c.CategoryId == id);

        if (category == null)
        {
            return (false, "Category not found.");
        }

        // Delete Restriction: CANNOT delete a category if NewsArticles.Any(a => a.CategoryID == categoryId)
        if (category.NewsArticles.Any())
        {
            return (false, "Cannot delete category because it is associated with existing news articles.");
        }

        if (category.InverseParentCategory.Any())
        {
            return (false, "Cannot delete category because it has child subcategories.");
        }

        context.Categories.Remove(category);
        context.SaveChanges();
        return (true, "Category deleted successfully.");
    }
}
