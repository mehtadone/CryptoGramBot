using Microsoft.Extensions.Caching.Memory;
using System;

namespace CryptoGramBot.Services.Cache
{
    public class MemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        #region MemoryCacheService

        public T Get<T>(string key)
        {
            if(!IsSet(key))
            {
                return default(T);
            }

            return _memoryCache.Get<T>(key);
        }

        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
            {
                return;
            }

            _memoryCache.Set(key, data, TimeSpan.FromMinutes(cacheTime));
        }

        public bool IsSet(string key)
        {
            object value = null;

            return _memoryCache.TryGetValue(key, out value);
        }

        #endregion
    }
}
