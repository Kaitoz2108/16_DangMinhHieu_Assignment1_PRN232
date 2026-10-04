using System.Collections.Generic;
using BusinessObjects.Entities;
using DataAccessObjects;

namespace Repositories;

public class SystemAccountRepository : ISystemAccountRepository
{
    public List<SystemAccount> GetAll() => SystemAccountDAO.Instance.GetAll();

    public SystemAccount? GetById(short id) => SystemAccountDAO.Instance.GetById(id);

    public SystemAccount? GetByEmail(string email) => SystemAccountDAO.Instance.GetByEmail(email);

    public SystemAccount? Authenticate(string email, string password) => SystemAccountDAO.Instance.Authenticate(email, password);

    public SystemAccount Create(SystemAccount account) => SystemAccountDAO.Instance.Create(account);

    public SystemAccount Update(SystemAccount account) => SystemAccountDAO.Instance.Update(account);

    public (bool Success, string Message) Delete(short accountId) => SystemAccountDAO.Instance.Delete(accountId);
}
