using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading;
using System.Web.SessionState;
using DealnetPortal.Web.Infrastructure.Extensions;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    [SessionState(SessionStateBehavior.ReadOnly)]
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
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadDocument(DocumentForUpload documentForUpload)
        {
            if (documentForUpload?.File?.ContentLength > 0)
            {
                byte[] documentBytes;
                using (var reader = new BinaryReader(documentForUpload.File.InputStream))
                {
                    documentBytes = reader.ReadBytes(documentForUpload.File.ContentLength);
                }
                var document = new ContractDocumentDTO
                {
                    Id = documentForUpload.Id ?? 0,
                    CreationDate = DateTime.Now,
                    DocumentTypeId = documentForUpload.DocumentTypeId != 0 ? documentForUpload.DocumentTypeId : 7,
                    DocumentBytes = documentBytes,
                    DocumentName = !string.IsNullOrEmpty(documentForUpload.DocumentName) ? documentForUpload.DocumentName : Path.GetFileName(documentForUpload.File.FileName),                    
                    ContractId = documentForUpload.ContractId
                };
                if (Session["CancelledUploadOperations"] != null && ((HashSet<string>)Session["CancelledUploadOperations"]).Contains(documentForUpload.OperationGuid))
                {
                    return Json(new {wasCancelled = true});
                }
                var updateResult = await _contractServiceAgent.AddDocumentToContract(document);
                var errors = updateResult.Item2.Where(r => r.Type == AlertType.Error).ToArray();
                return errors.Any() ? Json(new { isError = true, errorMessage = errors.Select(x => x.Message).Aggregate((x, y) => x + " " + y) }) : Json(new { updatedDocumentId = updateResult.Item1, isSuccess = true });
            }
            return GetErrorJson();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveDocument(int documentId)
        {
            var updateResult = await _contractServiceAgent.RemoveContractDocument(documentId);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}