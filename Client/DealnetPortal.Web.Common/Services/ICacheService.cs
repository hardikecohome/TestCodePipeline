using System;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Services
{
    public interface ICacheService
    {
        TValue Get<TValue>(string cacheKey, int? durationInMinutes, Func<TValue> getItemCallback);
        Task<TValue> GetAsync<TValue>(string cacheKey, int? durationInMinutes, Func<Task<TValue>> getItemCallback);
        void Remove(string cacheKey);
    }
}
