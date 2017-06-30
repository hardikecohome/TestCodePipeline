using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public class RateCardsRepository: BaseRepository, IRateCardsRepository
    {
        public RateCardsRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public Tier GetTierByDealerId(string id)
        {
            var dealer = _dbContext.Users
                .Include(x => x.Tier)
                .Include(x => x.Tier.RateCards)
                .SingleOrDefault(u => u.Id == id);

            return dealer.Tier;
        }

        public Tier GetTierByName(string tierName)
        {
            return _dbContext.Tiers.FirstOrDefault(t => t.Name == tierName);
        }
    }
}
