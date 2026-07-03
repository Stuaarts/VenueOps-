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
public sealed class VenuesController(VenueOpsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VenueRoomDto>>> GetVenues(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var venues = await db.VenueRooms
            .AsNoTracking()
            .Include(x => x.Bookings)
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto(x.Bookings.Count(b => b.EventDate >= today && b.Status != BookingStatus.Cancelled)))
            .ToListAsync(cancellationToken);

        return Ok(venues);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VenueRoomDto>> GetVenue(Guid id, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var venue = await db.VenueRooms
            .AsNoTracking()
            .Include(x => x.Bookings)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return venue is null
            ? NotFound()
            : Ok(venue.ToDto(venue.Bookings.Count(x => x.EventDate >= today && x.Status != BookingStatus.Cancelled)));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<VenueRoomDto>> CreateVenue(UpsertVenueRoomRequest request, CancellationToken cancellationToken)
    {
        if (await db.VenueRooms.AnyAsync(x => x.Name.ToLower() == request.Name.Trim().ToLowerInvariant(), cancellationToken))
        {
            return Conflict(new { message = "A venue or room with that name already exists." });
        }

        var venue = new VenueRoom
        {
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            Capacity = request.Capacity,
            IsActive = request.IsActive,
            Notes = request.Notes?.Trim()
        };

        db.VenueRooms.Add(venue);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetVenue), new { id = venue.Id }, venue.ToDto());
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VenueRoomDto>> UpdateVenue(Guid id, UpsertVenueRoomRequest request, CancellationToken cancellationToken)
    {
        var venue = await db.VenueRooms.FindAsync([id], cancellationToken);
        if (venue is null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        if (await db.VenueRooms.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            return Conflict(new { message = "A venue or room with that name already exists." });
        }

        venue.Name = name;
        venue.Location = request.Location.Trim();
        venue.Capacity = request.Capacity;
        venue.IsActive = request.IsActive;
        venue.Notes = request.Notes?.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return Ok(venue.ToDto());
    }
}
