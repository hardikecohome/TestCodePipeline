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
        public  async Task<ActionResult> UploadDocument(HttpPostedFileBase files, FormCollection form)
        {
            byte[] _documentBytes;        
            int _documentId, _contractId = 0;
            bool isDocument = int.TryParse(form[0], out _documentId);
            bool isContractId = int.TryParse(form[1], out _contractId);
            if (files != null && isDocument && isContractId) 
            {               
                using (var reader = new BinaryReader(files.InputStream))
                {
                    _documentBytes = reader.ReadBytes(files.ContentLength);
                }             
                var document = new ContractDocumentDTO
                {
                    DocumentTypeId = _documentId,
                    DocumentBytes = _documentBytes,
                    DocumentName = files.FileName, 
                    ContractId = _contractId
                };            
                    await _contractServiceAgent.AddDocumentToContract(document);              

            return Json("File was saved", JsonRequestBehavior.DenyGet);
    }
            return Json("occurred error", JsonRequestBehavior.DenyGet);
        }
    }
}