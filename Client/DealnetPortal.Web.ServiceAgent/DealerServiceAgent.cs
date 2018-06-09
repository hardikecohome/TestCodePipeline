using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Profile;
    using DealnetPortal.Api.Models.Notify;
    using Microsoft.Owin.Security;

    public class DealerServiceAgent : ApiBase, IDealerServiceAgent
    {
        private const string DealerApi = "Dealer";
        private readonly ILoggingService _loggingService;

        public DealerServiceAgent(IHttpApiClient client, ILoggingService loggingService, IAuthenticationManager authenticationManager)
            : base(client, DealerApi, authenticationManager)
        {
            _loggingService = loggingService;
        }

        public async Task<DealerProfileDTO> GetDealerProfile()
        {
            try
            {
                return await Client.GetAsyncEx<DealerProfileDTO>($"{_fullUri}/Profile", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get profile for an user", ex);
                return new DealerProfileDTO();
            }
        }

        public async Task<IList<Alert>> UpdateDealerProfile(DealerProfileDTO dealerProfile)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<DealerProfileDTO, IList<Alert>>(
                            $"{_fullUri}/Profile", dealerProfile, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer profile for an user", ex);
                throw;
            }
        }
        
        public async Task<DealerInfoDTO> GetDealerOnboardingForm(string accessKey)
        {
            try
            {
                return await Client.GetAsyncEx<DealerInfoDTO>($"{_fullUri}/Onboarding/key/{accessKey}", null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get dealer's onboarding form", ex);

            }
            return null;
        }
        public async Task<DealerInfoDTO> GetDealerOnboardingForm(int id)
        {
            try
            {
                return await Client.GetAsyncEx<DealerInfoDTO>($"{_fullUri}/Onboarding/{id}", null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get dealer's onboarding form", ex);
            }
            return null;
        }
        public async Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> UpdateDealerOnboardingForm(DealerInfoDTO dealerInfo)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<DealerInfoDTO, Tuple<DealerInfoKeyDTO, IList<Alert>>>(
                            $"{_fullUri}/Onboarding", dealerInfo, null, null);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer onboarding info for an user", ex);
                throw;
            }
        }

        public async Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> SubmitDealerOnboardingForm(DealerInfoDTO dealerInfo)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<DealerInfoDTO, Tuple<DealerInfoKeyDTO, IList<Alert>>>(
                            $"{_fullUri}/Onboarding/Submit", dealerInfo, null, null);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer onboarding info for an user", ex);
                throw;
            }
        }

        public async Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> AddDocumentToOnboardingForm(
            RequiredDocumentDTO document)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<RequiredDocumentDTO, Tuple<DealerInfoKeyDTO, IList<Alert>>>(
                            $"{_fullUri}/Onboarding/{document.DealerInfoId}/documents", document, null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't add document to dealer onboarding", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> DeleteDocumentFromOnboardingForm(RequiredDocumentDTO document)
        {
            try
            {
                return
                    await
                        Client.DeleteAsyncEx<IList<Alert>>(
                            $"{_fullUri}/Onboarding/{document.DealerInfoId}/documents/{document.Id}", null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't add document to dealer onboarding", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> SendDealerOnboardingDraftLink(DraftLinkDTO link)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<DraftLinkDTO, IList<Alert>>(
                            $"{_fullUri}/Onboarding/SendLink", link, null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer onboarding info for an user", ex);
                throw;
            }
        }

        public async Task<bool> CheckOnboardingLink(string dealerLink)
        {
            try
            {
                return
                    await Client.GetAsync<bool>(
                            $"{_fullUri}/Onboarding/CheckLink/{dealerLink}");                
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't check onboarding link", ex);
                throw;
            }            
        }

        public async Task<IList<Alert>> DealerSupportRequestEmail(SupportRequestDTO dealerSupportRequest)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<SupportRequestDTO, IList<Alert>>(
                            $"{_fullUri}/DealerSupportRequestEmail", dealerSupportRequest, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer profile for an user", ex);
                throw;
            }
        }


    }
}
