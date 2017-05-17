using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IDealerRepository
    {
        string GetParentDealerId(string dealerId);

        string GetUserIdByName(string userName);

        IList<string> GetUserRoles(string dealerId);

        string GetDealerNameByCustomerLinkId(int customerLinkId);
    }
}
