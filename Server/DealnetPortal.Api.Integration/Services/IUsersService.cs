using System.Collections.Generic;
using System.Security.Claims;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IUsersService
    {
        IEnumerable<Claim> GetUserClaims(ApplicationUser user);
        IEnumerable<UserRole> GetUserRoles(ApplicationUser user);
    }
}