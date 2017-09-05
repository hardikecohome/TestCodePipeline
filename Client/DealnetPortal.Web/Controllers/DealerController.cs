using System;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : UpdateController
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

        [HttpGet]
        public async Task<ActionResult> ResumeOnBoarding(string key)
        {
            return View("OnBoarding", await _dealerOnBoardingManager.GetDealerOnBoardingFormAsync(key));
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
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
                    LicenseDocuments = (await _dictionaryServiceAgent.GetAllLicenseDocuments()).Item1
                };

                return View(model);
            }

            var result = await _dealerOnBoardingManager.SubmitOnBoarding(model);

            if(result != null && result.Any(x => x.Type == AlertType.Error))
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
            
            return PartialView("OnBoarding/_SaveAndResumeModal", result);
        }

        public ActionResult OnBoardingSuccess()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendDraftLink(SaveAndResumeViewModel model)
        {
            var result = await _dealerOnBoardingManager.SendDealerOnboardingDraftLink(model);
            if (result == null || !result.Any()) { return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<JsonResult> UploadDocument()
        {
            if (Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }

            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };

            var isCheque = file.FileName.ToLowerInvariant().Contains("cheque");
            var result = await _dealerOnBoardingManager.UploadOnboardingDocument(scanningRequest, isCheque);

            return Json(string.Empty);
        }
    }
}