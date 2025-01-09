using System.Text.Json;
using BdziamPak.Packages.Index.Model;
using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.PakRepoApi.Services;

public class BdziamPakIndexService
{
    private const string IndexJson = "index.json";
    private readonly string _indexFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly BdziamPakSourceIndex _source;

    public BdziamPakIndexService(string dataDirectory)
    {
        _indexFilePath = Path.Combine(dataDirectory, IndexJson);

        if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory);

        if (!File.Exists(_indexFilePath))
        {
            _source = new BdziamPakSourceIndex { Paks = new List<BdziamPakMetadata>() };
            File.WriteAllText(_indexFilePath, JsonSerializer.Serialize(_source));
        }
        else
        {
            var json = File.ReadAllText(_indexFilePath);
            _source = JsonSerializer.Deserialize<BdziamPakSourceIndex>(json) ?? new BdziamPakSourceIndex
                { Paks = new List<BdziamPakMetadata>() };
        }
    }

    public async Task RegisterMetadataAsync(BdziamPakMetadata metadata)
    {
        await _semaphore.WaitAsync();
        try
        {
            _source.Paks.Add(metadata);
            await SaveIndexAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveMetadataAsync(string bdziamPakId, string version)
    {
        await _semaphore.WaitAsync();
        try
        {
            var metadata = _source.Paks.FirstOrDefault(p => p.BdziamPakId == bdziamPakId && p.Version == version);
            if (metadata != null)
            {
                _source.Paks.Remove(metadata);
                await SaveIndexAsync();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task EditSourceAsync(string newName, string newDescription)
    {
        await _semaphore.WaitAsync();
        try
        {
            _source.Name = newName;
            _source.Description = newDescription;
            await SaveIndexAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveIndexAsync()
    {
        var json = JsonSerializer.Serialize(_source);
        await File.WriteAllTextAsync(_indexFilePath, json);
    }

    public string GetIndexFilePath()
    {
        return _indexFilePath;
    }
}