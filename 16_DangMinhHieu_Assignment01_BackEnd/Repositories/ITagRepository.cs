using System.Collections.Generic;
using BusinessObjects.Entities;

namespace Repositories;

public interface ITagRepository
{
    List<Tag> GetAll();
    Tag? GetById(int id);
    Tag Create(Tag tag);
    Tag Update(Tag tag);
    (bool Success, string Message) Delete(int id);
}
