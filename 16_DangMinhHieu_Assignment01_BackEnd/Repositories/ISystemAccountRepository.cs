using System.Collections.Generic;
using BusinessObjects.Entities;

namespace Repositories;

public interface ISystemAccountRepository
{
    List<SystemAccount> GetAll();
    SystemAccount? GetById(short id);
    SystemAccount? GetByEmail(string email);
    SystemAccount? Authenticate(string email, string password);
    SystemAccount Create(SystemAccount account);
    SystemAccount Update(SystemAccount account);
    (bool Success, string Message) Delete(short accountId);
}
