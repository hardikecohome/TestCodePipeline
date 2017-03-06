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
        public TValue Get<TValue>(string cacheKey, int? durationInMinutes, Func<TValue> getItemCallback) where TValue : class
        {
            TValue item = MemoryCache.Default.Get(cacheKey) as TValue;
            if (item == null)
            {
                item = getItemCallback();
                if (item == null) { return null; }
                CacheItemPolicy policy = new CacheItemPolicy();
                if (durationInMinutes.HasValue)
                {
                    policy.SlidingExpiration = TimeSpan.FromMinutes(durationInMinutes.Value);
                }
                MemoryCache.Default.Add(cacheKey, item, policy);
            }
            return item;
        }

        public async Task<TValue> GetAsync<TValue>(string cacheKey, int? durationInMinutes, Func<Task<TValue>> getItemCallback)
            where TValue : class
        {
            TValue item = MemoryCache.Default.Get(cacheKey) as TValue;
            if (item == null)
            {
                item = await getItemCallback();
                if (item == null) { return null; }
                CacheItemPolicy policy = new CacheItemPolicy();
                if (durationInMinutes.HasValue)
                {
                    policy.SlidingExpiration = TimeSpan.FromMinutes(durationInMinutes.Value);
                }
                MemoryCache.Default.Add(cacheKey, item, policy);                
            }
            return item;
        }

        public void Remove(string cacheKey)
        {
            MemoryCache.Default.Remove(cacheKey);
        }
    }
}
