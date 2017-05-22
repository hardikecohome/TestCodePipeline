using System.Collections.Generic;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IDealerRepository
    {
        string GetParentDealerId(string dealerId);

        string GetUserIdByName(string userName);

        IList<string> GetUserRoles(string dealerId);

        string GetDealerNameByCustomerLinkId(int customerLinkId);

        DealerProfile GetDealerProfile(string dealerId);

        bool UpdateDealerProfile(DealerProfile profile);
    }
}
