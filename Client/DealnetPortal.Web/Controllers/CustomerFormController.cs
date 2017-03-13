using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    public class CustomerFormController : Controller
    {
        // GET: CustomerForm
        public ActionResult Index(string dealerName, string culture)
        {
            return View();
        }
        // GET: CustomerForm
        public ActionResult AgreementSubmitSuccess()
        {
            return View();
        }
    }
}