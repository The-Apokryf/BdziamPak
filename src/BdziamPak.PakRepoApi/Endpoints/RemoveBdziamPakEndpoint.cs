using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

/// <summary>
///     Endpoint to remove a BdziamPak by its ID and version.
/// </summary>
/// <param name="indexService">Service to manage BdziamPak index.</param>
/// <param name="configuration">Configuration settings.</param>
public class RemoveBdziamPakEndpoint(BdziamPakIndexService indexService, IConfiguration configuration)
    : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures the endpoint route and allows anonymous access.
    /// </summary>
    public override void Configure()
    {
        Delete("/remove/{bdziamPakId}@{version}");
        AllowAnonymous();
    }

    /// <summary>
    ///     Handles the request to remove a BdziamPak.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var apiKey = configuration["Auth:ApiKey"];
        var providedApiKey = HttpContext.Request.Headers["ApiKey"].FirstOrDefault();

        if (apiKey != providedApiKey)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var bdziamPakId = Route<string>("bdziamPakId");
        var version = Route<string>("version");

        await indexService.RemoveVersionAsync(bdziamPakId, version);
        await SendOkAsync(ct);
    }
}