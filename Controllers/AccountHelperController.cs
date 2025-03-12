using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ICT1._3_API.Controllers;

[ApiController]
public class AccountHelperController(UserManager<IdentityUser> userManager) : ControllerBase
{
    [HttpGet("user/{userMail}")]
    public async Task<ActionResult<IEnumerable<string>>> GetByUserMail(string userMail)
    {
        var user = await userManager.FindByEmailAsync(userMail);
        if (user == null)
        {
            return NotFound();
        }

        var userId = user.Id;
        return Ok(userId);
    }
}