﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Aspire.Integration.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class UsersService : IUsersService
    {
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IRateCardsRepository _rateCardsRepository;
        private readonly IAppConfiguration _сonfiguration;

        public UsersService(IAspireStorageReader aspireStorageReader, ILoggingService loggingService, IRateCardsRepository rateCardsRepository,
            ISettingsRepository settingsRepository,  IAppConfiguration appConfiguration)
        {
            _aspireStorageReader = aspireStorageReader;
            _loggingService = loggingService;
            _settingsRepository = settingsRepository;
            _rateCardsRepository = rateCardsRepository;
            _сonfiguration = appConfiguration;
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

        public async Task<IList<Alert>> SyncAspireUser(ApplicationUser user, UserManager<ApplicationUser> userManager = null)
        {
            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }

            var alerts = new List<Alert>();
            var userManagerForUpdate = userManager;
            //get user info from aspire DB
            DealerDTO aspireDealerInfo = null;
            try
            {
                aspireDealerInfo =
                    AutoMapper.Mapper.Map<DealerDTO>(_aspireStorageReader.GetDealerRoleInfo(user.UserName));

                if (aspireDealerInfo != null)
                {
                    var parentAlerts = await UpdateUserParent(user.Id, aspireDealerInfo, userManagerForUpdate);
                    if (parentAlerts.Any())
                    {
                        alerts.AddRange(parentAlerts);
                    }
                    var rolesAlerts = await UpdateUserRoles(user.Id, aspireDealerInfo, userManagerForUpdate);
                    if (rolesAlerts.Any())
                    {
                        alerts.AddRange(rolesAlerts);
                    }
                    if (user.Tier?.Name != aspireDealerInfo.Ratecard)
                    {
                        var tierAlerts = await UpdateUserTier(user.Id, aspireDealerInfo, userManagerForUpdate);
                        if (tierAlerts.Any())
                        {
                            alerts.AddRange(tierAlerts);
                        }
                    }
                    //currently email update isn't work correctly
                    //var dealerEmail = aspireDealerInfo.Emails?.FirstOrDefault()?.EmailAddress;
                    //if (!string.IsNullOrEmpty(dealerEmail) && dealerEmail != user.Email)
                    //{
                    //    await _userManager.SetEmailAsync(user.Id, dealerEmail);
                    //    user.EmailConfirmed = true;
                    //    await _userManager.UpdateAsync(user);
                    //}
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

        public string GetUserPassword(string userId)
        {
            string aspirePassword = null;
            using (var secContext = new SecureDbContext())
            {
                try
                {                
                    var user = secContext.Users.Find(userId);
                    aspirePassword = user?.Secure_AspirePassword;
                }
                catch (Exception e)
                {
                    _loggingService?.LogError($"Cannot recieve password for user {userId}", e);
                }
            }
            return aspirePassword;
        }

        public void UpdateUserPassword(string userId, string newPassword)
        {
            using (var secContext = new SecureDbContext())
            {
                try
                {
                    var user = secContext.Users.Find(userId);
                    if (user.Secure_AspirePassword != newPassword)
                    {
                        user.Secure_AspirePassword = newPassword;
                        secContext.SaveChanges();
                        _loggingService?.LogInfo(
                            $"Password for Aspire user [{userId}] was updated successefully");
                    }
                }
                catch (Exception e)
                {
                    _loggingService?.LogError($"Cannot update password for user {userId}", e);
                }
            }
        }

        #region private
        private async Task<IList<Alert>> UpdateUserParent(string userId, DealerDTO aspireUser, UserManager<ApplicationUser> userManager)
        {
            var alerts = new List<Alert>();
            var parentUser = !string.IsNullOrEmpty(aspireUser?.ParentDealerUserName)
                    ? await userManager.FindByNameAsync(aspireUser.ParentDealerUserName)
                    : null;

            if (parentUser != null)
            {
                var updateUser = await userManager.FindByIdAsync(userId);
                if (updateUser.ParentDealerId != parentUser.Id)
                {
                    updateUser.ParentDealer = parentUser;
                    updateUser.ParentDealerId = parentUser.Id;
                    var updateRes = await userManager.UpdateAsync(updateUser);
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

        private async Task<IList<Alert>> UpdateUserRoles(string userId, DealerDTO aspireUser, UserManager<ApplicationUser> userManager)
        {
            var alerts = new List<Alert>();
            if (!string.IsNullOrEmpty(aspireUser.Role))
            {
                var dbRoles = await userManager.GetRolesAsync(userId);
                var mbConfigRoles = _сonfiguration.GetSetting(WebConfigKeys.MB_ROLE_CONFIG_KEY).Split(',').Select(s => s.Trim()).ToArray();

                if (!dbRoles.Contains(aspireUser.Role) && !(mbConfigRoles.Contains(aspireUser.Role) && dbRoles.Contains(UserRole.MortgageBroker.ToString())))
                {                    
                    var user = await userManager.FindByIdAsync(userId);
                    var removeRes = await userManager.RemoveFromRolesAsync(userId, dbRoles.ToArray());
                    IdentityResult addRes;
                    if (mbConfigRoles.Contains(aspireUser.Role))
                    {
                        addRes = await userManager.AddToRolesAsync(userId, new[] {UserRole.MortgageBroker.ToString()});
                    }
                    else
                    {
                        addRes = await userManager.AddToRolesAsync(userId, new[] {UserRole.Dealer.ToString()});
                    }
                    
                    var updateRes = await userManager.UpdateAsync(user);
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

        private async Task<IList<Alert>> UpdateUserTier(string userId, DealerDTO aspireUser, UserManager<ApplicationUser> userManager)
        {
            var alerts = new List<Alert>();
            try
            {
                var tier = _rateCardsRepository.GetTierByName(aspireUser.Ratecard);
                var updateUser = await userManager.FindByIdAsync(userId);
                if (updateUser != null)
                {
                    updateUser.Tier = tier;
                    var updateRes = await userManager.UpdateAsync(updateUser);
                    if (updateRes.Succeeded)
                    {
                        {
                            _loggingService.LogInfo($"Tier [{aspireUser.Ratecard}] was set to an user [{updateUser.Id}]");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Error during update user tier",
                    Message = $"Error during update user tier for an user {userId}"
                });
                _loggingService.LogError($"Error during update user tier for an user {userId}", ex);
            }
            return alerts;
        }
        #endregion
    }
}
