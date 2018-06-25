using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Helpers;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.ServiceAgent
{
    public class DictionaryServiceAgent : ApiBase, IDictionaryServiceAgent
    {
        private const string ContractApi = "dict";
        private ILoggingService _loggingService;

        public DictionaryServiceAgent(IHttpApiClient client, ILoggingService loggingService, IAuthenticationManager authenticationManager)
            : base(client, ContractApi, authenticationManager)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetEquipmentTypes()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>>(
                            $"{_fullUri}/Dealer/EquipmentTypes", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Equipment Types", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetAllEquipmentTypes()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>>(
                            $"{_fullUri}/EquipmentTypes", null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Equipment Types", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<LicenseDocumentDTO>, IList<Alert>>> GetAllLicenseDocuments()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<LicenseDocumentDTO>, IList<Alert>>>(
                            $"{_fullUri}/LicenseDocuments", null, CurrentCulture);
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
                            $"{_fullUri}/ProvinceTaxRates/{province.Trim()}");
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
                            $"{_fullUri}/ProvinceTaxRates");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get all Province Tax Rates", ex);
                throw;
            }
        }

        public async Task<Tuple<VarificationIdsDTO, IList<Alert>>> GetVerificationId(int id)
        {
            try
            {
                return await Client.GetAsync<Tuple<VarificationIdsDTO, IList<Alert>>>(
                            $"{_fullUri}/VerificationIds/{id}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Verification Id", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<VarificationIdsDTO>, IList<Alert>>> GetAllVerificationIds()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<VarificationIdsDTO>, IList<Alert>>>(
                            $"{_fullUri}/VerificationIds", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get all Verification Ids", ex);
                throw;
            }
        }


        /// <summary>
        /// Get all document types (funding checklist)
        /// </summary>
        /// <returns>List of Document Type</returns>
        public async Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetDocumentTypes()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<DocumentTypeDTO>, IList<Alert>>>(
                            $"{_fullUri}/DocumentTypes", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Province Tax Rate", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetStateDocumentTypes(string state)
        {
            try
            {
                var endPoint = AuthenticationHeader != null
                    ? $"{_fullUri}/dealer/DocumentTypes/{state}"
                    : $"{_fullUri}/DocumentTypes/{state}";
                return await Client.GetAsyncEx<Tuple<IList<DocumentTypeDTO>, IList<Alert>>>(
                            endPoint, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get document types list for province", ex);
                throw;
            }
        }

        public async Task<Tuple<IDictionary<string, IList<DocumentTypeDTO>>, IList<Alert>>> GetAllStateDocumentTypes()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IDictionary<string, IList<DocumentTypeDTO>>, IList<Alert>>>(
                    $"{_fullUri}/dealer/DocumentTypes", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get document types list for dealer", ex);
                throw;
            }
        }

        public async Task<ApplicationUserDTO> GetDealerInfo()
        {
            try
            {
                return await Client.GetAsyncEx<ApplicationUserDTO>(
                            $"{_fullUri}/Dealer/Info", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get sub dealers list", ex);
                throw;
            }
        }

        public async Task<string> GetDealerCulture(string dealerName = null)
        {
            try
            {
                var url = $"{_fullUri}/Dealer";
                if (dealerName != null)
                {
                    url += $"/{dealerName}/Culture";
                }
                else
                {
                    url += "/Culture";
                }
                return await Client.GetAsyncEx<string>(url, AuthenticationHeader, CurrentCulture);
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
                await Client.PutAsyncEx(
                            $"{_fullUri}/Dealer/Culture/{culture}", "", AuthenticationHeader, CurrentCulture);
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
                return Client.GetAsyncEx<bool>(
                            $"{_fullUri}/Dealer/Skin/check", AuthenticationHeader, CurrentCulture);
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
                var url = $"{_fullUri}/Dealer";
                if (hashDealerName != null)
                {
                    url += $"/{hashDealerName}";
                }
                url += "/Settings";
                return await Client.GetAsyncEx<IList<StringSettingDTO>>(url, AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
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
                var hashDealerName = HttpContext.Current?.Request?.RequestContext?.RouteData?.Values["hashDealerName"]?.ToString() ??
                                     HttpRequestHelper.GetUrlReferrerRouteDataValues()?["hashDealerName"] as string;
                var url = $"{_fullUri}/Dealer";
                if (!string.IsNullOrEmpty(hashDealerName))
                {
                    url += $"/{hashDealerName}";
                }
                url += $"/BinSettings/{(int)type}";
                return await Client.GetAsyncEx<BinarySettingDTO>(url, AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Dealer Binary Setting", ex);
                throw;
            }
        }
        
        public async Task<Tuple<IList<RateReductionCardDTO>, IList<Alert>>> GetAllRateReductionCards()
        {
            try
            {
                return await Client.GetAsyncEx<Tuple<IList<RateReductionCardDTO>, IList<Alert>>>(
                            $"{_fullUri}/RateReductionCards", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Rate Reduction Cards", ex);
                throw;
            }
        }

        public async Task<TierDTO> GetDealerTier()
        {
            try
            {
                return await Client.GetAsyncEx<TierDTO>($"{_fullUri}/Dealer/Tier", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get tier", ex);
                throw;
            }
        }

        public async Task<TierDTO> GetDealerTier(int contractId)
        {
            try
            {
                return await Client.GetAsyncEx<TierDTO>($"{_fullUri}/Dealer/Tier/contract/{contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get dealer tier", ex);
                throw;
            }
        }
    }
}
