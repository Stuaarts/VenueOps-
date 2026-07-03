using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Data;

public sealed class VenueOpsDbContext(DbContextOptions<VenueOpsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<VenueRoom> VenueRooms => Set<VenueRoom>();
    public DbSet<EventBooking> EventBookings => Set<EventBooking>();
    public DbSet<StaffAssignment> StaffAssignments => Set<StaffAssignment>();
    public DbSet<ShiftNote> ShiftNotes => Set<ShiftNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var userRoleConverter = new EnumToStringConverter<UserRole>();
        var bookingStatusConverter = new EnumToStringConverter<BookingStatus>();
        var staffRoleConverter = new EnumToStringConverter<StaffRole>();
        var assignmentStatusConverter = new EnumToStringConverter<AssignmentStatus>();
        var shiftNoteTypeConverter = new EnumToStringConverter<ShiftNoteType>();

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Role).HasConversion(userRoleConverter).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasIndex(x => x.Email);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.ContactName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.Property(x => x.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<VenueRoom>(entity =>
        {
            entity.ToTable("venue_rooms");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<EventBooking>(entity =>
        {
            entity.ToTable("event_bookings");
            entity.HasIndex(x => x.EventDate);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.VenueRoomId, x.EventDate });
            entity.Property(x => x.EventName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Status).HasConversion(bookingStatusConverter).HasMaxLength(32).IsRequired();
            entity.Property(x => x.InternalNotes).HasMaxLength(2000);
            entity.HasOne(x => x.Client)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VenueRoom)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.VenueRoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StaffAssignment>(entity =>
        {
            entity.ToTable("staff_assignments");
            entity.HasIndex(x => new { x.EventBookingId, x.StaffUserId });
            entity.HasIndex(x => x.ShiftStart);
            entity.Property(x => x.Role).HasConversion(staffRoleConverter).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion(assignmentStatusConverter).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.EventBooking)
                .WithMany(x => x.StaffAssignments)
                .HasForeignKey(x => x.EventBookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.StaffUser)
                .WithMany(x => x.StaffAssignments)
                .HasForeignKey(x => x.StaffUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShiftNote>(entity =>
        {
            entity.ToTable("shift_notes");
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.NoteType).HasConversion(shiftNoteTypeConverter).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(1600).IsRequired();
            entity.HasOne(x => x.EventBooking)
                .WithMany(x => x.ShiftNotes)
                .HasForeignKey(x => x.EventBookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.StaffUser)
                .WithMany(x => x.ShiftNotes)
                .HasForeignKey(x => x.StaffUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
