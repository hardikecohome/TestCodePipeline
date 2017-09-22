using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;
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
