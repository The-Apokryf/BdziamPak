using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

public class BdziamPakIndex(BdziamPakIndexService indexService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/index.json");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var filePath = indexService.GetIndexFilePath();
        await SendFileAsync(new FileInfo(filePath), cancellation: ct);
    }
}