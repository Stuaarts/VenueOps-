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
public sealed class ClientsController(VenueOpsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetClients([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = db.Clients.AsNoTracking().Include(x => x.Bookings).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(normalized) ||
                x.ContactName.ToLower().Contains(normalized) ||
                x.Email.ToLower().Contains(normalized));
        }

        var clients = await query
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> GetClient(Guid id, CancellationToken cancellationToken)
    {
        var client = await db.Clients
            .AsNoTracking()
            .Include(x => x.Bookings)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return client is null ? NotFound() : Ok(client.ToDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient(UpsertClientRequest request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Name = request.Name.Trim(),
            ContactName = request.ContactName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            Notes = request.Notes?.Trim()
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client.ToDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, UpsertClientRequest request, CancellationToken cancellationToken)
    {
        var client = await db.Clients.Include(x => x.Bookings).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        client.Name = request.Name.Trim();
        client.ContactName = request.ContactName.Trim();
        client.Email = request.Email.Trim().ToLowerInvariant();
        client.Phone = request.Phone?.Trim();
        client.Notes = request.Notes?.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return Ok(client.ToDto());
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken)
    {
        var client = await db.Clients.Include(x => x.Bookings).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        if (client.Bookings.Count > 0)
        {
            return BadRequest(new { message = "Clients with bookings cannot be deleted." });
        }

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
