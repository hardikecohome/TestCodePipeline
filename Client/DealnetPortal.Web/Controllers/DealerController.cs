﻿using System;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;

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
        public async Task<ActionResult> OnBoarding(string key)
        {
            var result = await _dealerOnBoardingManager.GetNewDealerOnBoardingForm(key);

            if (result == null)
            {
                return RedirectToAction("OnBoardingError");
            }

            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> ResumeOnBoarding(string key)
        {
            var result = await _dealerOnBoardingManager.GetDealerOnBoardingFormAsync(key);

            if (result == null)
            {
                return RedirectToAction("OnBoardingError");
            }

            return View("OnBoarding", result);
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

        public ActionResult OnBoardingError()
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
        public async Task<JsonResult> UploadDocument(OnboardingDocumentForUpload documentForUpload)
        {
            if (documentForUpload?.File?.ContentLength <= 0)
            {
                return GetErrorJson();
            }

            var result = await _dealerOnBoardingManager.UploadOnboardingDocument(documentForUpload);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteDocument(OnboardingDocumentForDelete documentForDelete)
        {
            var result = await _dealerOnBoardingManager.DeleteOnboardingDocument(documentForDelete);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}