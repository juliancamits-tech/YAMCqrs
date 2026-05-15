using Microsoft.Extensions.Configuration;

namespace YAMCqrs.BackgroundWorker.Storage.MondgoDb;

public class BackgroundWorkerMongoConfiguration()
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }

    public string GetConnectionString(IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(ConnectionString, nameof(ConnectionString));

        if (ConnectionString.StartsWith("cs_", StringComparison.InvariantCultureIgnoreCase))
        {
            var aux = ConnectionString["cs_".Length..];
            return cfg.GetConnectionString(aux) ?? throw new InvalidOperationException($"Connection string not found in configuration for key '{ConnectionString}'");
        }

        return ConnectionString;
    }

    public string GetDatabaseName()
    {
        ArgumentNullException.ThrowIfNullOrEmpty(DatabaseName, nameof(DatabaseName));

        return DatabaseName;
    }
}