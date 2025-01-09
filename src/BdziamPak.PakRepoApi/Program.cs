using BdziamPak.PakRepoApi.Services;
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new BdziamPakIndexService(Path.Combine(Directory.GetCurrentDirectory(), "data")));

var app = builder.Build();
app.UseFastEndpoints();

app.Run();