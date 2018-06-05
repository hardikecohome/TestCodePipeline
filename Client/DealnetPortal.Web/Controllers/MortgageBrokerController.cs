﻿using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = RoleContstants.MortgageBroker)]
    public class MortgageBrokerController : Controller
    {
        private readonly ICustomerManager _customerManager;

        public MortgageBrokerController(ICustomerManager customerManager)
        {
            _customerManager = customerManager;
        }

        public async Task<ActionResult> NewClient()
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            return View(await _customerManager.GetTemplateAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewClient(NewCustomerViewModel newCustomer)
        {
            var result = await _customerManager.AddAsync(newCustomer);

            if (result?.Item2.Any(x => x.Type == AlertType.Error) ?? false)
            {
                ViewBag.Errors = result.Item2;

                return View("CustomerCreationDecline");
            }

            if (result?.Item1 == null || result.Item1.ContractState == ContractState.CreditCheckDeclined || result.Item1.OnCreditReview == true
                || result.Item1?.PrimaryCustomer?.CreditReport == null)
            {
                return View("CustomerCreationDecline");
            }
            
            ViewBag.CreditAmount = Convert.ToInt32(result?.Item1.Details.CreditAmount).ToString("N0", CultureInfo.InvariantCulture);
            ViewBag.FullName = $"{result?.Item1.PrimaryCustomer.FirstName} {result?.Item1.PrimaryCustomer.LastName}";

            return View("CustomerCreationSuccess");
        }

        public ActionResult CustomerCreationSuccess(int id)
        {
            return View();
        }

        public ActionResult MyClients()
        {
            return View();
        }

        
        [HttpGet]
        public async Task<ActionResult> GetCreatedContracts()
        {
            var result = await _customerManager.GetCreatedContractsAsync();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CheckCustomerExisting(string email)
        {
            var result = await _customerManager.CheckCustomerExistingAsync(email);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}