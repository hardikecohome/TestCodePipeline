using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.Dealer;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IDealerOnBoardingManager
    {
        Task<DealerOnboardingViewModel> GetNewDealerOnBoardingForm(string onboardingLink);
        Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsync(string accessKey);
        Task<SaveAndResumeViewModel> SaveDraft(DealerOnboardingViewModel model);
        Task<IList<Alert>> SendDealerOnboardingDraftLink(SaveAndResumeViewModel model);
        Task<SaveAndResumeViewModel> SubmitOnBoarding(DealerOnboardingViewModel model);
        Task<DocumentResponseViewModel> UploadOnboardingDocument(OnboardingDocumentForUpload fileModel);
        Task<DocumentResponseViewModel> DeleteOnboardingDocument(OnboardingDocumentForDelete fileModel);
    }
}