using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    public class UpdateController : Controller
    {
        protected JsonResult GetSuccessJson()
        {
            return Json(new { isSuccess = true });
        }

        protected JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}