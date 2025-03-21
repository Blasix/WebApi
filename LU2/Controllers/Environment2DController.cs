using System.Security.Claims;
using LU2.Models;
using LU2.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LU2.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class Environment2DController(Environment2DRepository repository) : ControllerBase
{
    private readonly IEnvironment2DRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Environment2D>>> Get()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userEnvironments = await _repository.GetByUserIdAsync(userId);
        return Ok(userEnvironments);
    }

    [HttpPost]
    public async Task<ActionResult<Environment2D>> Post([FromBody] Environment2D environment)
    {
        // Set the UserId of the environment to the UserId of the authenticated user
        environment.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userEnvironments = await _repository.GetByUserIdAsync(environment.UserId);
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
        await _repository.AddAsync(environment);
        return CreatedAtAction(nameof(Get), new { id = environment.Id }, environment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var environments = await _repository.GetByUserIdAsync(userId);
        if (environments == null || environments.All(e => e.Id != id.ToString()))
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}