using System;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : Controller
    {
        private readonly IDealerOnBoardingManager _dealerOnBoardingManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public DealerController(IDealerOnBoardingManager dealerOnBoardingManager, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _dealerOnBoardingManager = dealerOnBoardingManager;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        [HttpGet]
        public async Task<ActionResult> OnBoarding(string onboardingLink)
        {
            return View(await _dealerOnBoardingManager.GetNewDealerOnBoardingForm(onboardingLink));
        }

        // GET: Dealer
        [HttpGet]
        public async Task<ActionResult> ResumeOnBoarding(string key)
        {
            return View("OnBoarding",await _dealerOnBoardingManager.GetDealerOnBoardingFormAsync(key));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OnBoarding(DealerOnboardingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.GetModelErrors();
                model.DictionariesData = new DealerOnboardingDictionariesViewModel
                {
                    ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList()
                };

                return View(model);
            }
            var result = await _dealerOnBoardingManager.SubmitOnBoarding(model);
            if(result != null ||(result?.Any(x => x.Type == AlertType.Error) ?? false))
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            return RedirectToAction("OnBoardingSuccess");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveDraft(DealerOnboardingViewModel model)
        {
            var result = await _dealerOnBoardingManager.SaveDraft(model);
            var modal = new SaveAndResumeViewModel
            {
                AccessKey = result.Item1?.AccessKey ?? model.AccessKey,
                Success = result.Item2 != null ? !result.Item2.Any(a => a.Type == AlertType.Error) : true,
                Alerts = result.Item2,
                Email = model.Owners.Any() && !string.IsNullOrEmpty(model.Owners.First().EmailAddress)
                                        ? model.Owners.First().EmailAddress
                                        : !string.IsNullOrEmpty(model.CompanyInfo.EmailAddress)
                                            ? model.CompanyInfo.EmailAddress
                                            : String.Empty
            };
            return PartialView("OnBoarding/_SaveAndResumeModal", modal);
        }

        public ActionResult OnBoardingSuccess()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendLink(SaveAndResumeViewModel model)
        {
            var result = await _dealerOnBoardingManager.SendEmail(model);
            if (result == null || !result.Any()) { return Json(new { success = true }); }
            return Json(new { success = false });
        }
    }
}