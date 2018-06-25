using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    public class VerifyRecaptchaController:Controller
    {
        private readonly ISecurityManager _securityManager;

        public VerifyRecaptchaController(ISecurityManager securityManager)
        {
            _securityManager = securityManager;
        }

        [HttpPost]
        [Route("CustomerForm/VerifyRecaptcha")]
        public async Task<JsonResult> Index(RecaptchaViewModel res)
        {
            try
            {
                var result = await _securityManager.VerifyRecaptcha(res.Response);
                return Json(new { result });
            }
            catch(Exception)
            {
                return Json(new { result = false });
            }
        }
    }
}