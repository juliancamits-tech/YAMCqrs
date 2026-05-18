namespace YAMCqrs.EventBus.Core;

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
