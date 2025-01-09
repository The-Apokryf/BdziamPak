using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

public class RemoveBdziamPakEndpoint(BdziamPakIndexService indexService, IConfiguration configuration)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/remove/{bdziamPakId}@{version}");
        AllowAnonymous();
    }

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

        await indexService.RemoveMetadataAsync(bdziamPakId, version);
        await SendOkAsync(ct);
    }
}