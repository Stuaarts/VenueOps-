using VenueOps.Api.Services;

namespace VenueOps.Api.Tests;

public sealed class OperationsRulesTests
{
    [Fact]
    public void ValidateBookingWindow_rejects_end_before_start()
    {
        var error = OperationsRules.ValidateBookingWindow(TimeOnly.Parse("18:00"), TimeOnly.Parse("17:00"));

        Assert.Equal("End time must be later than start time.", error);
    }

    [Fact]
    public void ValidateGuestCountAgainstCapacity_rejects_over_capacity_events()
    {
        var error = OperationsRules.ValidateGuestCountAgainstCapacity(220, 180);

        Assert.Equal("Guest count exceeds venue capacity of 180.", error);
    }
}
