using System.Diagnostics.CodeAnalysis;

namespace YAMCqrs.EventBus.Core;

[ExcludeFromCodeCoverage(Justification = "Just Propertys nothing to test.")]
internal static class InternalSharedStatics
{
    /// <summary>
    /// Maximum number of retry attempts for failed events
    /// </summary>
    public static int PublishEventMaxRetryCount = 3;
    /// <summary>
    /// Maximum number of retry attempts for failed events
    /// </summary>
    public static int SubscribeEventMaxRetryCount = 3;

}
