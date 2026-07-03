using System.Text.Json.Serialization;

namespace VenueOps.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin,
    Manager,
    Staff,
    Demo
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    Inquiry,
    Confirmed,
    InProgress,
    Completed,
    Cancelled
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssignmentStatus
{
    Assigned,
    Confirmed,
    Completed,
    NoShow
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StaffRole
{
    Server,
    Bartender,
    Supervisor,
    Setup,
    Kitchen,
    Security
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShiftNoteType
{
    SetupCompleted,
    GuestCountChanged,
    ClientRequest,
    Incident,
    Closing,
    SuppliesIssue,
    StaffingIssue,
    General
}
