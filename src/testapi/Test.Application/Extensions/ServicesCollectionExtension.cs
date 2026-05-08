using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YAMCqrs.Core;
using YAMCqrs.ServiceBus.Core;

namespace Test.Application.Extensions;

/// <summary>
/// Provides extension methods for configuring application services.
/// </summary>
public static class ServicesCollectionExtension
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The configuration instance.</param>
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCqrs();
        services.AddServiceBusCqrs();
    }
}
