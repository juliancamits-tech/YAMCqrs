using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Infra.Extensions;

/// <summary>
/// Provides extension methods for configuring infrastructure services.
/// </summary>
public static class ServicesCollectionExtension
{
    /// <summary>
    /// Adds infrastructure services to the specified <see cref="IServiceCollection"/>.
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// </summary>
#pragma warning disable SA1611 // Element parameters should be documented
    public static void AddInfra(this IServiceCollection services, IConfiguration configuration)
#pragma warning restore SA1611 // Element parameters should be documented
    {
    }
}
