using Npgsql;

namespace VenueOps.Api.Services;

public static class DatabaseConnectionString
{
    private static readonly Dictionary<string, string> QueryKeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application_name"] = "Application Name",
        ["channel_binding"] = "Channel Binding",
        ["connect_timeout"] = "Timeout",
        ["sslcert"] = "SSL Certificate",
        ["sslkey"] = "SSL Key",
        ["sslmode"] = "SSL Mode",
        ["sslrootcert"] = "Root Certificate"
    };

    public static string Resolve(IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return NormalizePostgresUrl(configuredConnectionString);
        }

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return NormalizePostgresUrl(databaseUrl);
        }

        throw new InvalidOperationException("A PostgreSQL connection string is not configured.");
    }

    public static string NormalizePostgresUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return value;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'))
        };

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var userParts = uri.UserInfo.Split(':', 2);
            builder.Username = Uri.UnescapeDataString(userParts[0]);
            if (userParts.Length > 1)
            {
                builder.Password = Uri.UnescapeDataString(userParts[1]);
            }
        }

        foreach (var (key, parameterValue) in ParseQueryString(uri.Query))
        {
            var keyword = QueryKeywordMap.TryGetValue(key, out var mappedKeyword)
                ? mappedKeyword
                : key.Replace('_', ' ');

            try
            {
                builder[keyword] = parameterValue;
            }
            catch (ArgumentException)
            {
                // Ignore provider-specific URL query parameters that Npgsql does not understand.
            }
        }

        return builder.ConnectionString;
    }

    private static IEnumerable<(string Key, string Value)> ParseQueryString(string query)
    {
        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length == 2)
            {
                yield return (
                    Uri.UnescapeDataString(pair[0]),
                    Uri.UnescapeDataString(pair[1]));
            }
        }
    }
}
