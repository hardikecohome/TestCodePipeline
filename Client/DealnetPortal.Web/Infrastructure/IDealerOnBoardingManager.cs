using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IDealerOnBoardingManager
    {
        Task<DealerOnboardingViewModel> GetNewDealerOnBoardingForm(string onboardingLink);
        Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsync(string accessKey);
        Task<SaveAndResumeViewModel> SaveDraft(DealerOnboardingViewModel model);
        Task<IList<Alert>> SendDealerOnboardingDraftLink(SaveAndResumeViewModel model);
        Task<IList<Alert>> SubmitOnBoarding(DealerOnboardingViewModel model);
        Task<DocumentResponseViewModel> UploadOnboardingDocument(OnboardingDocumentForUpload fileModel);
        Task<DocumentResponseViewModel> DeleteOnboardingDocument(OnboardingDocumentForDelete fileModel);
    }
}