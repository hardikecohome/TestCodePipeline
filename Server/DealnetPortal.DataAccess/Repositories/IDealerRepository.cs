using System.Collections.Generic;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IDealerRepository
    {
        string GetParentDealerId(string dealerId);

        string GetUserIdByName(string userName);

        string GetUserIdByOnboardingLink(string link);

        IList<string> GetUserRoles(string dealerId);

        string GetDealerNameByCustomerLinkId(int customerLinkId);

        DealerProfile GetDealerProfile(string dealerId);

        DealerProfile UpdateDealerProfile(DealerProfile profile);

        void UpdateDealer(ApplicationUser dealer);
    }
}
