using Microsoft.Extensions.Configuration;

namespace YAMCqrs.Core.Extensions;

public static class ConfigurationExtension
{
    public static string FindConnectionString(this IConfiguration configuration, string connectionString)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

        if (connectionString.StartsWith("cs_", StringComparison.InvariantCultureIgnoreCase))
        {
            var aux = connectionString["cs_".Length..];
            return configuration.GetConnectionString(aux) ?? throw new InvalidOperationException($"Connection string not found in configuration for key '{connectionString}'");
        }

        return connectionString;
    }
}
