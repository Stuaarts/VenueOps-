using Microsoft.EntityFrameworkCore;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Data;

public static class DemoDataSeeder
{
    private const string DemoPassword = "VenueOpsDemo!2026";

    public static async Task SeedAsync(VenueOpsDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.Date);
        var users = new List<AppUser>
        {
            NewUser("Avery Chen", "admin@venueops.local", UserRole.Admin),
            NewUser("Morgan Patel", "manager@venueops.local", UserRole.Manager),
            NewUser("Riley Demo", "demo@venueops.local", UserRole.Demo),
            NewUser("Sam Rivera", "sam.staff@venueops.local", UserRole.Staff),
            NewUser("Jordan Kim", "jordan.staff@venueops.local", UserRole.Staff),
            NewUser("Taylor Brooks", "taylor.staff@venueops.local", UserRole.Staff),
            NewUser("Casey Nguyen", "casey.staff@venueops.local", UserRole.Staff),
            NewUser("Jamie O'Neil", "jamie.staff@venueops.local", UserRole.Staff),
            NewUser("Devon Lee", "devon.staff@venueops.local", UserRole.Staff),
            NewUser("Alex Carter", "alex.staff@venueops.local", UserRole.Staff),
            NewUser("Priya Shah", "priya.staff@venueops.local", UserRole.Staff),
            NewUser("Noah Green", "noah.staff@venueops.local", UserRole.Staff),
            NewUser("Mia Wilson", "mia.staff@venueops.local", UserRole.Staff)
        };

        var clients = new List<Client>
        {
            NewClient("Northstar Finance", "Elena Morris", "elena.morris@example.com", "555-0134", "Annual corporate events and executive dinners."),
            NewClient("Harlow & Reed", "Marcus Reed", "marcus.reed@example.com", "555-0188", "Wedding planning partner with frequent room blocks."),
            NewClient("Civic Arts Council", "Dina Alvarez", "dina.alvarez@example.com", "555-0119", "Nonprofit galas and community receptions."),
            NewClient("Bridgeway Labs", "Nora Singh", "nora.singh@example.com", "555-0192", "Conferences and product launch events.")
        };

        var venues = new List<VenueRoom>
        {
            new()
            {
                Name = "Grand Ballroom",
                Location = "Main Floor",
                Capacity = 420,
                Notes = "Divisible ballroom with stage, dance floor, and service corridor."
            },
            new()
            {
                Name = "Riverside Terrace",
                Location = "Outdoor Level",
                Capacity = 180,
                Notes = "Weather-dependent outdoor reception area with tenting option."
            },
            new()
            {
                Name = "Summit Conference Hall",
                Location = "Second Floor",
                Capacity = 260,
                Notes = "AV-ready hall with breakout room access."
            }
        };

        var bookings = new List<EventBooking>
        {
            NewBooking("Northstar Leadership Summit", clients[0], venues[2], today.AddDays(2), "08:00", "15:30", 180, "Corporate", BookingStatus.Confirmed, "VIP breakfast service at 08:30."),
            NewBooking("Harlow Reed Wedding Reception", clients[1], venues[0], today.AddDays(5), "16:00", "23:30", 245, "Wedding", BookingStatus.Confirmed, "Florist load-in after 12:00."),
            NewBooking("Civic Arts Donor Gala", clients[2], venues[0], today.AddDays(9), "17:00", "22:00", 310, "Gala", BookingStatus.Inquiry, "Client requested revised floor plan."),
            NewBooking("Bridgeway Product Launch", clients[3], venues[2], today.AddDays(12), "10:00", "16:00", 210, "Conference", BookingStatus.Confirmed, "Requires registration tables and green room."),
            NewBooking("Terrace Cocktail Preview", clients[1], venues[1], today.AddDays(15), "18:00", "21:00", 95, "Private", BookingStatus.InProgress, "Rain call at 10:00 event day."),
            NewBooking("Community Volunteer Lunch", clients[2], venues[1], today.AddDays(-3), "11:00", "14:00", 120, "Private", BookingStatus.Completed, "Completed with final guest count of 126."),
            NewBooking("Finance Board Dinner", clients[0], venues[0], today.AddDays(20), "18:30", "22:00", 80, "Corporate", BookingStatus.Inquiry, "Menu tasting pending."),
            NewBooking("Bridgeway Training Workshop", clients[3], venues[2], today.AddDays(27), "09:00", "13:00", 140, "Conference", BookingStatus.Cancelled, "Client moved to virtual event.")
        };

        db.Users.AddRange(users);
        db.Clients.AddRange(clients);
        db.VenueRooms.AddRange(venues);
        db.EventBookings.AddRange(bookings);
        await db.SaveChangesAsync(cancellationToken);

