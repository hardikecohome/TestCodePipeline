using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
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
        bool DeleteDocumentFromDealer(int documentId);
        RequiredDocument SetDocumentStatus(int documentId, DocumentStatus newStatus);
        RequiredDocument AddDocumentToDealer(string accessCode, RequiredDocument document);
    }
}
