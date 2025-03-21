using LU2.Models;
using Microsoft.AspNetCore.Mvc;
using LU2.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace LU2.Controllers;

// TODO! make sure to user user id from token to check if user is authorized to operate.
// var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

[ApiController]
[Authorize]
[Route("[controller]")]
public class Object2DController(Object2DRepository repository) : ControllerBase
{
    private readonly IObject2DRepository _repository = repository;
    
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
            await _repository.DeleteByEnvironmentIdAsync(environmentId);
        }
        return NoContent();
    }
}