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
[Route("api/staff-assignments")]
public sealed class StaffAssignmentsController(VenueOpsDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffAssignmentDto>>> GetAssignments(
        [FromQuery] Guid? eventBookingId,
        [FromQuery] Guid? staffUserId,
        CancellationToken cancellationToken)
    {
        var query = BaseAssignmentQuery();

        if (eventBookingId.HasValue)
        {
            query = query.Where(x => x.EventBookingId == eventBookingId.Value);
        }

        if (staffUserId.HasValue)
        {
            query = query.Where(x => x.StaffUserId == staffUserId.Value);
        }

        var assignments = await query
            .OrderBy(x => x.ShiftStart)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(assignments.Select(x => x.ToDto()).ToList());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPost]
    public async Task<ActionResult<StaffAssignmentDto>> CreateAssignment(UpsertStaffAssignmentRequest request, CancellationToken cancellationToken)
    {
        var validationProblem = await ValidateAssignmentRequest(request, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var assignment = new StaffAssignment
        {
            EventBookingId = request.EventBookingId,
            StaffUserId = request.StaffUserId,
            Role = request.Role,
            ShiftStart = request.ShiftStart,
            ShiftEnd = request.ShiftEnd,
            Status = request.Status,
            Notes = request.Notes?.Trim()
        };

        db.StaffAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        var created = await BaseAssignmentQuery().SingleAsync(x => x.Id == assignment.Id, cancellationToken);
        return CreatedAtAction(nameof(GetAssignments), new { eventBookingId = created.EventBookingId }, created.ToDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffAssignmentDto>> UpdateAssignment(Guid id, UpsertStaffAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await db.StaffAssignments.FindAsync([id], cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        var validationProblem = await ValidateAssignmentRequest(request, cancellationToken);
        if (validationProblem is not null)
        {
            return validationProblem;
        }

        assignment.EventBookingId = request.EventBookingId;
        assignment.StaffUserId = request.StaffUserId;
        assignment.Role = request.Role;
        assignment.ShiftStart = request.ShiftStart;
        assignment.ShiftEnd = request.ShiftEnd;
        assignment.Status = request.Status;
        assignment.Notes = request.Notes?.Trim();

        await db.SaveChangesAsync(cancellationToken);

        var updated = await BaseAssignmentQuery().SingleAsync(x => x.Id == id, cancellationToken);
        return Ok(updated.ToDto());
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager},{RoleNames.Staff}")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<StaffAssignmentDto>> UpdateAssignmentStatus(Guid id, UpdateAssignmentStatusRequest request, CancellationToken cancellationToken)
    {
        var assignment = await db.StaffAssignments.FindAsync([id], cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        if (currentUser.Role == UserRole.Staff && assignment.StaffUserId != currentUser.UserId)
        {
            return Forbid();
        }

        assignment.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);

        var updated = await BaseAssignmentQuery().SingleAsync(x => x.Id == id, cancellationToken);
        return Ok(updated.ToDto());
    }

    [Authorize(Roles = RoleNames.Operations)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await db.StaffAssignments.FindAsync([id], cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        db.StaffAssignments.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<StaffAssignment> BaseAssignmentQuery()
    {
        var query = db.StaffAssignments
            .AsNoTracking()
            .Include(x => x.EventBooking)
            .Include(x => x.StaffUser)
            .AsQueryable();

        if (currentUser.Role == UserRole.Staff)
        {
            query = query.Where(x => x.StaffUserId == currentUser.UserId);
        }

        return query;
    }

    private async Task<ActionResult?> ValidateAssignmentRequest(UpsertStaffAssignmentRequest request, CancellationToken cancellationToken)
    {
        var shiftWindowError = OperationsRules.ValidateShiftWindow(request.ShiftStart, request.ShiftEnd);
        if (shiftWindowError is not null)
        {
            ModelState.AddModelError(nameof(request.ShiftEnd), shiftWindowError);
        }

        var bookingExists = await db.EventBookings.AnyAsync(x => x.Id == request.EventBookingId, cancellationToken);
        if (!bookingExists)
        {
            ModelState.AddModelError(nameof(request.EventBookingId), "Booking does not exist.");
        }

        var staff = await db.Users.SingleOrDefaultAsync(x => x.Id == request.StaffUserId, cancellationToken);
        if (staff is null || !staff.IsActive || staff.Role != UserRole.Staff)
        {
            ModelState.AddModelError(nameof(request.StaffUserId), "Active staff user does not exist.");
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }
}
