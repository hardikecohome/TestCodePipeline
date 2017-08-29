using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IDealerService
    {
        DealerProfileDTO GetDealerProfile(string dealerId);
        IList<Alert> UpdateDealerProfile(DealerProfileDTO dealerProfile);

        DealerInfoDTO GetDealerOnboardingForm(string accessKey);
        DealerInfoDTO GetDealerOnboardingForm(int id);
        Tuple<DealerInfoKeyDTO, IList<Alert>> UpdateDealerOnboardingForm(DealerInfoDTO dealerInfo);
        Tuple<DealerInfoKeyDTO, IList<Alert>> AddDocumentToOnboardingForm(RequiredDocumentDTO document);
    }
}
