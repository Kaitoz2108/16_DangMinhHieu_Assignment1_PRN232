using System.Collections.Generic;
using BusinessObjects.Entities;
using DataAccessObjects;

namespace Repositories;

public class TagRepository : ITagRepository
{
    public List<Tag> GetAll() => TagDAO.Instance.GetAll();

    public Tag? GetById(int id) => TagDAO.Instance.GetById(id);

    public Tag Create(Tag tag) => TagDAO.Instance.Create(tag);

    public Tag Update(Tag tag) => TagDAO.Instance.Update(tag);

    public (bool Success, string Message) Delete(int id) => TagDAO.Instance.Delete(id);
}
