using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueOps.Api.Data;
using VenueOps.Api.Dtos;
using VenueOps.Api.Services;

namespace VenueOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(VenueOpsDbContext db, IJwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(tokenService.CreateToken(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> Me(ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([currentUser.UserId], cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        return Ok(new AuthUserDto(user.Id, user.FullName, user.Email, user.Role));
    }
}
