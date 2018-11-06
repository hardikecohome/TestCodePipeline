using System;
using System.Collections.Generic;
using System.Configuration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.MyProfile;

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
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

            var identity = (ClaimsIdentity)User.Identity;

            model.QuebecDealer = identity.HasClaim(ClaimContstants.QuebecDealer, "True");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SetProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .ToList();
                return Json(new { Errors = errorList }, JsonRequestBehavior.AllowGet);
            }
            var quebecPostalCodes = ConfigurationManager.AppSettings[PortalConstants.QuebecPostalCodesNameKey].Split(',');
            if (model.QuebecDealer && (model.PostalCodes != null && model.PostalCodes.Any(x => quebecPostalCodes.All(p=> p != x.PostalCode[0].ToString()))))
            {
                return Json(new { Errors = new List<string> {Resources.Resources.ServiceAreaInQc } }, JsonRequestBehavior.AllowGet);
            }

            var alerts = await _profileManager.UpdateDealerProfile(model);

            return alerts.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}