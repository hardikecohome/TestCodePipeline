using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class AsyncCancellationController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CancelDocumentUpload(string operationGuid)
        {
            if (Session["CancelledUploadOperations"] == null)
            {
                Session["CancelledUploadOperations"] = new HashSet<string>();
            }
            ((HashSet<string>)Session["CancelledUploadOperations"]).Add(operationGuid);
            return Json(new { isSuccess = true });
        }
    }
}