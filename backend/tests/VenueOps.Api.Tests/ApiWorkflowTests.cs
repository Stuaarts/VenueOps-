using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VenueOps.Api.Domain;
using VenueOps.Api.Dtos;

namespace VenueOps.Api.Tests;

public sealed class ApiWorkflowTests
{
    private const string DemoPassword = "VenueOpsDemo!2026";

    [Fact]
    public async Task Login_returns_jwt_and_user_role()
    {
        await using var factory = new VenueOpsApiFactory();
        using var client = factory.CreateClient();

        var response = await LoginAsync(client, "admin@venueops.local");

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(UserRole.Admin, response.User.Role);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.Token);
        var me = await client.GetAsync("/api/auth/me");
        var authHeader = string.Join("; ", me.Headers.WwwAuthenticate.Select(x => x.ToString()));
        Assert.True(me.IsSuccessStatusCode, $"Expected /me success but got {(int)me.StatusCode}. {authHeader}");
    }

    [Fact]
    public async Task Demo_user_cannot_create_booking()
    {
        await using var factory = new VenueOpsApiFactory();
        using var client = factory.CreateClient();
        await AuthorizeAsAsync(client, "demo@venueops.local");

        var request = await BuildBookingRequestAsync(client);
        var response = await client.PostAsJsonAsync("/api/bookings", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_can_create_booking_update_status_assign_staff_and_staff_can_add_note()
    {
        await using var factory = new VenueOpsApiFactory();
        using var client = factory.CreateClient();
        await AuthorizeAsAsync(client, "manager@venueops.local");

        var bookingRequest = await BuildBookingRequestAsync(client);
        var createBookingResponse = await client.PostAsJsonAsync("/api/bookings", bookingRequest, TestJson.Options);
        createBookingResponse.EnsureSuccessStatusCode();
        var booking = await ReadJsonAsync<BookingDetailDto>(createBookingResponse);

        var statusResponse = await client.PatchAsJsonAsync(
            $"/api/bookings/{booking.Id}/status",
            new UpdateBookingStatusRequest(BookingStatus.Confirmed),
            TestJson.Options);
        statusResponse.EnsureSuccessStatusCode();
        var updatedBooking = await ReadJsonAsync<BookingDetailDto>(statusResponse);
        Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);

        var staff = await client.GetFromJsonAsync<List<UserDto>>("/api/users/staff", TestJson.Options);
        Assert.NotNull(staff);
        var staffUser = staff.First();

        var assignmentRequest = new UpsertStaffAssignmentRequest(
            booking.Id,
            staffUser.Id,
            StaffRole.Supervisor,
            new DateTimeOffset(booking.EventDate.ToDateTime(TimeOnly.Parse("16:00")), TimeSpan.Zero),
            new DateTimeOffset(booking.EventDate.ToDateTime(TimeOnly.Parse("23:00")), TimeSpan.Zero),
            AssignmentStatus.Assigned,
            "Lead floor operations.");

        var assignmentResponse = await client.PostAsJsonAsync("/api/staff-assignments", assignmentRequest, TestJson.Options);
        assignmentResponse.EnsureSuccessStatusCode();
        var assignment = await ReadJsonAsync<StaffAssignmentDto>(assignmentResponse);
        Assert.Equal(staffUser.Id, assignment.StaffUserId);

        await AuthorizeAsAsync(client, staffUser.Email);
        var noteResponse = await client.PostAsJsonAsync(
            "/api/shift-notes",
            new CreateShiftNoteRequest(booking.Id, null, ShiftNoteType.SetupCompleted, "Setup checklist completed before doors.", false),
            TestJson.Options);

        noteResponse.EnsureSuccessStatusCode();
        var note = await ReadJsonAsync<ShiftNoteDto>(noteResponse);
        Assert.Equal(staffUser.Id, note.StaffUserId);
        Assert.Equal(ShiftNoteType.SetupCompleted, note.NoteType);
    }

    [Fact]
    public async Task Booking_validation_rejects_invalid_time_window()
    {
        await using var factory = new VenueOpsApiFactory();
        using var client = factory.CreateClient();
        await AuthorizeAsAsync(client, "manager@venueops.local");

        var request = await BuildBookingRequestAsync(client, start: "18:00", end: "17:00");
        var response = await client.PostAsJsonAsync("/api/bookings", request, TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("End time must be later than start time", body);
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, DemoPassword),
            TestJson.Options);

        response.EnsureSuccessStatusCode();
        return await ReadJsonAsync<LoginResponse>(response);
    }

    private static async Task AuthorizeAsAsync(HttpClient client, string email)
    {
        var login = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
    }

    private static async Task<UpsertBookingRequest> BuildBookingRequestAsync(
        HttpClient client,
        string start = "18:00",
        string end = "22:00")
    {
        var clients = await client.GetFromJsonAsync<List<ClientDto>>("/api/clients", TestJson.Options);
        var venues = await client.GetFromJsonAsync<List<VenueRoomDto>>("/api/venues", TestJson.Options);

        Assert.NotNull(clients);
        Assert.NotNull(venues);

        var venue = venues.First(x => x.Capacity >= 120);
        return new UpsertBookingRequest(
            "Portfolio Demo Reception",
            clients.First().Id,
            venue.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            TimeOnly.Parse(start),
            TimeOnly.Parse(end),
            120,
            "Corporate",
            BookingStatus.Inquiry,
            "Created by integration test.");
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<T>(stream, TestJson.Options);
        return result ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
    }
}
