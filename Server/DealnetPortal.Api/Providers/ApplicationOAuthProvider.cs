﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly IAspireService _aspireService;
        private readonly ILoggingService _loggingService;
        private readonly string _publicClientId;
        private AuthType _authType;

        public ApplicationOAuthProvider(string publicClientId)
        {
            _aspireService = (IAspireService) GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAspireService));
            _loggingService = (ILoggingService) GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));

            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;

            if (!Enum.TryParse(ConfigurationManager.AppSettings.Get("AuthProvider"), out _authType))
            {
                _authType = AuthType.AuthProvider;
            }
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            
            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null || !string.IsNullOrEmpty(user.AspireLogin))
            {
                var aspireRes = await CheckAndAddOrUpdateAspireUser(context);
                if (aspireRes?.Item2?.Any(e => e.Type == AlertType.Error) ?? false)
                {
                    user = null;
                    if (aspireRes?.Item2?.Any(e => e.Code == ErrorCodes.AspireConnectionFailed) ?? false)
                    {
                        context.SetError(ErrorConstants.ServiceFailed, "External service is unavailable");
                        return;
                    }                    
                }
                else
                {
                    user = aspireRes?.Item1;
                }
            }            

            if (user == null)
            {
                context.SetError(ErrorConstants.InvalidGrant, "The user name or password is incorrect.");
                return;
            }

            var applicationId = context.OwinContext.Get<string>("portalId");

            if (user.ApplicationId != applicationId)
            {
                context.SetError(ErrorConstants.UnknownApplication, "Unknown application to log in.");
                return;
            }

            if (_authType != AuthType.AuthProviderOneStepRegister && !user.EmailConfirmed)
            {
                context.SetError(ErrorConstants.ResetPasswordRequired, "Your on-time password is correct, now please change the password");
                return;
            }

            //TODO: special clames and other headers info can be added here
            //context.OwinContext.Response.Headers.Append("user", "userHeader");

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);                        

            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = CreateProperties(user.UserName);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var portalId = context.Parameters.Where(f => f.Key == "portalId").Select(f => f.Value).FirstOrDefault()?.FirstOrDefault();
            if (!string.IsNullOrEmpty(portalId))
            {
                context.OwinContext.Set<string>("portalId", portalId);
            }
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }

        private async Task<Tuple<ApplicationUser, IList<Alert>>> CheckAndAddOrUpdateAspireUser(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ApplicationUser user = null;
            List<Alert> outAlerts = new List<Alert>();

            if (_aspireService != null)
            {
                _loggingService?.LogInfo($"Check user [{context.UserName}] in Aspire");
                var alerts = await _aspireService.LoginUser(context.UserName, context.Password);
                if (alerts?.Any() ?? false)
                {
                    outAlerts.AddRange(alerts);
                }

                if (alerts?.All(a => a.Type != AlertType.Error) ?? false)
                {
                    var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

                    var applicationId = context.OwinContext.Get<string>("portalId");

                    var oldUser = await userManager.FindByNameAsync(context.UserName);

                    if (oldUser != null)
                    {
                        //update password for existing aspire user
                        var updateRes = await userManager.ChangePasswordAsync(oldUser.Id, oldUser.AspirePassword,
                            context.Password);
                        if (updateRes.Succeeded)
                        {
                            oldUser.AspirePassword = context.Password;
                            updateRes = await userManager.UpdateAsync(oldUser);
                            if (updateRes.Succeeded)
                            {
                                _loggingService?.LogInfo(
                                    $"Password for Aspire user [{context.UserName}] was updated successefully");
                                user = await userManager.FindAsync(context.UserName, context.Password);
                            }
                        }
                    }
                    else
                    {

                        var newUser = new ApplicationUser()
                        {
                            UserName = context.UserName,
                            Email = "",
                            ApplicationId = applicationId,
                            EmailConfirmed = true,
                            TwoFactorEnabled = false,
                            AspireLogin = context.UserName,
                            AspirePassword = context.Password
                        };

                        try
                        {
                            IdentityResult result = await userManager.CreateAsync(newUser, context.Password);
                            if (result.Succeeded)
                            {
                                _loggingService?.LogInfo(
                                    $"New entity for Aspire user [{context.UserName}] created successefully");
                                user = await userManager.FindAsync(context.UserName, context.Password);
                            }
                        }
                        catch (Exception ex)
                        {
                            user = null;
                        }
                    }
                }
                else
                {
                    alerts?.Where(a => a.Type == AlertType.Error).ForEach(a => 
                        _loggingService?.LogInfo(a.Message));
                }
            }
            return new Tuple<ApplicationUser, IList<Alert>>(user, outAlerts);
        }
    }
}