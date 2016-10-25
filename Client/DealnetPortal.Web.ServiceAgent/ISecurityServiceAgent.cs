using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Web.ServiceAgent
{
    /// <summary>
    /// Agent for access Security Service
    /// </summary>
    public interface ISecurityServiceAgent
    {
        /// <summary>
        /// Authenicates user 
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <returns>Tos security principal</returns>
        Task<Tuple<IPrincipal, IList<Alert>>> Authenicate(string userName, string password, string portalId);
        /// <summary>
        /// Sets default request header for bearer authorization
        /// </summary>
        /// <param name="principal"></param>
        void SetAuthorizationHeader(IPrincipal principal);

        bool IsAutorizated();
    }
}
