using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IRateCardsService
    {
        TierDTO GetRateCardsByDealerId(string id);
        TierDTO GetFiltredRateCards(string id, double creditAmount);
    }
}