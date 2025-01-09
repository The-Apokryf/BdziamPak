using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BdziamPak.Packages.Index.Model;
using BdziamPak.Packages.Packaging.Model;

public class BdziamPakClient(string baseAddress)
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(baseAddress) };

    public async Task RegisterMetadataAsync(BdziamPakMetadata metadata)
    {
        var response = await _httpClient.PostAsJsonAsync("/register", metadata);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveMetadataAsync(string bdziamPakId, string version, string apiKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/remove/{bdziamPakId}@{version}");
        request.Headers.Add("ApiKey", apiKey);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task EditSourceAsync(string newName, string newDescription)
    {
        var query = $"?newName={newName}&newDescription={newDescription}";
        var response = await _httpClient.PutAsync($"/edit-source{query}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BdziamPakSourceIndex> GetIndexAsync()
    {
        var response = await _httpClient.GetAsync("/index.json");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BdziamPakSourceIndex>();
    }
}