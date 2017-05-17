using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.MyProfile;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class ProfileController : UpdateController
    {
        private readonly IProfileManager _profileManager;

        public ProfileController(IProfileManager profileManager)
        {
            _profileManager = profileManager;
        }

        public async Task<ActionResult> Index()
        {
            var model = await _profileManager.GetDealerProfile();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SetProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return GetErrorJson();
            }
            var alerts = await _profileManager.UpdateDealerProfile(model);

            return alerts.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SetProfile(ProfileViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        foreach (ModelState modelState in ViewData.ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                var t = error;
        //            }
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    var updateResult = await _profileManager.UpdateDealerProfile(model);
        //    if (updateResult.Any(r => r.Type == AlertType.Error))
        //    {
        //        TempData[PortalConstants.CurrentAlerts] = updateResult;
        //        return RedirectToAction("Error", "Info");
        //    }
        //    return RedirectToAction("Index");

        //}
    }
}