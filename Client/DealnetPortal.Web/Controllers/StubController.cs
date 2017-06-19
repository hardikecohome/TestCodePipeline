using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    public class StubController : Controller
    {
        // GET: Stub
        public ActionResult CreditCheck()
        {
            return View();
        }

        public ActionResult MyDeals()
        {
            return View();
        }

        public ActionResult Reports()
        {
            return View();
        }
    }
}