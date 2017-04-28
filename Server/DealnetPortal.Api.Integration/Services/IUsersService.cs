using System.Collections.Generic;
using System.Security.Claims;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IUsersService
    {
        IList<Claim> GetUserClaims(ApplicationUser user);
        IList<UserRole> GetUserRoles(ApplicationUser user);
    }
}