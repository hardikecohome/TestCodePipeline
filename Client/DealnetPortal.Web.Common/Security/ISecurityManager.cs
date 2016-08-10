using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Security
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
        /// <returns></returns>
        Task<bool> Login(string userName, string password);
        IPrincipal GetUser();
        /// <summary>
        /// Set user's Principal
        /// </summary>
        /// <param name="user"></param>
        void SetUser(IPrincipal user);

        void Logout();
    }
}
