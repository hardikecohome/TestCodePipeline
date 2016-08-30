using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace DealnetPortal.Web.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class ReportsController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> WorkItems()
        {
            return this.Json(new[]
            {

                new[] {"1", "1", "1", "1", "1", "1", "1"},
                new[] {"2", "2", "2", "2", "2", "2", "2"},
                new[] {"3", "3", "3", "3", "3", "3", "3"}

            },JsonRequestBehavior.AllowGet);
        }
    }
}
