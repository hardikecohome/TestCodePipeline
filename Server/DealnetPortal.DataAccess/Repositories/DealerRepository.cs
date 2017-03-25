using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess.Repositories
{
    public class DealerRepository : BaseRepository, IDealerRepository
    {
        public DealerRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public string GetParentDealerId(string dealerId)
        {
            return base.GetUserById(dealerId).ParentDealerId;
        }

        public string GetUserIdByName(string userName)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserName == userName)?.Id;
        }
    }

    
}
