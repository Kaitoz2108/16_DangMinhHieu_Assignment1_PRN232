using System;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

public class CategoriesController : ODataController
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        var categories = _categoryRepository.GetAll().AsQueryable();
        return Ok(categories);
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get([FromRoute] short key)
    {
        var category = _categoryRepository.GetById(key);
        if (category == null)
        {
            return NotFound($"Category with ID {key} not found.");
        }
        return Ok(category);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public IActionResult Post([FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = _categoryRepository.Create(category);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut]
    public IActionResult Put([FromRoute] short key, [FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != category.CategoryId)
        {
            category.CategoryId = key;
        }

        try
        {
            var updated = _categoryRepository.Update(category);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Category with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete]
    public IActionResult Delete([FromRoute] short key)
    {
        var result = _categoryRepository.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
