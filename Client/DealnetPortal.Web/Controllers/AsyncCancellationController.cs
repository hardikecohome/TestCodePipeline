using System.Collections.Generic;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
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