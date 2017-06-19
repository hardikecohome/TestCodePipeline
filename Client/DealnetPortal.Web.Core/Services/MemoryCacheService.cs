using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Core.Services
{
    public class MemoryCacheService : ICacheService
    {
        public TValue Get<TValue>(string cacheKey, int? durationInMinutes, Func<TValue> getItemCallback)
        {
            var objectFromCache = MemoryCache.Default.Get(cacheKey);
            if (objectFromCache != null && !(objectFromCache is TValue))
            {
                return default(TValue);
            }
            if (objectFromCache == null)
            {
                var item = getItemCallback();
                if (item == null) { return default(TValue); }
                CacheItemPolicy policy = new CacheItemPolicy();
                if (durationInMinutes.HasValue)
                {
                    policy.SlidingExpiration = TimeSpan.FromMinutes(durationInMinutes.Value);
                }
                MemoryCache.Default.Add(cacheKey, item, policy);
                return item;
            }
            return (TValue)objectFromCache;
        }

        public async Task<TValue> GetAsync<TValue>(string cacheKey, int? durationInMinutes, Func<Task<TValue>> getItemCallback)
        {
            var objectFromCache = MemoryCache.Default.Get(cacheKey);
            if (objectFromCache != null && !(objectFromCache is TValue))
            {
                return default(TValue);
            }
            if (objectFromCache == null)
            {
                var item = await getItemCallback();
                if (item == null) { return default(TValue); }
                CacheItemPolicy policy = new CacheItemPolicy();
                if (durationInMinutes.HasValue)
                {
                    policy.SlidingExpiration = TimeSpan.FromMinutes(durationInMinutes.Value);
                }
                MemoryCache.Default.Add(cacheKey, item, policy);
                return item;
            }
            return (TValue)objectFromCache;
        }

        public void Remove(string cacheKey)
        {
            MemoryCache.Default.Remove(cacheKey);
        }
    }
}
