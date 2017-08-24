using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain.Dealer;

namespace DealnetPortal.DataAccess.Repositories
{
    public class DealerOnboardingRepository : BaseRepository, IDealerOnboardingRepository
    {
        public DealerOnboardingRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public DealerInfo GetDealerInfoById(int id)
        {
            return _dbContext.DealerInfos.Find(id);
        }

        public DealerInfo GetDealerInfoByAccessCode(string accessCode)
        {
            return _dbContext.DealerInfos.FirstOrDefault(di => di.AccessCode == accessCode);
        }

        public bool AddOrUpdateDealerInfo(DealerInfo dealerInfo, string accessCode = null)
        {
            bool isUpdated = false;
            DealerInfo dbDealer = null;

            if (!string.IsNullOrEmpty(accessCode))
            {
                dbDealer = GetDealerInfoByAccessCode(accessCode);
            }
            if (dbDealer == null && dealerInfo.Id != 0)
            {
                dbDealer = GetDealerInfoById(dealerInfo.Id);
            }
            if (dbDealer == null)
            {
                //add new
                dbDealer = _dbContext.DealerInfos.Add(dealerInfo);
            }
            else
            {
                //update
                UpdateDealerInfo(dbDealer, dealerInfo);
            }

            return dbDealer != null;
        }

        public bool DeleteDealerInfo(int dealerInfoId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteDealerInfo(string accessCode)
        {
            throw new NotImplementedException();
        }

        private void UpdateDealerInfo(DealerInfo dbDealerInfo, DealerInfo updateDealerInfo)
        {
            
        }
    }
}
