using System.Security.Claims;
using LU2.Models;
using LU2.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LU2.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class Environment2DController(Environment2DRepository environment2DRepository, Object2DRepository object2DRepository) : ControllerBase
{
    private readonly IEnvironment2DRepository _environment2DRepository = environment2DRepository;
    private readonly IObject2DRepository _object2DRepository = object2DRepository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Environment2D>>> Get()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userEnvironments = await _environment2DRepository.GetByUserIdAsync(userId);
        return Ok(userEnvironments);
    }

    [HttpPost]
    public async Task<ActionResult<Environment2D>> Post([FromBody] Environment2D environment)
    {
        // Set the UserId of the environment to the UserId of the authenticated user
        environment.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userEnvironments = await _environment2DRepository.GetByUserIdAsync(environment.UserId);
        if (userEnvironments.Count() >= 5)
        {
            return BadRequest("A user can only have up to 5 environments.");
        }
        if (environment.Name.Length <= 1 || string.IsNullOrWhiteSpace(environment.Name))
        {
            return BadRequest("The environment name must be at least 1 character long.");
        }
        if (environment.Name.Length > 25)
        {
            return BadRequest("The environment name must be 25 characters or less.");
        }
        environment.Id = Guid.NewGuid().ToString();
        await _environment2DRepository.AddAsync(environment);
        return CreatedAtAction(nameof(Get), new { id = environment.Id }, environment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(!await IsUserEnvironment(id, userId))
        {
            return Unauthorized();
        }

        await _environment2DRepository.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpGet("{id}/objects")]
    public async Task<ActionResult<IEnumerable<Object2D>>> GetObjects(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(!await IsUserEnvironment(id, userId))
        {
            return Unauthorized();
        }
        
        var environmentObjects = await _object2DRepository.GetByEnvironmentIdAsync(id);
        return Ok(environmentObjects);
    }
    
    [HttpPost("{id}/objects")]
    public async Task<ActionResult<Object2D>> PostObject(Guid id, [FromBody] Object2D object2D)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(!await IsUserEnvironment(id, userId))
        {
            return Unauthorized();
        }
        
        object2D.Id = Guid.NewGuid().ToString();
        object2D.EnvironmentId = id.ToString();
        await _object2DRepository.AddAsync(object2D);
        return CreatedAtAction(nameof(GetObjects), new { id = object2D.Id }, object2D);
    }
    
    [HttpDelete("{id}/objects/{objectId}")]
    public async Task<IActionResult> DeleteObject(Guid id, Guid objectId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(!await IsUserEnvironment(id, userId))
        {
            return Unauthorized();
        }
        
        var object2D = await _object2DRepository.GetByIdAsync(objectId);
        if (object2D == null || object2D.EnvironmentId != id.ToString())
        {
            return NotFound();
        }

        await _object2DRepository.DeleteAsync(objectId);
        return NoContent();
    }
    
    [HttpDelete("{id}/objects")]
    public async Task<IActionResult> DeleteObjects(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(!await IsUserEnvironment(id, userId))
        {
            return Unauthorized();
        }
        
        await _object2DRepository.DeleteByEnvironmentIdAsync(id);
        return NoContent();
    }
    
    private async Task<bool> IsUserEnvironment(Guid id, string userId)
    {
        var environments = await _environment2DRepository.GetByUserIdAsync(userId);
        return environments.Any(e => e.Id.Equals(id.ToString()));
    }
}