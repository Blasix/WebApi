using ICT1._3_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICT1._3_API.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace ICT1._3_API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class Object2DController(Object2DRepository repository) : ControllerBase
{
    private readonly IObject2DRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Object2D>>> Get()
    {
        var objects = await _repository.GetAllAsync();
        return Ok(objects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Object2D>> Get(Guid id)
    {
        var object2D = await _repository.GetByIdAsync(id);
        if (object2D == null)
        {
            return NotFound();
        }
        return Ok(object2D);
    }

    [HttpGet("environment/{environmentId}")]
    public async Task<ActionResult<IEnumerable<Object2D>>> GetByEnvironmentId(Guid environmentId)
    {
        var environmentObjects = await _repository.GetByEnvironmentIdAsync(environmentId);
        return Ok(environmentObjects);
    }

    [HttpPost]
    public async Task<ActionResult<Object2D>> Post([FromBody] Object2D object2D)
    {
        object2D.Id = Guid.NewGuid().ToString();
        await _repository.AddAsync(object2D);
        return CreatedAtAction(nameof(Get), new { id = object2D.Id }, object2D);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] Object2D updatedObject2D)
    {
        var object2D = await _repository.GetByIdAsync(id);
        if (object2D == null)
        {
            return NotFound();
        }

        updatedObject2D.Id = id.ToString();
        await _repository.UpdateAsync(updatedObject2D);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var object2D = await _repository.GetByIdAsync(id);
        if (object2D == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpDelete("environment/{environmentId}")]
    public async Task<IActionResult> DeleteByEnvironmentId(Guid environmentId)
    {
        var environmentObjects = await _repository.GetByEnvironmentIdAsync(environmentId);
        if (environmentObjects == null)
        {
            return NotFound();
        }

        foreach (var object2D in environmentObjects)
        {
            await _repository.DeleteAsync(Guid.Parse(object2D.Id));
        }
        return NoContent();
    }
}