﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Api.Models.DealerOnboarding;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IDealerOnBoardingManager
    {
        Task<DealerOnboardingViewModel> GetNewDealerOnBoardingForm(string onboardingLink);
        Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsync(string accessKey);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> SaveDraft(DealerOnboardingViewModel model);
        Task<IList<Alert>> SubmitOnBoarding(DealerOnboardingViewModel model);
    }
}