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
[Route("api/shift-notes")]
public sealed class ShiftNotesController(VenueOpsDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftNoteDto>>> GetNotes(
        [FromQuery] Guid? eventBookingId,
        CancellationToken cancellationToken)
    {
        var query = BaseNotesQuery();

        if (eventBookingId.HasValue)
        {
            query = query.Where(x => x.EventBookingId == eventBookingId.Value);
        }

        var notes = await query
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(notes.Select(x => x.ToDto()).ToList());
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager},{RoleNames.Staff}")]
    [HttpPost]
    public async Task<ActionResult<ShiftNoteDto>> CreateNote(CreateShiftNoteRequest request, CancellationToken cancellationToken)
    {
        var booking = await db.EventBookings
            .Include(x => x.StaffAssignments)
            .SingleOrDefaultAsync(x => x.Id == request.EventBookingId, cancellationToken);

        if (booking is null)
        {
            ModelState.AddModelError(nameof(request.EventBookingId), "Booking does not exist.");
            return ValidationProblem(ModelState);
        }

        var noteAuthorId = request.StaffUserId ?? currentUser.UserId;
        if (currentUser.Role == UserRole.Staff)
        {
            noteAuthorId = currentUser.UserId;
            if (!booking.StaffAssignments.Any(x => x.StaffUserId == currentUser.UserId))
            {
                return Forbid();
            }
        }

        var author = await db.Users.SingleOrDefaultAsync(x => x.Id == noteAuthorId && x.IsActive, cancellationToken);
        if (author is null)
        {
            ModelState.AddModelError(nameof(request.StaffUserId), "Active note author does not exist.");
            return ValidationProblem(ModelState);
        }

        var note = new ShiftNote
        {
            EventBookingId = request.EventBookingId,
            StaffUserId = noteAuthorId,
            NoteType = request.NoteType,
            Body = request.Body.Trim(),
            IsPinned = request.IsPinned && currentUser.IsInRole(UserRole.Admin, UserRole.Manager)
        };

        db.ShiftNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        var created = await BaseNotesQuery().SingleAsync(x => x.Id == note.Id, cancellationToken);
        return CreatedAtAction(nameof(GetNotes), new { eventBookingId = created.EventBookingId }, created.ToDto());
    }

    private IQueryable<ShiftNote> BaseNotesQuery()
    {
        var query = db.ShiftNotes
            .AsNoTracking()
            .Include(x => x.EventBooking)
            .Include(x => x.StaffUser)
            .AsQueryable();

        if (currentUser.Role == UserRole.Staff)
        {
            query = query.Where(x => x.EventBooking!.StaffAssignments.Any(a => a.StaffUserId == currentUser.UserId));
        }

        return query;
    }
}
