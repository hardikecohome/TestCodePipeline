using System;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Culture;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class CultureSetterManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ILoggingService _loggingService;
        private readonly IHttpApiClient _client;
        private readonly ICultureManager _cultureManager;

        public CultureSetterManager(ICultureManager cultureManager, IHttpApiClient client, IDictionaryServiceAgent dictionaryServiceAgent, ILoggingService loggingService)
        {
            _cultureManager = cultureManager;
            _client = client;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _loggingService = loggingService;
        }

        public async Task ChangeCulture(string culture)
        {
            try
            {
                await _dictionaryServiceAgent.ChangeDealerCulture(culture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't update culture {culture} for user {HttpContext.Current.User?.Identity?.Name}", ex);
            }
            SetCulture(culture);
        }

        public void SetCulture(string culture, bool createCookie = true)
        {
            _cultureManager.SetCulture(culture);
            //var filteredCulture = CultureHelper.FilterCulture(culture);
            //_client.Client.DefaultRequestHeaders.AcceptLanguage.Clear();
            //_client.Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(filteredCulture));
        }
    }
}