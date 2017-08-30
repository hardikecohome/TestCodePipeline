using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Models.Dealer;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IDealerOnBoardingManager
    {
        Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsynch(string accessKey);
    }
}