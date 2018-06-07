using System.Security.Claims;

namespace DealnetPortal.Web.Common.Security
{
    public class UserPrincipal : ClaimsPrincipal
    {
        public UserPrincipal(UserIdentity identity) : base(identity)
        {
        }
    }
}
