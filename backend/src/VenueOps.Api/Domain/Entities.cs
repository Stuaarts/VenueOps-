namespace VenueOps.Api.Domain;

public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AppUser : AuditableEntity
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
    public ICollection<ShiftNote> ShiftNotes { get; set; } = new List<ShiftNote>();
}

public sealed class Client : AuditableEntity
{
    public required string Name { get; set; }
    public required string ContactName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public ICollection<EventBooking> Bookings { get; set; } = new List<EventBooking>();
}

public sealed class VenueRoom : AuditableEntity
{
    public required string Name { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public ICollection<EventBooking> Bookings { get; set; } = new List<EventBooking>();
}

public sealed class EventBooking : AuditableEntity
{
    public required string EventName { get; set; }
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid VenueRoomId { get; set; }
    public VenueRoom? VenueRoom { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GuestCount { get; set; }
    public required string EventType { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Inquiry;
    public string? InternalNotes { get; set; }
    public ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
    public ICollection<ShiftNote> ShiftNotes { get; set; } = new List<ShiftNote>();
}

public sealed class StaffAssignment : AuditableEntity
{
    public Guid EventBookingId { get; set; }
    public EventBooking? EventBooking { get; set; }
    public Guid StaffUserId { get; set; }
    public AppUser? StaffUser { get; set; }
    public StaffRole Role { get; set; }
    public DateTimeOffset ShiftStart { get; set; }
    public DateTimeOffset ShiftEnd { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    public string? Notes { get; set; }
}

public sealed class ShiftNote : AuditableEntity
{
    public Guid EventBookingId { get; set; }
    public EventBooking? EventBooking { get; set; }
    public Guid StaffUserId { get; set; }
    public AppUser? StaffUser { get; set; }
    public ShiftNoteType NoteType { get; set; } = ShiftNoteType.General;
    public required string Body { get; set; }
    public bool IsPinned { get; set; }
}
