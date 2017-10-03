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
            dealer.Tier.RateCards = dealer.Tier.RateCards.Where(x => 
            (x.ValidFrom == null && x.ValidTo == null) ||
            (x.ValidFrom <= DateTime.Now && x.ValidTo > DateTime.Now)||
            (x.ValidFrom <= DateTime.Now && x.ValidTo == null) ||
            (x.ValidFrom == null && x.ValidTo > DateTime.Now)).ToList();
            return dealer.Tier;
        }

        public Tier GetTierByName(string tierName)
        {
            return _dbContext.Tiers.FirstOrDefault(t => t.Name == tierName);
        }
    }
}
