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
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
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
        private readonly ISettingsRepository _settingsRepository;

        #region constants
        private const string MB_ROLE_CONFIG_KEY = "AspireMortgageBrokerRole";
        private const string DEFAULT_MB_ROLE = "Mortgage Brokers";
        #endregion

        public UsersService(IAspireStorageReader aspireStorageReader, IDatabaseFactory databaseFactory, ILoggingService loggingService, ISettingsRepository settingsRepository)
        {
            _aspireStorageReader = aspireStorageReader;
            _loggingService = loggingService;
            _settingsRepository = settingsRepository;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(databaseFactory.Get()));
        }        

        public IList<Claim> GetUserClaims(string userId)
        {
            var settings = _settingsRepository.GetUserSettings(userId);
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

            if (settings?.SettingValues != null)
            {
                if (settings.SettingValues.Any())
                {
                    claims.Add(new Claim(ClaimNames.ShowAbout, false.ToString()));
                    claims.Add(new Claim(ClaimNames.HasSkin, true.ToString()));
                }
                else
                {
                    claims.Add(new Claim(ClaimNames.ShowAbout, true.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimNames.ShowAbout, true.ToString()));
                claims.Add(new Claim(ClaimNames.HasSkin, false.ToString()));
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
                    var parentAlerts = await UpdateUserParent(user.Id, aspireDealerInfo);
                    if (parentAlerts.Any())
                    {
                        alerts.AddRange(parentAlerts);
                    }
                    var rolesAlerts = await UpdateUserRoles(user.Id, aspireDealerInfo);
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

        #region private
        private async Task<IList<Alert>> UpdateUserParent(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            var parentUser = !string.IsNullOrEmpty(aspireUser?.ParentDealerUserName)
                    ? await _userManager.FindByNameAsync(aspireUser.ParentDealerUserName)
                    : null;

            if (parentUser != null)
            {
                var updateUser = await _userManager.FindByIdAsync(userId);
                if (updateUser.ParentDealerId != parentUser.Id)
                {
                    updateUser.ParentDealer = parentUser;
                    updateUser.ParentDealerId = parentUser.Id;
                    var updateRes = await _userManager.UpdateAsync(updateUser);
                    if (updateRes.Succeeded)
                    {
                        _loggingService?.LogInfo(
                            $"Parent dealer for Aspire user [{userId}] was updated successefully");
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
            }
            return alerts;
        }

        private async Task<IList<Alert>> UpdateUserRoles(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            if (!string.IsNullOrEmpty(aspireUser.Role))
            {
                var dbRoles = await _userManager.GetRolesAsync(userId);
                if (!dbRoles.Contains(aspireUser.Role))
                {
                    var mbRoles = ConfigurationManager.AppSettings[MB_ROLE_CONFIG_KEY] != null
                        ? ConfigurationManager.AppSettings[MB_ROLE_CONFIG_KEY].Split(',').Select(s => s.Trim()).ToArray()
                        : new string[] { DEFAULT_MB_ROLE };
                    var user = await _userManager.FindByIdAsync(userId);
                    var removeRes = await _userManager.RemoveFromRolesAsync(userId, dbRoles.ToArray());
                    IdentityResult addRes;
                    if (mbRoles.Contains(aspireUser.Role))
                    {
                        addRes = await _userManager.AddToRolesAsync(userId, new[] { UserRole.MortgageBroker.ToString() });
                    }
                    else
                    {
                        addRes = await _userManager.AddToRolesAsync(userId, new[] { UserRole.Dealer.ToString() });
                    }

                    var updateRes = await _userManager.UpdateAsync(user);
                    if (addRes.Succeeded && removeRes.Succeeded && updateRes.Succeeded)
                    {
                        _loggingService?.LogInfo(
                            $"Roles for Aspire user [{userId}] was updated successefully");
                    }
                    else
                    {
                        removeRes.Errors?.ForEach(e =>
                        {
                            alerts.Add(new Alert()
                            {
                                Type = AlertType.Error,
                                Header = "Error during remove role",
                                Message = e
                            });
                            _loggingService.LogError($"Error during remove role for an user {userId}: {e}");
                        });
                        addRes.Errors?.ForEach(e =>
                        {
                            alerts.Add(new Alert()
                            {
                                Type = AlertType.Error,
                                Header = "Error during add role",
                                Message = e
                            });
                            _loggingService.LogError($"Error during add role for an user {userId}: {e}");
                        });
                    }
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Error during update role",
                    Message = $"Error during getting user role from Aspire, for an user {userId}"
                });
                _loggingService.LogError($"Error during getting user role from Aspire, for an user {userId}");
            }
            return alerts;
        }
        #endregion
    }
}
