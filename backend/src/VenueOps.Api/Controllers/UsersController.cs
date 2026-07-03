using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueOps.Api.Authorization;
using VenueOps.Api.Data;
using VenueOps.Api.Domain;
using VenueOps.Api.Dtos;
using VenueOps.Api.Services;

namespace VenueOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(VenueOpsDbContext db) : ControllerBase
{
    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().AsQueryable();
        if (role.HasValue)
        {
            query = query.Where(x => x.Role == role.Value);
        }

        var users = await query
            .OrderBy(x => x.Role)
            .ThenBy(x => x.FullName)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [Authorize(Roles = RoleNames.AssignableStaffReaders)]
    [HttpGet("staff")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetStaff(CancellationToken cancellationToken)
    {
        var staff = await db.Users
            .AsNoTracking()
            .Where(x => x.IsActive && x.Role == UserRole.Staff)
            .OrderBy(x => x.FullName)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(staff);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email.ToLower() == email, cancellationToken))
        {
            return Conflict(new { message = "A user with that email already exists." });
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = request.IsActive
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user.ToDto());
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Id != id && x.Email.ToLower() == email, cancellationToken))
        {
            return Conflict(new { message = "A user with that email already exists." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(user.ToDto());
    }
}
