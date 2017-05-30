using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class CalculatorController : Controller
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        public CalculatorController(IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = new LoanCalculatorViewModel
            {
                EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
                ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1
            };

            return View(viewModel);
        }
    }
}