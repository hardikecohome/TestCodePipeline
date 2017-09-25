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
                return await Client.GetAsyncEx<DealerProfileDTO>($"{_fullUri}/GetDealerProfile", AuthenticationHeader, CurrentCulture);
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
                        Client.PostAsyncEx<DealerProfileDTO, IList<Alert>>(
                            $"{_fullUri}/UpdateDealerProfile", dealerProfile, AuthenticationHeader, CurrentCulture);
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
                return await Client.GetAsyncEx<DealerInfoDTO>($"{_fullUri}/GetDealerOnboardingInfo?accessKey={accessKey}", null, CurrentCulture);
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
                return await Client.GetAsyncEx<DealerInfoDTO>($"{_fullUri}/GetDealerOnboardingInfo?dealerInfoId={id}", null, CurrentCulture);
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
                        Client.PostAsyncEx<DealerInfoDTO, Tuple<DealerInfoKeyDTO, IList<Alert>>>(
                            $"{_fullUri}/UpdateDealerOnboardingInfo", dealerInfo, null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update dealer onboarding info for an user", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> SubmitDealerOnboardingForm(DealerInfoDTO dealerInfo)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<DealerInfoDTO, IList<Alert>>(
                            $"{_fullUri}/SubmitDealerOnboardingInfo", dealerInfo, null, CurrentCulture);
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
                        Client.PutAsyncEx<RequiredDocumentDTO, Tuple<DealerInfoKeyDTO, IList<Alert>>>(
                            $"{_fullUri}/AddDocumentToDealerOnboarding", document, null, CurrentCulture);
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
                        Client.PutAsyncEx<RequiredDocumentDTO, IList<Alert>>(
                            $"{_fullUri}/DeleteDocumentFromOnboardingForm", document, null, CurrentCulture);
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
                            $"{_fullUri}/SendDealerOnboardingDraftLink", link, null, CurrentCulture);
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
                            $"{_fullUri}/CheckOnboardingLink?dealerLink={dealerLink}");                
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't check onboarding link", ex);
                throw;
            }            
        }
        public async Task<string> UpdateDealerParent()
        {
            try
            {
                return await Client.GetAsyncEx<string>($"{_fullUri}/GetDealerParent", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get parent for an user", ex);
                return null;               
            }
        }

    }
}
