using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess.Repositories
{
    public class CustomerFormRepository : BaseRepository, ICustomerFormRepository
    {
        public CustomerFormRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }
    }
}
