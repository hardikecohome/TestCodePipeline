using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class UsersService : IUsersService
    {
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILoggingService _loggingService;
        private readonly List<string> _mortgageBrokers = new List<string>() {"user@user.com", "enertech"};

        public UsersService(IAspireStorageReader aspireStorageReader, IDatabaseFactory databaseFactory, ILoggingService loggingService)
        {
            _aspireStorageReader = aspireStorageReader;
            _loggingService = loggingService;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(databaseFactory.Get()));
        }        

        public IList<Claim> GetUserClaims(ApplicationUser user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            //var roles = GetUserRoles(user);            
            //roles?.ForEach(r => claims.Add(new Claim(ClaimTypes.Role, r.ToString())));

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

        public async Task<IList<Alert>> SyncAspireUser(ApplicationUser user)
        {
            var alerts = new List<Alert>();

            //get user info from aspire DB
            DealerDTO aspireDealerInfo = null;
            try
            {
                aspireDealerInfo =
                    AutoMapper.Mapper.Map<DealerDTO>(_aspireStorageReader.GetDealerRoleInfo(user.UserName));

                if (aspireDealerInfo != null)
                {
                    var parentAlerts = await UpdateUserParent(user, aspireDealerInfo);
                    if (parentAlerts.Any())
                    {
                        alerts.AddRange(parentAlerts);
                    }
                    var rolesAlerts = await UpdateUserRoles(user, aspireDealerInfo);
                    if (rolesAlerts.Any())
                    {
                        alerts.AddRange(rolesAlerts);
                    }
                }                
            }
            catch (Exception ex)
            {
                aspireDealerInfo = null;
                var errorMsg = $"Cannot connect to aspire database for get [{user.UserName}] info";
                _loggingService?.LogWarning(errorMsg);
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.AspireDatabaseConnectionFailed,
                    Header = errorMsg,
                    Type = AlertType.Warning,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        private async Task<IList<Alert>> UpdateUserParent(ApplicationUser user, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            var parentUser = !string.IsNullOrEmpty(aspireUser?.ParentDealerUserName)
                    ? await _userManager.FindByNameAsync(aspireUser.ParentDealerUserName)
                    : null;

            if (parentUser != null)
            {
                var updateUser = await _userManager.FindByNameAsync(user.UserName);
                updateUser.ParentDealer = parentUser;
                updateUser.ParentDealerId = parentUser.Id;
                var updateRes = await _userManager.UpdateAsync(updateUser);
                if (updateRes.Succeeded)
                {
                    _loggingService?.LogInfo(
                        $"Parent dealer for Aspire user [{user.UserName}] was updated successefully");
                }
                else
                {
                    updateRes.Errors?.ForEach(e =>
                    {
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Error,
                            Header = "Error during update Aspire user",
                            Message = e
                        });
                        _loggingService.LogError($"Error during update Aspire user: {e}");
                    });
                }
            }
            return alerts;
        }

        private async Task<IList<Alert>> UpdateUserRoles(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            if (string.IsNullOrEmpty(aspireUser.Role))
            {
                var mbRole = ConfigurationManager.AppSettings["AspireMortgageBrokerRole"] ?? "Broker";                
                var rolesToSet = new List<string>();
                if (aspireUser.Role == mbRole)
                {
                    rolesToSet.Add(UserRole.MortgageBroker.ToString());                    
                }
                var userRoles = await _userManager.GetRolesAsync(userId);

            }
            return alerts;
        }

        public IList<UserRole> GetUserRoles(ApplicationUser user)
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
