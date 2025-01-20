using BdziamPak.PackageModel;
using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

/// <summary>
///     Endpoint for registering BdziamPak metadata.
/// </summary>
public class RegisterBdziamPakEndpoint(BdziamPakIndexService indexService, IConfiguration configuration)
    : Endpoint<BdziamPakMetadata>
{
    /// <summary>
    ///     Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        // Sets the HTTP method to POST and the route to "/register".
        Post("/register");
        // Allows anonymous access to this endpoint.
        AllowAnonymous();
    }

    /// <summary>
    ///     Handles the incoming request to register BdziamPak metadata.
    /// </summary>
    /// <param name="req">The BdziamPak metadata to be registered.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(BdziamPakMetadata req, CancellationToken ct)
    {
        var apiKey = configuration["Auth:ApiKey"];
        var providedApiKey = HttpContext.Request.Headers["ApiKey"].FirstOrDefault();

        if (apiKey != providedApiKey)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        // Registers the metadata using the index service.
        await indexService.RegisterMetadataAsync(req);
        // Sends an OK response.
        await SendOkAsync(ct);
    }
}