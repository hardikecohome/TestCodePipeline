﻿using System;

namespace DealnetPortal.Domain.Repositories
{
    public interface IRateCardsRepository
    {
        Tier GetTierByDealerId(string dealerId, DateTime? validDate);
        Tier GetTierByName(string tierName);
    }
}