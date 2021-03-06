﻿using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Managers;
using DealnetPortal.Web.ServiceAgent;

using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

namespace DealnetPortal.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISecurityManager _securityManager;
        private readonly CultureSetterManager _cultureManager;
        private readonly IUserManagementServiceAgent _userManagementServiceAgent;
        private readonly ILoggingService _loggingService;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ISettingsManager _settingsManager;
        private AuthType _authType;

        public AccountController(ISecurityManager securityManager, CultureSetterManager cultureManager, IUserManagementServiceAgent userManagementServiceAgent,
            ILoggingService loggingService, IDictionaryServiceAgent dictionaryServiceAgent, ISettingsManager settingsManager)
        {
            _securityManager = securityManager;
            _cultureManager = cultureManager;
            _userManagementServiceAgent = userManagementServiceAgent;
            _loggingService = loggingService;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _settingsManager = settingsManager;

            if (!Enum.TryParse(ConfigurationManager.AppSettings.Get("AuthProvider"), out _authType))
            {
                _authType = AuthType.AuthProvider;
            }
        }
        
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null)
                returnUrl = Server.UrlEncode(Request.UrlReferrer.PathAndQuery);

            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(DealnetPortal.Web.Models.LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string decodedUrl = "";
            if (!string.IsNullOrEmpty(returnUrl))
                decodedUrl = Server.UrlDecode(returnUrl);

            _loggingService.LogInfo(string.Format("Attemtp to login user: {0}", model.UserName));
            var result = await _securityManager.Login(model.UserName, model.Password, ApplicationSettingsManager.PortalId, model.RememberMe);
            if (result.Any(item => item.Type == AlertType.Error && item.Header == ErrorConstants.ResetPasswordRequired))
            {
                _loggingService.LogInfo(string.Format("Attemtp to login user: {0}; needs change password", model.UserName));
                return RedirectToAction("ChangePasswordAfterRegistration");
            }
            if (result.Any(item => item.Type == AlertType.Error))
            {
                var error = result.FirstOrDefault(r => r.Type == AlertType.Error);
                _loggingService.LogError(string.Format("Invalid login attempt for user: {0} - {1}:{2}", model.UserName, error?.Header, error?.Message));
                ModelState.AddModelError("", Resources.Resources.InvalidLoginAttempt);
                return View(model);
            }
            try
            {
                var culture = await _dictionaryServiceAgent.GetDealerCulture(model.UserName);
                _loggingService.LogInfo($"Setting culture {culture} with decodedUrl: {decodedUrl} for user {model.UserName}");
                _cultureManager.SetCulture(culture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't set default culture for user: {model.UserName}", ex);
            }

            if (Url.IsLocalUrl(decodedUrl))
            {
                return Redirect(decodedUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/LogOff
        [HttpGet]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _settingsManager.ClearUserSettings(User?.Identity?.Name);            
            _loggingService.LogInfo(string.Format("User {0} logged out", User?.Identity?.Name));
            _securityManager.Logout();                        

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            ViewBag.RegisterWithPassword = _authType == AuthType.AuthProviderOneStepRegister;
            return View();
        }

        // GET: /Account/Register
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(DealnetPortal.Web.Models.ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userManagementServiceAgent.ForgotPassword(Mapper.Map<ForgotPasswordBindingModel>(model));
            if (result.Any(item => item.Type == AlertType.Error))
            {
                AddAlertsToModelErrors(result);
                ViewBag.RegisterWithPassword = _authType == AuthType.AuthProviderOneStepRegister;
                return View(model);
            }

            return RedirectToAction("Login", "Account");
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(DealnetPortal.Web.Models.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            model.ApplicationId = ApplicationSettingsManager.PortalId;
            model.UserName = model.Email;
            var result = await _userManagementServiceAgent.Register(Mapper.Map<RegisterBindingModel>(model));
            if (result.Any(item => item.Type == AlertType.Error))
            {
                AddAlertsToModelErrors(result);
                ViewBag.RegisterWithPassword = _authType == AuthType.AuthProviderOneStepRegister;
                return View(model);
            }
            if (_authType != AuthType.AuthProviderOneStepRegister)
            {
                return RedirectToAction("ConfirmSuccessfullRegistration");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/ConfirmSuccessfullRegistration
        public ActionResult ConfirmSuccessfullRegistration()
        {
            return View();
        }

        // GET: /Account/ChangePasswordAfterRegistration
        public ActionResult ChangePasswordAfterRegistration()
        {
            return View();
        }

        // POST: /Account/ChangePasswordAfterRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePasswordAfterRegistration(DealnetPortal.Web.Models.ChangePasswordAnonymouslyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _userManagementServiceAgent.ChangePasswordAnonymously(Mapper.Map<ChangePasswordAnonymouslyBindingModel>(model));
            if (result.Any(item => item.Type == AlertType.Error))
            {
                AddAlertsToModelErrors(result);
                return View(model);
            }
            return RedirectToAction("ConfirmSuccessfullPassChange");
        }

        // GET: /Account/ConfirmSuccessfullPassChange
        public ActionResult ConfirmSuccessfullPassChange()
        {
            return View();
        }

        private void AddAlertsToModelErrors(IEnumerable<Alert> alerts)
        {
            foreach (var alert in alerts.Where(a => a.Type == AlertType.Error))
            {
                ModelState.AddModelError("", alert.Message);
            }
        } 
    }
}