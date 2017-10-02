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
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> UpdateDealerOnboardingForm(DealerInfoDTO dealerInfo);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> SubmitDealerOnboardingForm(DealerInfoDTO dealerInfo);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> AddDocumentToOnboardingForm(RequiredDocumentDTO document);
        IList<Alert> DeleteDocumentFromOnboardingForm(RequiredDocumentDTO document);
        IList<Alert> DeleteDealerOnboardingForm(int dealerInfoId);
        Task<IList<Alert>> SendDealerOnboardingDraftLink(DraftLinkDTO link);
        bool CheckOnboardingLink(string dealerLink);
        string GetDealerParentName(string dealerId);
    }
}
