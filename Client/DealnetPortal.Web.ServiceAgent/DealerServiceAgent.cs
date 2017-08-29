using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnBoarding;
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

        public async Task<DealerOnboardingDTO> GetDealerOnBoardingForm()
        {
            try
            {
                //return  await
                    //    Client.GetAsyncEx<DealerOnboardingDTO>($"{_fullUri}/GetDealerOnboardingForm", AuthenticationHeader,
                    //        CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get dealer's onboarding form", ex);

            }
            return null;
        }
    }
}
