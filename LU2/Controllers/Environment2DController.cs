using ICT1._3_API.Models;
using ICT1._3_API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ICT1._3_API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class Environment2DController(Environment2DRepository repository) : ControllerBase
{
    private readonly IEnvironment2DRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Environment2D>>> Get()
    {
        var environments = await _repository.GetAllAsync();
        return Ok(environments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Environment2D>> Get(Guid id)
    {
        var environment = await _repository.GetByIdAsync(id);
        if (environment == null)
        {
            return NotFound();
        }
        return Ok(environment);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Environment2D>>> GetByUserId(Guid userId)
    {
        var userEnvironments = await _repository.GetByUserIdAsync(userId.ToString());
        return Ok(userEnvironments);
    }

    [HttpPost]
    public async Task<ActionResult<Environment2D>> Post([FromBody] Environment2D environment)
    {
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] Environment2D updatedEnvironment)
    {
        var environment = await _repository.GetByIdAsync(id);
        if (environment == null)
        {
            return NotFound();
        }

        updatedEnvironment.Id = id.ToString();
        await _repository.UpdateAsync(updatedEnvironment);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var environment = await _repository.GetByIdAsync(id);
        if (environment == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}