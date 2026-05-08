using Bogus;
using Microsoft.AspNetCore.Mvc;
using Test.Application.CommandTest;
using Test.Application.DomainEvent;
using Test.Application.QueryTest;
using YAMCqrs.Core.Abstractions;

namespace Test.Api;

/// <summary>
/// Handles API requests for MyCommand operations.
/// <param name="dispatcher">Dispatcher used to send commands.</param>
/// </summary>
[Route("api/[controller]")]
[ApiController]
public sealed class MyController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Sends a MyCommand request through the dispatcher.
    /// </summary>
    /// <returns>Returns 200 OK if the command is successful, otherwise returns 400 Bad Request with the error message.</returns>
    [HttpPost("command")]
    public async Task<IActionResult> PostCommand()
    {
        var f = new Faker();

        var result = await dispatcher.SendAsync(new MyCommand { Name = f.Name.FirstName() }, CancellationToken.None);

        if (result.IsFailure)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Sends a MyCommand request through the dispatcher.
    /// </summary>
    /// <returns>Returns 200 OK if the command is successful, otherwise returns 400 Bad Request with the error message.</returns>
    [HttpPost("query")]
    public async Task<IActionResult> PostQuery()
    {
        var f = new Faker();

        var result = await dispatcher.QueryAsync(new MyQuery { Name = f.Name.FirstName() }, CancellationToken.None);

        if (result.IsFailure)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Sends a MyCommand request through the dispatcher.
    /// </summary>
    /// <returns>Returns 200 OK if the command is successful, otherwise returns 400 Bad Request with the error message.</returns>
    [HttpPost("domainEvent")]
    public async Task<IActionResult> PostDomainEvent()
    {
        var f = new Faker();

        var result = await dispatcher.SendAsync(new DomainEventCommand { Name = f.Name.FirstName() }, CancellationToken.None);

        if (result.IsFailure)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }
}