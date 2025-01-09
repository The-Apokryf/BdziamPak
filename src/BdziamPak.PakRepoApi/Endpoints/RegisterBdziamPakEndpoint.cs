using BdziamPak.Packages.Packaging.Model;
using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

public class RegisterBdziamPakEndpoint(BdziamPakIndexService indexService) : Endpoint<BdziamPakMetadata>
{
    public override void Configure()
    {
        Post("/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(BdziamPakMetadata req, CancellationToken ct)
    {
        await indexService.RegisterMetadataAsync(req);
        await SendOkAsync(ct);
    }
}