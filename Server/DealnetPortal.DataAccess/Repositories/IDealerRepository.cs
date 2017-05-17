using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IDealerRepository
    {
        string GetParentDealerId(string dealerId);

        string GetUserIdByName(string userName);

        string GetDealerNameByCustomerLinkId(int customerLinkId);

        DealerProfile GetDealerProfile(string dealerId);

        bool UpdateDealerProfile(DealerProfile profile);
    }
}
