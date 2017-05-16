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
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;
    using Api.Models.Profile;

    public class DealerServiceAgent : TransientApiBase, IDealerServiceAgent
    {
        private const string DealerApi = "Dealer";
        private readonly ILoggingService _loggingService;

        public DealerServiceAgent(ITransientHttpApiClient client, ILoggingService loggingService)
            : base(client, DealerApi)
        {
            _loggingService = loggingService;
        }

        public async Task<DealerProfileDTO> GetDealerProfile()
        {
            try
            {
                return await Client.GetAsync<DealerProfileDTO>($"{_fullUri}/GetDealerProfile");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get profile for an user", ex);
                return new DealerProfileDTO();
                //throw;
            }
        }
    }
}
