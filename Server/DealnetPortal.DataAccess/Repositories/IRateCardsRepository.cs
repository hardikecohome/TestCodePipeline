using System;
using System.Collections.Generic;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IRateCardsRepository
    {
        Tier GetTierByDealerId(string dealerId, DateTime? validDate);
        Tier GetTierByName(string tierName);
    }
}