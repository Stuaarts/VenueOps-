using Microsoft.Extensions.Configuration;
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

    [Fact]
    public void Resolve_prefers_database_url_over_local_appsettings_connection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=venueops;Username=venueops;Password=local",
                ["DATABASE_URL"] = "postgresql://venueops:hosted@db.example.com/venueops?sslmode=require"
            })
            .Build();

        var connectionString = DatabaseConnectionString.Resolve(configuration);

        Assert.Contains("Host=db.example.com", connectionString);
        Assert.Contains("Password=hosted", connectionString);
        Assert.DoesNotContain("Host=localhost", connectionString);
    }
}
