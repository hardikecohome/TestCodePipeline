using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{

    public class MortgageBrokerController : Controller
    {
        public ActionResult NewCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewCustomer(NewCustomerViewModel newCustomer)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            return RedirectToAction("CustomerCreationSuccess", new { contractId =  0 });
        }

        public ActionResult CustomerCreationSuccess(int contractId)
        {
            return View();
        }

        public ActionResult MyCustomers()
        {
            return View();
        }
    }
}