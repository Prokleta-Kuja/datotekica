using Microsoft.Extensions.Caching.Memory;

namespace datotekica.Services;

public class CacheService
{
    readonly IMemoryCache _cache;
    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
    public Guid RegisterDownload(string path, bool isDir) => RegisterDownload(new Dictionary<string, bool> { { path, isDir } });
    public Guid RegisterDownload(Dictionary<string, bool> items)
    {
        var id = Guid.NewGuid();
        _cache.Set(id, items, TimeSpan.FromSeconds(30));
        return id;
    }
    public bool TryGetDownload(Guid id, out Dictionary<string, bool> items)
    {
        return _cache.TryGetValue<Dictionary<string, bool>>(id, out items);
    }
}