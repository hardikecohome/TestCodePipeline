using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Repositories;

namespace DealnetPortal.DataAccess.Repositories
{
    public class RateCardsRepository: BaseRepository, IRateCardsRepository
    {
        public RateCardsRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public Tier GetTierByDealerId(string dealerId, int? beacons, DateTime? validDate )
        {
            var dealer = _dbContext.Users
                .Include(x => x.Tier)
                .Include(x => x.Tier.RateCards)
                .SingleOrDefault(u => u.Id == dealerId);
            var date = validDate ?? DateTime.Now;
            dealer.Tier.RateCards = dealer.Tier.RateCards.Where(x =>
            (x.ValidFrom == null && x.ValidTo == null) ||
            (x.ValidFrom <= date && x.ValidTo > date) ||
            (x.ValidFrom <= date && x.ValidTo == null) ||
            (x.ValidFrom == null && x.ValidTo > date)).ToList();
            if (beacons.HasValue)
            {
                dealer.Tier.RateCards = dealer.Tier.RateCards.Where(x =>!x.CustomerRiskGroupId.HasValue ||
                (x.CustomerRiskGroup.BeaconScoreFrom <= beacons && beacons <= x.CustomerRiskGroup.BeaconScoreTo)).ToList();
            }

            return dealer.Tier;
        }

        public Tier GetTierByName(string tierName)
        {
            return _dbContext.Tiers.SingleOrDefault(t => t.Name == tierName);
        }

        public decimal? GetCreditAmount(int creditScore)
        {
            return _dbContext.CreditAmountSettings.FirstOrDefault(ca => creditScore >= ca.CreditScoreFrom && creditScore <= ca.CreditScoreTo)?.CreditAmount;
        }

        public CustomerRiskGroup GetCustomerRiskGroupByBeacon(int beaconScore)
        {
            return _dbContext.CustomerRiskGroups.FirstOrDefault(crg => beaconScore >= crg.BeaconScoreFrom && beaconScore <= crg.BeaconScoreTo);
        }
    }
}
