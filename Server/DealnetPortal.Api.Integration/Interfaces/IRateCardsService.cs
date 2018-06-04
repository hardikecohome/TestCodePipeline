using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Interfaces
{
    public interface IRateCardsService
    {
        TierDTO GetRateCardsByDealerId(string dealerId);
        TierDTO GetRateCardsForContract(int contractId, string dealerId);
        decimal? GetCreditAmount(int creditScore);
        IList<RateReductionCardDTO> GetRateReductionCards();
    }
}