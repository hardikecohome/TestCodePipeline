using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Interfaces
{
    public interface IRateCardsService
    {
        TierDTO GetRateCardsByDealerId(string dealerId);
        TierDTO GetRateCardsByDealerId(int contractId, string dealerId);
    }
}