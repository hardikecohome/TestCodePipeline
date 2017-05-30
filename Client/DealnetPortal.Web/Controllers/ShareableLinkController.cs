using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using DealnetPortal.Utilities;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class ShareableLinkController : UpdateController
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        public ShareableLinkController(IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<ActionResult> Index()
        {
            var customerLinkDto = await _dictionaryServiceAgent.GetShareableLinkSettings();
            var customerLink = AutoMapper.Mapper.Map<ShareableLinkViewModel>(customerLinkDto);
            customerLink.HashDealerName = SecurityUtils.Hash(User.Identity.Name);
            return View(customerLink);
        }

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
            var alerts = await _dictionaryServiceAgent.UpdateShareableLinkSettings(customerLinkDto);
            return alerts.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}