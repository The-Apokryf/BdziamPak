using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

/// <summary>
/// Represents an API endpoint for serving the BdziamPak package index file.
/// </summary>
public class BdziamPakIndexEndpoint(BdziamPakIndexService indexService) : EndpointWithoutRequest
{
    /// <summary>
    /// Configures the endpoint's route and access settings.
    /// </summary>
    public override void Configure()
    {
        // Define the route for the endpoint
        Get("/index.json");
        
        // Allow anonymous access to this endpoint
        AllowAnonymous();
    }

    /// <summary>
    /// Handles the HTTP request and serves the package index file.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the file path for the package index
        var filePath = indexService.GetIndexFilePath();
        
        // Send the file as a response to the client
        await SendFileAsync(new FileInfo(filePath), cancellation: ct);
    }
}