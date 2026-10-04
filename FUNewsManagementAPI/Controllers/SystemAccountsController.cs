using System;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

[Authorize(Roles = "Admin")]
public class SystemAccountsController : ODataController
{
    private readonly ISystemAccountRepository _accountRepository;

    public SystemAccountsController(ISystemAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        var accounts = _accountRepository.GetAll().AsQueryable();
        return Ok(accounts);
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get([FromRoute] short key)
    {
        var account = _accountRepository.GetById(key);
        if (account == null)
        {
            return NotFound($"Account with ID {key} not found.");
        }
        return Ok(account);
    }

    [HttpPost]
    public IActionResult Post([FromBody] SystemAccount account)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = _accountRepository.Create(account);
            return Created(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public IActionResult Put([FromRoute] short key, [FromBody] SystemAccount account)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != account.AccountId)
        {
            account.AccountId = key;
        }

        try
        {
            var updated = _accountRepository.Update(account);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Account with ID {key} not found.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete]
    public IActionResult Delete([FromRoute] short key)
    {
        var result = _accountRepository.Delete(key);
        if (!result.Success)
        {
            // Descriptive error (400 Bad Request)
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
