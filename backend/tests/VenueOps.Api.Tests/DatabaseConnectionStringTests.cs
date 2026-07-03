using VenueOps.Api.Services;

namespace VenueOps.Api.Tests;

public sealed class DatabaseConnectionStringTests
{
    [Fact]
    public void NormalizePostgresUrl_converts_provider_urls_for_npgsql()
    {
        var connectionString = DatabaseConnectionString.NormalizePostgresUrl(
            "postgresql://venue%40ops:secret%21@ep-example.neon.tech:6543/venueops?sslmode=require&channel_binding=require");

        Assert.Contains("Host=ep-example.neon.tech", connectionString);
        Assert.Contains("Port=6543", connectionString);
        Assert.Contains("Database=venueops", connectionString);
        Assert.Contains("Username=venue@ops", connectionString);
        Assert.Contains("Password=secret!", connectionString);
        Assert.Contains("SSL Mode=Require", connectionString);
        Assert.Contains("Channel Binding=Require", connectionString);
    }

    [Fact]
    public void NormalizePostgresUrl_keeps_keyword_connection_strings()
    {
        const string connectionString = "Host=localhost;Database=venueops;Username=venueops;Password=secret";

        var normalized = DatabaseConnectionString.NormalizePostgresUrl(connectionString);

        Assert.Equal(connectionString, normalized);
    }
}
