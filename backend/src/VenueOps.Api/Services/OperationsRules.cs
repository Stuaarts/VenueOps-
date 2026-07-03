namespace VenueOps.Api.Services;

public static class OperationsRules
{
    public static string? ValidateBookingWindow(TimeOnly start, TimeOnly end) =>
        end > start ? null : "End time must be later than start time.";

    public static string? ValidateShiftWindow(DateTimeOffset start, DateTimeOffset end) =>
        end > start ? null : "Shift end must be later than shift start.";

    public static string? ValidateGuestCountAgainstCapacity(int guestCount, int capacity) =>
        guestCount <= capacity ? null : $"Guest count exceeds venue capacity of {capacity}.";
}
