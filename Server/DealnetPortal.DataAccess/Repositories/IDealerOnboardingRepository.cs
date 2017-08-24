using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain.Dealer;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IDealerOnboardingRepository
    {
        DealerInfo GetDealerInfoById(int id);
        DealerInfo GetDealerInfoByAccessCode(string accessCode);

        bool AddOrUpdateDealerInfo(DealerInfo dealerInfo, string accessCode = null);
        bool DeleteDealerInfo(int dealerInfoId);
        bool DeleteDealerInfo(string accessCode);
    }
}
