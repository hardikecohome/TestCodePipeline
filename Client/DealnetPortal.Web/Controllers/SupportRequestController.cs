using AutoMapper;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Notify;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.Http;

namespace DealnetPortal.Web.Controllers
{
    public class SupportRequestController : Controller
    {
        private readonly IDealerServiceAgent _dealerServiceAgent;

        public SupportRequestController(IDealerServiceAgent dealerServiceAgent)
        {
            _dealerServiceAgent = dealerServiceAgent;
        }

        // GET: SupportRequest
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> DealerSupportRequestEmail(SupportRequestDTO dealerSupportRequest)
        {
            var result = await _dealerServiceAgent.DealerSupportRequestEmail(dealerSupportRequest);
            return "ok";
        }
    }
}