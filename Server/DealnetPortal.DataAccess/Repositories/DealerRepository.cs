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

        public string GetParentId(string dealerId)
        {
            return base.GetUserById(dealerId).ParentDealerId;
        }
    }

    
}
