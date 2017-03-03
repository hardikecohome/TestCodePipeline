using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Core.Services
{
    public interface ICacheService
    {
        TValue Get<TValue>(string cacheKey, int? durationInMinutes, Func<TValue> getItemCallback) where TValue : class;
        Task<TValue> GetAsync<TValue>(string cacheKey, int? durationInMinutes, Func<Task<TValue>> getItemCallback) where TValue : class;
        void Remove(string cacheKey);
    }
}
