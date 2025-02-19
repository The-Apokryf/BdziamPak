using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

/// <summary>
///     Endpoint for editing a BdziamPak source.
/// </summary>
/// <param name="indexService">Service for managing BdziamPak index.</param>
public class EditBdziamPakSourceEndpoint(BdziamPakIndexService indexService, IConfiguration configuration)
    : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        Put("/edit-source");
        AllowAnonymous();
    }

    /// <summary>
    ///     Handles the request to edit a BdziamPak source.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var newName = Query<string>("newName");
        var newDescription = Query<string>("newDescription");

        await indexService.EditSourceAsync(newName, newDescription);
        await SendOkAsync(ct);
    }
}