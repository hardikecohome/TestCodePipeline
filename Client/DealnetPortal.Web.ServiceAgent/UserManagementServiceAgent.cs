using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Common.Api;

namespace DealnetPortal.Web.ServiceAgent
{
    public class UserManagementServiceAgent : ApiBase, IUserManagementServiceAgent
    {
        private const string AccountApi = "Account";

        public UserManagementServiceAgent(IHttpApiClient client)
            : base(client, AccountApi)
        {
        }
    }
}
