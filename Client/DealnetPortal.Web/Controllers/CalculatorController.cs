using System.Security.Claims;
using DealnetPortal.Web.Infrastructure;

using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class CalculatorController : Controller
    {
        private readonly IContractManager _contractManager;

        public CalculatorController(IContractManager contractManager)
        {
            _contractManager = contractManager;
        }

        public async Task<ActionResult> Index()
        {
	        var identity = (ClaimsIdentity) User.Identity;
	        var quebecDealer = identity.HasClaim(ClaimContstants.QuebecDealer, "True");
	        var clarityDealer = identity.HasClaim(ClaimContstants.ClarityDealer, "True");
	        ViewBag.ReducationAvailable = !quebecDealer && !clarityDealer;

            return View(await _contractManager.GetStandaloneCalculatorInfoAsync());
        }
    }
}