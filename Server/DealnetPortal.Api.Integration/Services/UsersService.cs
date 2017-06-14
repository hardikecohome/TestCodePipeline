using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
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
        private readonly IRateCardsRepository _rateCardsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UsersService(IAspireStorageReader aspireStorageReader, IDatabaseFactory databaseFactory, ILoggingService loggingService, ISettingsRepository settingsRepository,
            IRateCardsRepository rateCardsRepository, IUnitOfWork unitOfWork)
        {
            _aspireStorageReader = aspireStorageReader;
            _loggingService = loggingService;
            _settingsRepository = settingsRepository;
            _rateCardsRepository = rateCardsRepository;
            _unitOfWork = unitOfWork;
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
                    //AutoMapper.Mapper.Map<DealerDTO>(_aspireStorageReader.GetDealerInfo(user.UserName));

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
                    if (!string.IsNullOrEmpty(aspireDealerInfo.Ratecard) && user.Tier?.Name != aspireDealerInfo.Ratecard)
                    {
                        var tierAlerts = UpdateUserTier(user.Id, aspireDealerInfo);
                        if (tierAlerts.Any())
                        {
                            alerts.AddRange(tierAlerts);
                        }
                    }
                    var dealerEmail = aspireDealerInfo.Emails?.FirstOrDefault()?.EmailAddress;
                    if (!string.IsNullOrEmpty(dealerEmail) && dealerEmail != user.Email)
                    {
                        await _userManager.SetEmailAsync(user.Id, dealerEmail);
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
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

        private async Task<IList<Alert>> UpdateUserParent(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            var parentUser = !string.IsNullOrEmpty(aspireUser?.ParentDealerUserName)
                    ? await _userManager.FindByNameAsync(aspireUser.ParentDealerUserName)
                    : null;

            if (parentUser != null)
            {
                var updateUser = await _userManager.FindByIdAsync(userId);
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
            return alerts;
        }

        private async Task<IList<Alert>> UpdateUserRoles(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();
            if (!string.IsNullOrEmpty(aspireUser.Role))
            {
                var mbRole = ConfigurationManager.AppSettings["AspireMortgageBrokerRole"] ?? "Broker";                
                var rolesToSet = new List<string>();
                if (aspireUser.Role == mbRole)
                {
                    rolesToSet.Add(UserRole.MortgageBroker.ToString());                    
                }
                else
                {
                    rolesToSet.Add(UserRole.Dealer.ToString());
                }
                var dbRoles = await _userManager.GetRolesAsync(userId);
                var removeRes = await _userManager.RemoveFromRolesAsync(userId, dbRoles.Except(rolesToSet).ToArray());
                var addRes = await _userManager.AddToRolesAsync(userId, rolesToSet.Except(dbRoles).ToArray());
                if(addRes.Succeeded && removeRes.Succeeded)
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

        private IList<Alert> UpdateUserTier(string userId, DealerDTO aspireUser)
        {
            var alerts = new List<Alert>();

            try
            {
                var tier = _rateCardsRepository.GetTierByName(aspireUser.Ratecard);
                if (tier != null)
                {
                    var user = _userManager.Users.Include(u => u.Tier).FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        user.Tier = tier;
                        _unitOfWork.Save();
                        _loggingService.LogInfo($"Tier [{aspireUser.Ratecard}] was set to an user [{userId}]");
                    }
                }
            }
            catch (Exception ex)
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
    }
}
