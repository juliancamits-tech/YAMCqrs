# 13. Connection Strings

Date: 2026-05-12

## Status

Accepted

## Context

Some extensions are going to need a connection string for connect to external service like data base or service bus, we need to define how to handle this information.

## Decision

The extension that need a connection string should create a configuration class where the dev-user can put the connection string. However, since in the .NET ecosystem, using the `ConnectionStrings` array is standard and sometimes the connection string already exists there to avoid duplication, we define a special rule called "connection string proxy". If the configuration class has a string that starts with `cs_`, we assume this is a call for ***Go to find it in the ConnectionStrings*** and the configuration class should go to the `ConnectionStrings` to find the value. But where is the key? The key is the rest of the string without the `cs_` prefix, for example, `cs_kafka` is ***Go to ConnectionStrings and use the key `kafka` to find the real value***.

'''csharp
    public string GetConnectionString(IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

        if (connectionString.StartsWith("cs_", StringComparison.InvariantCultureIgnoreCase))
        {
            connectionString = connectionString.Substring("cs_".Length);
            return cfg.GetConnectionString(connectionString) ?? throw new InvalidOperationException($"Connection string not found in configuration for key '{connectionString}'";
        }

        return connectionString;
    }
'''

## Consequences

### Positives

- Avoid duplicate connection strings
- Provide a clear and efficient way to handle connection strings in .NET applications

### Negatives

- Need really good documentation to ensure users understand how to use this feature correctly.

## Future Considerations

- Expand the "connection string proxy" logic to include other configuration sources or key-value stores if needed.
- Add more comprehensive error handling to manage cases where the connection string cannot be found in any of the configured sources.