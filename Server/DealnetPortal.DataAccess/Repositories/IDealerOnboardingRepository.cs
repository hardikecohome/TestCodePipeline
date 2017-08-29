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
        DealerInfo GetDealerInfoByAccessKey(string accessKey);

        DealerInfo AddOrUpdateDealerInfo(DealerInfo dealerInfo);
        bool DeleteDealerInfo(int dealerInfoId);
        bool DeleteDealerInfo(string accessCode);

        RequiredDocument AddDocumentToDealer(int dealerInfoId, RequiredDocument document);
        RequiredDocument AddDocumentToDealer(string accessCode, RequiredDocument document);
    }
}
