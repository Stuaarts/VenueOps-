using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VenueOps.Api.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VenueOpsDbContext>
{
    public VenueOpsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VenueOpsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=venueops;Username=venueops;Password=venueops_dev_password")
            .Options;

        return new VenueOpsDbContext(options);
    }
}