        var staff = users.Where(x => x.Role == UserRole.Staff).ToList();
        var assignments = new List<StaffAssignment>
        {
            NewAssignment(bookings[0], staff[0], StaffRole.Supervisor, 7, 16, AssignmentStatus.Confirmed, "Lead registration and room turnover."),
            NewAssignment(bookings[0], staff[1], StaffRole.Setup, 6, 12, AssignmentStatus.Confirmed, "AV and classroom setup."),
            NewAssignment(bookings[0], staff[2], StaffRole.Server, 8, 15, AssignmentStatus.Assigned, "Breakfast and lunch service."),
            NewAssignment(bookings[1], staff[3], StaffRole.Supervisor, 15, 24, AssignmentStatus.Confirmed, "Reception lead."),
            NewAssignment(bookings[1], staff[4], StaffRole.Bartender, 16, 24, AssignmentStatus.Confirmed, "Main bar."),
            NewAssignment(bookings[1], staff[5], StaffRole.Server, 16, 23, AssignmentStatus.Assigned, "Table service."),
            NewAssignment(bookings[2], staff[6], StaffRole.Setup, 12, 18, AssignmentStatus.Assigned, "Gala floor plan setup."),
            NewAssignment(bookings[2], staff[7], StaffRole.Security, 17, 23, AssignmentStatus.Assigned, "Lobby and donor entrance."),
            NewAssignment(bookings[3], staff[8], StaffRole.Supervisor, 9, 17, AssignmentStatus.Confirmed, "Launch event lead."),
            NewAssignment(bookings[3], staff[9], StaffRole.Kitchen, 8, 16, AssignmentStatus.Assigned, "Break service."),
            NewAssignment(bookings[4], staff[0], StaffRole.Bartender, 17, 22, AssignmentStatus.Assigned, "Terrace bar."),
            NewAssignment(bookings[4], staff[2], StaffRole.Server, 17, 22, AssignmentStatus.Assigned, "Passed appetizers."),
            NewAssignment(bookings[5], staff[4], StaffRole.Supervisor, 10, 15, AssignmentStatus.Completed, "Lunch event complete."),
            NewAssignment(bookings[5], staff[5], StaffRole.Server, 10, 15, AssignmentStatus.Completed, "Guest count adjustment handled."),
            NewAssignment(bookings[6], staff[7], StaffRole.Server, 19, 22, AssignmentStatus.Assigned, "Dinner service pending confirmation.")
        };

        db.StaffAssignments.AddRange(assignments);
        await db.SaveChangesAsync(cancellationToken);

        var notes = new List<ShiftNote>
        {
            NewNote(bookings[0], staff[0], ShiftNoteType.SetupCompleted, "Registration tables and signage are staged outside Summit Hall.", true),
            NewNote(bookings[0], staff[1], ShiftNoteType.SuppliesIssue, "Need two extra water stations before lunch service."),
            NewNote(bookings[1], staff[3], ShiftNoteType.ClientRequest, "Client requested sweetheart table moved closer to the dance floor."),
            NewNote(bookings[1], staff[4], ShiftNoteType.General, "Bar inventory confirmed with beverage manager."),
            NewNote(bookings[2], staff[6], ShiftNoteType.ClientRequest, "Awaiting approval on revised donor wall placement."),
            NewNote(bookings[2], staff[7], ShiftNoteType.StaffingIssue, "Security may need one additional person for VIP arrivals."),
            NewNote(bookings[3], staff[8], ShiftNoteType.SetupCompleted, "Main stage and demo counter measurements confirmed."),
            NewNote(bookings[3], staff[9], ShiftNoteType.SuppliesIssue, "Coffee urn count increased from 4 to 6."),
            NewNote(bookings[4], staff[0], ShiftNoteType.General, "Weather backup room held through noon on event day."),
            NewNote(bookings[4], staff[2], ShiftNoteType.ClientRequest, "Client asked for citrus-free mocktail option."),
            NewNote(bookings[5], staff[4], ShiftNoteType.GuestCountChanged, "Final count changed from 120 to 126 at check-in."),
            NewNote(bookings[5], staff[5], ShiftNoteType.Closing, "Terrace reset completed and linens returned."),
            NewNote(bookings[6], staff[7], ShiftNoteType.General, "Menu tasting follow-up scheduled with catering."),
            NewNote(bookings[0], staff[2], ShiftNoteType.General, "Extra extension cords staged near breakout hallway."),
            NewNote(bookings[1], staff[5], ShiftNoteType.Incident, "Minor spill near service station cleaned immediately.")
        };

        db.ShiftNotes.AddRange(notes);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static AppUser NewUser(string fullName, string email, UserRole role) => new()
    {
        FullName = fullName,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
        Role = role,
        IsActive = true
    };

    private static Client NewClient(string name, string contact, string email, string phone, string notes) => new()
    {
        Name = name,
        ContactName = contact,
        Email = email,
        Phone = phone,
        Notes = notes
    };

    private static EventBooking NewBooking(
        string name,
        Client client,
        VenueRoom venue,
        DateOnly date,
        string start,
        string end,
        int guests,
        string eventType,
        BookingStatus status,
        string notes) => new()
    {
        EventName = name,
        Client = client,
        VenueRoom = venue,
        EventDate = date,
        StartTime = TimeOnly.Parse(start),
        EndTime = TimeOnly.Parse(end),
        GuestCount = guests,
        EventType = eventType,
        Status = status,
        InternalNotes = notes
    };

    private static StaffAssignment NewAssignment(
        EventBooking booking,
        AppUser user,
        StaffRole role,
        int startHourOffset,
        int endHourOffset,
        AssignmentStatus status,
        string notes)
    {
        var shiftDate = booking.EventDate.ToDateTime(TimeOnly.MinValue);
        return new StaffAssignment
        {
            EventBooking = booking,
            StaffUser = user,
            Role = role,
            ShiftStart = new DateTimeOffset(shiftDate.AddHours(startHourOffset), TimeSpan.Zero),
            ShiftEnd = new DateTimeOffset(shiftDate.AddHours(endHourOffset), TimeSpan.Zero),
            Status = status,
            Notes = notes
        };
    }

    private static ShiftNote NewNote(EventBooking booking, AppUser user, ShiftNoteType type, string body, bool isPinned = false) => new()
    {
        EventBooking = booking,
        StaffUser = user,
        NoteType = type,
        Body = body,
        IsPinned = isPinned
    };
}
