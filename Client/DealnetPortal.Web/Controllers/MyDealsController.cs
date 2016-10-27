using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using System.IO;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class MyDealsController : UpdateDataController
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public MyDealsController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent,
            IContractManager contractManager) : base(contractManager)
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
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddComment(CommentViewModel comment)
        {
            if (!ModelState.IsValid || comment.ContractId == null && comment.ParentCommentId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await _contractServiceAgent.AddComment(Mapper.Map<CommentDTO>(comment));
            return updateResult.Item2.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : Json(new { updatedCommentId = updateResult.Item1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveComment(int commentId)
        {
            var updateResult = await _contractServiceAgent.RemoveComment(commentId);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
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
                {   CreationDate = DateTime.Now,
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
           var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();       

              if (contract?.Item1.Documents != null && docTypes?.Item1 != null)
               {
                //var document = contract.Item1.Documents
                //                              .OrderByDescending(x => x.Id)
                //                              .Select(i =>  new { i.DocumentName, i.DocumentTypeId })                                             
                //                              .Take(5)
                //                              .ToList();
                var document = (from i in contract.Item1.Documents
                                join p in docTypes.Item1
                                on i.DocumentTypeId equals p.Id
                                orderby i.Id descending
                                select new
                                {
                                    p.Description,
                                    i.DocumentName,
                                }).Take(5).ToList();
              
                 
                return Json(document, JsonRequestBehavior.DenyGet);
               }
            return Json(new { message = string.Format("error") }, JsonRequestBehavior.DenyGet);
        }
    }
}