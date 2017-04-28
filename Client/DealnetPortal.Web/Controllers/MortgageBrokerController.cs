using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    public class MortgageBrokerController : Controller
    {
        private readonly ICustomerManager _customerManager;

        public MortgageBrokerController(ICustomerManager customerManager)
        {
            _customerManager = customerManager;
        }

        public async Task<ActionResult> NewCustomer()
        {
            return View(await _customerManager.GetTemplateAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewCustomer(NewCustomerViewModel newCustomer)
        {
            //if (!ModelState.IsValid)
            //{
            //    ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
            //    return View();
            //}

            //var newCustomerDto = new NewCustomerDTO();
            //newCustomerDto.PrimaryCustomer = Mapper.Map<CustomerDTO>(newCustomer.HomeOwner);
            //newCustomerDto.PrimaryCustomer.Locations = new List<LocationDTO>();
            //var mainAddress = Mapper.Map<LocationDTO>(newCustomer.HomeOwner.AddressInformation);
            //mainAddress.AddressType = AddressType.MainAddress;
            //newCustomerDto.PrimaryCustomer.Locations.Add(mainAddress);
            //if (newCustomer.HomeOwner.PreviousAddressInformation != null)
            //{
            //    var previousAddress = Mapper.Map<LocationDTO>(newCustomer.HomeOwner.PreviousAddressInformation);
            //    previousAddress.AddressType = AddressType.PreviousAddress;
            //    newCustomerDto.PrimaryCustomer.Locations.Add(previousAddress);
            //}
            //var customerContactInfo = Mapper.Map<CustomerDataDTO>(newCustomer.HomeOwnerContactInfo);
            //newCustomerDto.PrimaryCustomer.Emails = customerContactInfo.Emails;
            //newCustomerDto.PrimaryCustomer.Phones = customerContactInfo.Phones;
            //newCustomerDto.CustomerComment = newCustomer.CustomerComment;
            //newCustomerDto.HomeImprovementTypes = newCustomer.HomeImprovementTypes;
            //var submitResult = await _contractServiceAgent.CreateContractForCustomer(newCustomerDto);

            //if (submitResult?.Any(x => x.Type == AlertType.Error) ?? false)
            //{
            //    TempData[PortalConstants.CurrentAlerts] = submitResult;
            //    return RedirectToAction("Error", "Info");
            //}

            return RedirectToAction("CustomerCreationSuccess", new {contractId = 0});
        }

        public ActionResult CustomerCreationSuccess(int id)
        {
            return View();
        }

        public ActionResult MyCustomers()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetCreatedContracts()
        {
            var contracts = (await _contractServiceAgent.GetCreatedContracts()).OrderByDescending(x => x.LastUpdateTime).ToList();
            var contractsVms = AutoMapper.Mapper.Map<IList<DealItemOverviewViewModel>>(contracts);

            return Json(contractsVms, JsonRequestBehavior.AllowGet);
        }
    }
}