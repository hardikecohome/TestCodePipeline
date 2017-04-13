using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Helpers;

namespace DealnetPortal.Web.ServiceAgent
{
    public class DictionaryServiceAgent : TransientApiBase, IDictionaryServiceAgent
    {
        private const string ContractApi = "dict";
        private ILoggingService _loggingService;

        public DictionaryServiceAgent(ITransientHttpApiClient client, ILoggingService loggingService)
            : base(client, ContractApi)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetEquipmentTypes()
        {
            try
            {
                return await Client.GetAsync<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>>(
                            $"{_fullUri}/EquipmentTypes");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Equipment Types", ex);
                throw;
            }
        }

        public async Task<Tuple<ProvinceTaxRateDTO, IList<Alert>>> GetProvinceTaxRate(string province)
        {
            try
            {
                return await Client.GetAsync<Tuple<ProvinceTaxRateDTO, IList<Alert>>>(
                            $"{_fullUri}/{province.Trim()}/ProvinceTaxRate");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Province Tax Rate", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<ProvinceTaxRateDTO>, IList<Alert>>> GetAllProvinceTaxRates()
        {
            try
            {
                return await Client.GetAsync<Tuple<IList<ProvinceTaxRateDTO>, IList<Alert>>>(
                            $"{_fullUri}/AllProvinceTaxRates");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get all Province Tax Rates", ex);
                throw;
            }
        }

        /// <summary>
        /// Get Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        public async Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetDocumentTypes()
        {
            try
            {
                return await Client.GetAsync<Tuple<IList<DocumentTypeDTO>, IList<Alert>>>(
                            $"{_fullUri}/DocumentTypes");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Province Tax Rate", ex);
                throw;
            }
        }

        public async Task<ApplicationUserDTO> GetDealerInfo()
        {
            try
            {
                return await Client.GetAsync<ApplicationUserDTO>(
                            $"{_fullUri}/GetDealerInfo").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get sub dealers list", ex);
                throw;
            }
        }

        public async Task<string> GetDealerCulture()
        {
            try
            {
                return await Client.GetAsync<string>(
                            $"{_fullUri}/GetDealerCulture");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get dealers culture", ex);
                throw;
            }
        }

        public async Task ChangeDealerCulture(string culture)
        {
            try
            {
                await Client.PutAsync(
                            $"{_fullUri}/PutDealerCulture?culture={culture}", "");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't change dealers culture", ex);
                throw;
            }
        }

        public Task<bool> CheckDealerSkinExistence()
        {
            try
            {
                return Client.GetAsync<bool>(
                            $"{_fullUri}/CheckDealerSkinExist");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't Check Dealer Skin Existence", ex);
                throw;
            }
        }

        public async Task<IList<StringSettingDTO>> GetDealerSettings(string hashDealerName)
        {
            try
            {
                var url = $"{_fullUri}/GetDealerSettings";
                if (hashDealerName != null)
                {
                    url += $"?hashDealerName={hashDealerName}";
                }
                return await Client.GetAsync<IList<StringSettingDTO>>(url).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Dealer Settings", ex);
                throw;
            }
        }

        public async Task<BinarySettingDTO> GetDealerBinSetting(SettingType type)
        {
            try
            {
                var url = $"{_fullUri}/GetDealerBinSetting?settingType={(int)type}";
                var hashDealerName = HttpContext.Current?.Request?.RequestContext?.RouteData?.Values["hashDealerName"]?.ToString() ??
                                     HttpRequestHelper.GetUrlReferrerRouteDataValues()?["hashDealerName"] as string;
                if (!string.IsNullOrEmpty(hashDealerName))
                {
                    url += $"&hashDealerName={hashDealerName}";
                }
                return await Client.GetAsync<BinarySettingDTO>(url).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Dealer Binary Setting", ex);
                throw;
            }
        }

        public async Task<CustomerLinkDTO> GetShareableLinkSettings()
        {
            try
            {
                return await Client.GetAsync<CustomerLinkDTO>(
                            $"{_fullUri}/GetCustomerLinkSettings").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Customer Link Settings", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateShareableLinkSettings(CustomerLinkDTO customerLink)
        {
            try
            {
                return await Client.PutAsync<CustomerLinkDTO, IList<Alert>>(
                            $"{_fullUri}/UpdateCustomerLinkSettings", customerLink);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't change Customer Link Settings", ex);
                throw;
            }
        }

        public async Task<CustomerLinkLanguageOptionsDTO> GetCustomerLinkLanguageOptions(string hashDealerName, string culture)
        {
            try
            {
                return await Client.GetAsync<CustomerLinkLanguageOptionsDTO>(
                            $"{_fullUri}/GetCustomerLinkLanguageOptions?hashDealerName={hashDealerName}&lang={culture}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Customer Link Language Options", ex);
                throw;
            }
        }
    }
}
