using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    /// <summary>
    /// Security manager - encapsulates all security concerns for web-application
    /// <see cref="ISecurityManager"/> for details
    /// </summary>
    public interface ISecurityManager
    {
        /// <summary>
        /// User login 
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="portalId">portal identificator</param>
        /// <returns></returns>
        Task<IList<Alert>> Login(string userName, string password, string portalId);
        IPrincipal GetUser();
        /// <summary>
        /// Set user's Principal
        /// </summary>
        /// <param name="user"></param>
        void SetUser(IPrincipal user);
        void Logout();        
    }
}
