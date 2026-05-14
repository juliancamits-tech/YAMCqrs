namespace Test.Aspire.Extensions;

internal static class DistributedApplicationBuilderExtensions
{
    public const string USERNAME = "admin";
    public const string PASSWORD = "StrongPassword123++";

    private static IResourceBuilder<ParameterResource>? saUser;
    private static IResourceBuilder<ParameterResource>? saPwd;

    public static IResourceBuilder<ParameterResource> GetSaUser(this IDistributedApplicationBuilder builder)
    {
        return saUser ??= builder.AddParameter("dbUser", USERNAME)
                .WithDescription("UserName");
    }

    public static IResourceBuilder<ParameterResource> GetSaPass(this IDistributedApplicationBuilder builder)
    {
        return saPwd ??= builder.AddParameter("dbPass", PASSWORD)
                .WithDescription("Password");
    }
}