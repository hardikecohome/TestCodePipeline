﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Enumeration;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Security;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISecurityManager _securityManager;
        private readonly IUserManagementServiceAgent _userManagementServiceAgent;
        public AccountController(ISecurityManager securityManager, IUserManagementServiceAgent userManagementServiceAgent)
        {
            _securityManager = securityManager;
            _userManagementServiceAgent = userManagementServiceAgent;
        }
        
        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _securityManager.Login(model.Email, model.Password);
            if (result.Any(item => item.Type == AlertType.Error && item.Header == ErrorConstants.ResetPasswordRequired))
            {
                return RedirectToAction("ChangePasswordAfterRegistration");
            }
            if (result.Any(item => item.Type == AlertType.Error))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/LogOff
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _securityManager.Logout();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _userManagementServiceAgent.Register(model);
            if (result.Any(item => item.Type == AlertType.Error))
            {
                AddAlertsToModelErrors(result);
                return View(model);
            }
            return RedirectToAction("ConfirmSuccessfullRegistration");
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
        public async Task<ActionResult> ChangePasswordAfterRegistration(ChangePasswordAnonymouslyBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _userManagementServiceAgent.ChangePasswordAnonymously(model);
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