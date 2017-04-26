using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class UsersService : IUsersService
    {
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly List<string> _mortgageBrokers = new List<string>() {"user@user.com", "enertech"};

        public UsersService(IAspireStorageReader aspireStorageReader)
        {
            _aspireStorageReader = aspireStorageReader;
        }

        public IEnumerable<Claim> GetUserClaims(ApplicationUser user)
        {
            var claims = new List<Claim>();
            var roles = GetUserRoles(user);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            roles?.ForEach(r => claims.Add(new Claim(ClaimTypes.Role, r.ToString())));
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            //    switch (role)
            //    {
            //        case UserRole.Admin:
            //            break;
            //        case UserRole.Dealer:
            //        case UserRole.SubDealer:
            //            claims.Add(new Claim(ClaimNames.AllowCreateApplication, true.ToString()));
            //            claims.Add(new Claim(ClaimNames.ShowMyDeals, true.ToString()));
            //            break;
            //        case UserRole.MortgageBroker:
            //            claims.Add(new Claim(ClaimNames.AllowCreateCustomer, true.ToString()));
            //            claims.Add(new Claim(ClaimNames.ShowMyCustomers, true.ToString()));
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}

            if (!(user.Settings?.SettingValues?.Any() ?? false))
            {
                claims.Add(new Claim(ClaimNames.ShowAbout, true.ToString()));
                claims.Add(new Claim(ClaimNames.HasSkin, true.ToString()));
            }

            return claims;
        }

        public IEnumerable<UserRole> GetUserRoles(ApplicationUser user)
        {
            List<UserRole> roles = new List<UserRole>();
            if (_mortgageBrokers.Contains(user.UserName))
            {
                roles.Add(UserRole.MortgageBroker);
            }
            else
            {
                roles.Add(user.ParentDealer != null ? UserRole.SubDealer : UserRole.Dealer);
            }
            return roles;
        }
    }
}
