using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    //[Authorize(Roles = "Dealer")]
    [Authorize]
    public class CalculatorController : Controller
    {
        private readonly IContractManager _contractManager;

        public CalculatorController(IContractManager contractManager)
        {
            _contractManager = contractManager;
        }

        //public async Task<ActionResult> Index()
        //{
        //    var viewModel = new LoanCalculatorViewModel
        //    {
        //        EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
        //        ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1
        //    };

        //    return View(viewModel);
        //}

        public async Task<ActionResult> Index()
        {
            return View("IndexJs", await _contractManager.GetStandaloneCalculatorInfoAsync());
        }
    }
}