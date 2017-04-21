using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IRateCardsRepository
    {
        Tier GetTierByDealerId(string id);
    }
}