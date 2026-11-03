using System;
using System.Collections.Generic;
using BusinessObjects.Entities;
using DataAccessObjects;

namespace Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    public List<NewsArticle> GetAll(bool includeInactive = true) => NewsArticleDAO.Instance.GetAll(includeInactive);

    public List<NewsArticle> GetActiveArticles() => NewsArticleDAO.Instance.GetActiveArticles();

    public NewsArticle? GetById(string id) => NewsArticleDAO.Instance.GetById(id);

    public List<NewsArticle> GetByCreator(short accountId) => NewsArticleDAO.Instance.GetByCreator(accountId);

    public List<NewsArticle> GetReport(DateTime? startDate, DateTime? endDate) => NewsArticleDAO.Instance.GetReport(startDate, endDate);

    public NewsArticle Create(NewsArticle article, List<int>? tagIds = null) => NewsArticleDAO.Instance.Create(article, tagIds);

    public NewsArticle Update(NewsArticle article, List<int>? tagIds = null) => NewsArticleDAO.Instance.Update(article, tagIds);

    public (bool Success, string Message) Delete(string id) => NewsArticleDAO.Instance.Delete(id);
}
