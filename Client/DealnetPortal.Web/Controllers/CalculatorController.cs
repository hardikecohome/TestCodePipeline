using DealnetPortal.Web.Infrastructure;

using System.Threading.Tasks;
using System.Web.Mvc;

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
            return View(await _contractManager.GetStandaloneCalculatorInfoAsync());
        }
    }
}