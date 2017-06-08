using System.Collections.Generic;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IRateCardsRepository
    {
        Tier GetTierByDealerId(string id);
        Tier GetTierByName(string tierName);
    }
}