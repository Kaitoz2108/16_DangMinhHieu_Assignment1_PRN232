using System.Collections.Generic;
using BusinessObjects.Entities;

namespace Repositories;

public interface ICategoryRepository
{
    List<Category> GetAll();
    List<Category> GetActiveCategories();
    Category? GetById(short id);
    Category Create(Category category);
    Category Update(Category category);
    (bool Success, string Message) Delete(short id);
}
