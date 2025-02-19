using System.Net.Http.Json;
using BdziamPak.PackageModel;
using BdziamPak.Sources.Model;

namespace BdziamPak.Client;

/// <summary>
///     Represents a client for interacting with the BdziamPak API.
///     <param name="baseAddress">The base address of the BdziamPak API.</param>
/// </summary>
public class BdziamPakClient(string baseAddress)
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(baseAddress) };

    /// <summary>
    ///     Registers metadata with the BdziamPak API.
    /// </summary>
    /// <param name="metadata">The metadata to register.</param>
    public async Task RegisterMetadataAsync(BdziamPakMetadata metadata)
    {
        var response = await _httpClient.PostAsJsonAsync("/register", metadata);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Removes metadata from the BdziamPak API.
    /// </summary>
    /// <param name="bdziamPakId">The ID of the metadata to remove.</param>
    /// <param name="version">The version of the metadata to remove.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    public async Task RemoveMetadataAsync(string bdziamPakId, string version, string apiKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/remove/{bdziamPakId}@{version}");
        request.Headers.Add("ApiKey", apiKey);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Edits the source information in the BdziamPak API.
    /// </summary>
    /// <param name="newName">The new name for the source.</param>
    /// <param name="newDescription">The new description for the source.</param>
    public async Task EditSourceAsync(string newName, string newDescription)
    {
        var query = $"?newName={newName}&newDescription={newDescription}";
        var response = await _httpClient.PutAsync($"/edit-source{query}", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    ///     Gets the index from the BdziamPak API.
    /// </summary>
    /// <returns>The BdziamPak source index.</returns>
    public async Task<BdziamPakSourceIndex> GetIndexAsync()
    {
        var response = await _httpClient.GetAsync("/index.json");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BdziamPakSourceIndex>();
    }
}