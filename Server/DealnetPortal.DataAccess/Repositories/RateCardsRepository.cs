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

        public Tier GetFiltredRateCards(string id, double creditAmount)
        {
            var dealer = _dbContext.Users
                .Include(x => x.Tier)
                .Include(x => x.Tier.RateCards)
                .FirstOrDefault(x => x.Id == id);

            if (dealer == null)
            {
                return new Tier();
            }

            var tier = dealer.Tier;

            var result = _dbContext.RateCards
                .Where(x => x.TierId == tier.Id)
                .Where(x => x.LoanValueFrom <= creditAmount && x.LoanValueTo >= creditAmount)
                //.Where(x => x.ValidFrom >= EntityFunctions.TruncateTime(DateTime.UtcNow.Date) && x.ValidTo <= EntityFunctions.TruncateTime(DateTime.UtcNow.Date))
                .ToList();

            tier.RateCards = result;

            return tier;
        }
    }
}
