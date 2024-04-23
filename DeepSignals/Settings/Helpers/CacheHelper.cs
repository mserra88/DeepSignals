using Microsoft.Extensions.Caching.Memory;

namespace DeepSignals.Settings.Helpers
{
    public class CacheHelper
    {
        public static async Task<T> TryGetValueOrCreateAsync<T, TValue>(IMemoryCache _memoryCache, Func<TValue, CancellationToken, Task<T>> getDataFunc, string cacheKey, TValue value, TimeSpan cacheDuration)
        {
            return _memoryCache.TryGetValue(cacheKey, out T cachedData) ? cachedData : await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = cacheDuration;
                entry.Priority = CacheItemPriority.High;
                entry.Size = 1;
                return await getDataFunc.Invoke(value, CancellationToken.None);
            });
        }

        public static async Task Remove(IMemoryCache _memoryCache, string cacheKey) => _memoryCache.Remove(cacheKey);


        /*
         
                             var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);

                    _memoryCache.Set(_cacheKey, _cachedData, cacheEntryOptions);


         */
    }
}