using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class ShareableLinkController : UpdateController
    {
        private readonly ICustomerFormServiceAgent _customerFormServiceAgent;
        public ShareableLinkController(ICustomerFormServiceAgent customerFormServiceAgent)
        {
            _customerFormServiceAgent = customerFormServiceAgent;
        }

        public async Task<ActionResult> Index()
        {
            var customerLinkDto = await _customerFormServiceAgent.GetShareableLinkSettings();
            var customerLink = AutoMapper.Mapper.Map<ShareableLinkViewModel>(customerLinkDto);
            customerLink.HashDealerName = SecurityUtils.Hash(User.Identity.Name);
            return View(customerLink);
        }

        [Authorize(Roles = "Dealer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Index(ShareableLinkViewModel customerLink)
        {
            if (!ModelState.IsValid)
            {
                return GetErrorJson();
            }
            var customerLinkDto = AutoMapper.Mapper.Map<CustomerLinkDTO>(customerLink);
            if (customerLinkDto.EnabledLanguages == null)
            {
                customerLinkDto.EnabledLanguages = new List<LanguageCode>();
            }
            if (customerLinkDto.Services == null)
            {
                customerLinkDto.Services = new Dictionary<LanguageCode, List<string>>();
            }
            var alerts = await _customerFormServiceAgent.UpdateShareableLinkSettings(customerLinkDto);
            return alerts.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}