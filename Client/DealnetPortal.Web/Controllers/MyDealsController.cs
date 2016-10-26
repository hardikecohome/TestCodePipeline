using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;
using System.IO;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class MyDealsController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public MyDealsController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent,
            IContractManager contractManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractManager = contractManager;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ContractEdit(int id)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetContractEditAsync(id));
        }

        [HttpPost]
        public  async Task<ActionResult> UploadDocument(DocumentForUpload documentForUpload)
        {
            byte[] _documentBytes;
            if (documentForUpload?.File?.ContentLength > 0)
            {
                using (var reader = new BinaryReader(documentForUpload.File.InputStream))
                {
                    _documentBytes = reader.ReadBytes(documentForUpload.File.ContentLength);
                }
                var document = new ContractDocumentDTO
                {
                    DocumentTypeId = documentForUpload.DocumentTypeId != 0 ? documentForUpload.DocumentTypeId : 7,
                    DocumentBytes = _documentBytes,
                    DocumentName = !string.IsNullOrEmpty(documentForUpload.DocumentName) ? documentForUpload.DocumentName : documentForUpload.File.FileName,                    
                    ContractId = documentForUpload.ContractId
                };
                await _contractServiceAgent.AddDocumentToContract(document);

                return Json(new { message = string.Format("success") }, JsonRequestBehavior.DenyGet);
            }
            return Json(new { message = string.Format("error") }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<ActionResult> UploadedList(int id)
        {
           var contract = await _contractServiceAgent.GetContract(id);
            if (contract.Item1.Documents != null)
            {
                var document = contract.Item1.Documents.Where(i => i.ContractId == id)
                                                        .Select(i => i.DocumentName)
                                                        .Take(5)
                                                        .ToList();

                return Json(document, JsonRequestBehavior.DenyGet);
            }
            return Json(new { message = string.Format("error") }, JsonRequestBehavior.DenyGet);
        }
    }
}