using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AutoMapper;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Hosting;

namespace DealnetPortal.Api.Controllers
{
    [RoutePrefix("api/Storage")]
    public class StorageController : BaseApiController
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly ISignatureService _signatureService;

        public StorageController(ILoggingService loggingService, IContractService contractService,
            IContractRepository contractRepository, IFileRepository fileRepository, IUnitOfWork unitOfWork, ISignatureService signatureService) : base(loggingService)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
            _contractService = contractService;
            _signatureService = signatureService;
        }

        //[AllowAnonymous]
        //[Route("UploadAgreementTemplate")]
        //[HttpPost]
        //public async Task<IHttpActionResult> PostUploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate)
        //{
        //    try
        //    {
        //        var result = await Task.Run(() =>
        //        {
        //            var newAgreement = Mapper.Map<AgreementTemplate>(newAgreementTemplate);

        //            if (string.IsNullOrEmpty(newAgreement.DealerId) &&
        //                !string.IsNullOrEmpty(newAgreementTemplate.DealerName))
        //            {
        //                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //                var user = userManager?.FindByNameAsync(newAgreementTemplate.DealerName).GetAwaiter().GetResult();
        //                if (user != null)
        //                {
        //                    newAgreement.DealerId = user.Id;
        //                }
        //            }

        //            var addedAgreement = _fileRepository.AddOrUpdateAgreementTemplate(newAgreement);
        //            _unitOfWork.Save();
        //            return addedAgreement;
        //        });
        //        var resDto = Mapper.Map<AgreementTemplateDTO>(result);                
        //        return Ok(resDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}


        [AllowAnonymous]
        [Route("NotifySignatureStatus")]
        [HttpPost]
        public IHttpActionResult PostNotifySignatureStatus(HttpRequestMessage request)
        {
            try
            {
                XDocument xDocument = XDocument.Parse(request.Content.ReadAsStringAsync().Result);
                var xmlns = xDocument?.Root?.Attribute(XName.Get("xmlns"))?.Value ?? "http://www.docusign.net/API/3.0";

                var envelopeStatus = xDocument.Root.Element(XName.Get("EnvelopeStatus", xmlns));
                var envelopeId = envelopeStatus?.Element(XName.Get("EnvelopeID", xmlns))?.Value;
                var status = envelopeStatus?.Element(XName.Get("Status", xmlns))?.Value;

                if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(envelopeId))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }                
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error occurred when received request from DocuSign", ex);
                return InternalServerError();
            }            
        }         
    }
}
