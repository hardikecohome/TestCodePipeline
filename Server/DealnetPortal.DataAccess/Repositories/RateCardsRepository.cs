using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            var dealer = base.GetUserById(id);
            return dealer.Tier;
        }
    }
}
