using YAMCqrs.Core.Abstractions.Queries;

namespace Test.Application.QueryTest;

/// <summary>
/// Represents a query with a name that returns a string result when executed.
/// </summary>
public class MyQuery : IQuery<string>
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}