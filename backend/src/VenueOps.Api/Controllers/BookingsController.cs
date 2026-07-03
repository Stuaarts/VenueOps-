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
public sealed class BookingsController(VenueOpsDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingSummaryDto>>> GetBookings(
        [FromQuery] string? search,
        [FromQuery] BookingStatus? status,
        [FromQuery] Guid? venueRoomId,
        [FromQuery] Guid? assignedStaffUserId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query = BaseBookingQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.EventName.ToLower().Contains(normalized) ||
                x.EventType.ToLower().Contains(normalized) ||
                x.Client!.Name.ToLower().Contains(normalized) ||
                x.VenueRoom!.Name.ToLower().Contains(normalized));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (venueRoomId.HasValue)
        {
            query = query.Where(x => x.VenueRoomId == venueRoomId.Value);
        }

        if (assignedStaffUserId.HasValue)
        {
            query = query.Where(x => x.StaffAssignments.Any(a => a.StaffUserId == assignedStaffUserId.Value));
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.EventDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.EventDate <= to.Value);
        }

        var bookings = await query
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(bookings.Select(x => x.ToSummaryDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDetailDto>> GetBooking(Guid id, CancellationToken cancellationToken)
    {
        var booking = await BaseBookingQuery()
            .Include(x => x.ShiftNotes)
                .ThenInclude(x => x.StaffUser)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return booking is null ? NotFound() : Ok(booking.ToDetailDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPost]
    public async Task<ActionResult<BookingDetailDto>> CreateBooking(UpsertBookingRequest request, CancellationToken cancellationToken)
    {
        var validationProblem = await ValidateBookingRequest(request, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var booking = new EventBooking
        {
            EventName = request.EventName.Trim(),
            ClientId = request.ClientId,
            VenueRoomId = request.VenueRoomId,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            GuestCount = request.GuestCount,
            EventType = request.EventType.Trim(),
            Status = request.Status,
            InternalNotes = request.InternalNotes?.Trim()
        };

        db.EventBookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);

        var created = await BaseBookingQuery()
            .Include(x => x.ShiftNotes).ThenInclude(x => x.StaffUser)
            .SingleAsync(x => x.Id == booking.Id, cancellationToken);

        return CreatedAtAction(nameof(GetBooking), new { id = created.Id }, created.ToDetailDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookingDetailDto>> UpdateBooking(Guid id, UpsertBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await db.EventBookings.FindAsync([id], cancellationToken);
        if (booking is null)
        {
            return NotFound();
        }

        var validationProblem = await ValidateBookingRequest(request, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        booking.EventName = request.EventName.Trim();
        booking.ClientId = request.ClientId;
        booking.VenueRoomId = request.VenueRoomId;
        booking.EventDate = request.EventDate;
        booking.StartTime = request.StartTime;
        booking.EndTime = request.EndTime;
        booking.GuestCount = request.GuestCount;
        booking.EventType = request.EventType.Trim();
        booking.Status = request.Status;
        booking.InternalNotes = request.InternalNotes?.Trim();

        await db.SaveChangesAsync(cancellationToken);

        var updated = await BaseBookingQuery()
            .Include(x => x.ShiftNotes).ThenInclude(x => x.StaffUser)
            .SingleAsync(x => x.Id == id, cancellationToken);

        return Ok(updated.ToDetailDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingDetailDto>> UpdateStatus(Guid id, UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var booking = await db.EventBookings.FindAsync([id], cancellationToken);
        if (booking is null)
        {
            return NotFound();
        }

        booking.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);

        var updated = await BaseBookingQuery()
            .Include(x => x.ShiftNotes).ThenInclude(x => x.StaffUser)
            .SingleAsync(x => x.Id == id, cancellationToken);

        return Ok(updated.ToDetailDto());
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
    {
        var booking = await db.EventBookings.FindAsync([id], cancellationToken);
        if (booking is null)
        {
            return NotFound();
        }

        db.EventBookings.Remove(booking);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<EventBooking> BaseBookingQuery()
    {
        var query = db.EventBookings
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.VenueRoom)
            .Include(x => x.StaffAssignments)
                .ThenInclude(x => x.StaffUser)
            .AsQueryable();

        if (currentUser.Role == UserRole.Staff)
        {
            query = query.Where(x => x.StaffAssignments.Any(a => a.StaffUserId == currentUser.UserId));
        }

        return query;
    }

    private async Task<ActionResult?> ValidateBookingRequest(UpsertBookingRequest request, CancellationToken cancellationToken)
    {
        var bookingWindowError = OperationsRules.ValidateBookingWindow(request.StartTime, request.EndTime);
        if (bookingWindowError is not null)
        {
            ModelState.AddModelError(nameof(request.EndTime), bookingWindowError);
        }

        var clientExists = await db.Clients.AnyAsync(x => x.Id == request.ClientId, cancellationToken);
        if (!clientExists)
        {
            ModelState.AddModelError(nameof(request.ClientId), "Client does not exist.");
        }

        var venue = await db.VenueRooms.SingleOrDefaultAsync(x => x.Id == request.VenueRoomId, cancellationToken);
        if (venue is null)
        {
            ModelState.AddModelError(nameof(request.VenueRoomId), "Venue room does not exist.");
        }
        else if (!venue.IsActive)
        {
            ModelState.AddModelError(nameof(request.VenueRoomId), "Venue room is inactive.");
        }
        else
        {
            var guestCountError = OperationsRules.ValidateGuestCountAgainstCapacity(request.GuestCount, venue.Capacity);
            if (guestCountError is not null)
            {
                ModelState.AddModelError(nameof(request.GuestCount), guestCountError);
            }
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }
}
