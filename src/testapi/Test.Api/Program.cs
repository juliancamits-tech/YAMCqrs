using Test.Application.Extensions;
using Test.Infra.Extensions;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var configuration = builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
    builder.Services.AddLogging();

    builder.Services.AddApplication(configuration);
    builder.Services.AddInfra(configuration);

    var app = builder.Build();

    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while building the application: {ex.Message}");
    throw;
}