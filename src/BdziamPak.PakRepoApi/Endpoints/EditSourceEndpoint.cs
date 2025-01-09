using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

namespace BdziamPak.PakRepoApi.Endpoints;

public class EditBdziamPakSource(BdziamPakIndexService indexService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Put("/edit-source");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newName = Query<string>("newName");
        var newDescription = Query<string>("newDescription");

        await indexService.EditSourceAsync(newName, newDescription);
        await SendOkAsync(ct);
    }
}