using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Utilities;

namespace DealnetPortal.Web.ServiceAgent
{
    public class DictionaryServiceAgent : ApiBase, IDictionaryServiceAgent
    {
        private const string ContractApi = "dict";
        private ILoggingService _loggingService;

        public DictionaryServiceAgent(IHttpApiClient client, ILoggingService loggingService)
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

        public async Task<IList<StringSettingDTO>> GetDealerSettings()
        {
            try
            {
                return await Client.GetAsync<IList<StringSettingDTO>>(
                            $"{_fullUri}/GetDealerSettings").ConfigureAwait(false);
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
                return await Client.GetAsync<BinarySettingDTO>(
                            $"{_fullUri}/GetDealerBinSetting?settingType={(int)type}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Dealer Binary Setting", ex);
                throw;
            }
        }
    }
}
