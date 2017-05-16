using System.Threading.Tasks;
using System.Web.Mvc;
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
            var model = await _profileManager.Get();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return GetErrorJson();
            }

            return GetSuccessJson();
        }
    }
}