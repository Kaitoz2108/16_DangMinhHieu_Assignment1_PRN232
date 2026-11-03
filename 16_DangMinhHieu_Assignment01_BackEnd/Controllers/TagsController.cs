using System;
using System.Linq;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories;

namespace FUNewsManagementAPI.Controllers;

public class TagsController : ODataController
{
    private readonly ITagRepository _tagRepository;

    public TagsController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        var tags = _tagRepository.GetAll().AsQueryable();
        return Ok(tags);
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get([FromRoute] int key)
    {
        var tag = _tagRepository.GetById(key);
        if (tag == null)
        {
            return NotFound($"Tag with ID {key} not found.");
        }
        return Ok(tag);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public IActionResult Post([FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = _tagRepository.Create(tag);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut]
    public IActionResult Put([FromRoute] int key, [FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != tag.TagId)
        {
            tag.TagId = key;
        }

        try
        {
            var updated = _tagRepository.Update(tag);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Tag with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete]
    public IActionResult Delete([FromRoute] int key)
    {
        var result = _tagRepository.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
