using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueOps.Api.Data;
using VenueOps.Api.Domain;
using VenueOps.Api.Dtos;
using VenueOps.Api.Services;

namespace VenueOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DashboardController(VenueOpsDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var weekStartDate = DateTimeOffset.UtcNow.Date;
        var weekEndDate = weekStartDate.AddDays(7);

        var bookingQuery = db.EventBookings
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.VenueRoom)
            .Include(x => x.StaffAssignments)
                .ThenInclude(x => x.StaffUser)
            .AsQueryable();

        if (currentUser.Role == UserRole.Staff)
        {
            bookingQuery = bookingQuery.Where(x => x.StaffAssignments.Any(a => a.StaffUserId == currentUser.UserId));
        }

        var upcoming = await bookingQuery
            .Where(x => x.EventDate >= today && x.Status != BookingStatus.Cancelled)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.StartTime)
            .Take(8)
            .ToListAsync(cancellationToken);

        var monthBookings = await bookingQuery
            .Where(x => x.EventDate >= monthStart && x.EventDate <= monthEnd)
            .ToListAsync(cancellationToken);

        var eventsByStatus = await bookingQuery
            .GroupBy(x => x.Status)
            .Select(x => new StatusCountDto(x.Key, x.Count()))
            .ToListAsync(cancellationToken);

        var assignmentQuery = db.StaffAssignments
            .AsNoTracking()
            .Include(x => x.EventBooking)
            .Include(x => x.StaffUser)
            .Where(x => x.ShiftStart >= weekStartDate && x.ShiftStart < weekEndDate);

        if (currentUser.Role == UserRole.Staff)
        {
            assignmentQuery = assignmentQuery.Where(x => x.StaffUserId == currentUser.UserId);
        }

        var assignmentsThisWeek = await assignmentQuery
            .OrderBy(x => x.ShiftStart)
            .Take(8)
            .ToListAsync(cancellationToken);

        var notesQuery = db.ShiftNotes
            .AsNoTracking()
            .Include(x => x.EventBooking)
            .Include(x => x.StaffUser)
            .AsQueryable();

        if (currentUser.Role == UserRole.Staff)
        {
            notesQuery = notesQuery.Where(x => x.EventBooking!.StaffAssignments.Any(a => a.StaffUserId == currentUser.UserId));
        }

        var recentNotes = await notesQuery
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var needsStaff = await bookingQuery
            .CountAsync(x =>
                x.EventDate >= today &&
                x.Status != BookingStatus.Cancelled &&
                !x.StaffAssignments.Any(),
                cancellationToken);

        var cancelledCount = await bookingQuery.CountAsync(x => x.Status == BookingStatus.Cancelled, cancellationToken);

        var metrics = new DashboardMetricsDto(
            upcoming.Count,
            monthBookings.Count,
            monthBookings.Sum(x => x.GuestCount),
            assignmentsThisWeek.Count,
            cancelledCount,
            needsStaff);

        return Ok(new DashboardDto(
            metrics,
            eventsByStatus.OrderBy(x => x.Status).ToList(),
            upcoming.Select(x => x.ToSummaryDto()).ToList(),
            assignmentsThisWeek.Select(x => x.ToDto()).ToList(),
            recentNotes.Select(x => x.ToDto()).ToList()));
    }
}
