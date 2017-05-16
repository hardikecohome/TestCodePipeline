using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.MyProfile;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
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
        public ActionResult SetProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }
    }
}