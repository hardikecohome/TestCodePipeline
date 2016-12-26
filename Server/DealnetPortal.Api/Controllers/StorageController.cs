using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using AutoMapper;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.AspNet.Identity.Owin;

namespace DealnetPortal.Api.Controllers
{
    [RoutePrefix("api/Storage")]
    public class StorageController : BaseApiController
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContractRepository _contractRepository;
        private readonly IContractService _contractService;

        public StorageController(ILoggingService loggingService, IContractService contractService,
            IContractRepository contractRepository, IFileRepository fileRepository, IUnitOfWork unitOfWork) : base(loggingService)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
            _contractService = contractService;
        }

        [AllowAnonymous]
        [Route("UploadAgreementTemplate")]
        [HttpPost]
        public async Task<IHttpActionResult> PostUploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate)
        {
            try
            {
                var result = await Task.Run(() =>
                {
                    var newAgreement = Mapper.Map<AgreementTemplate>(newAgreementTemplate);

                    if (string.IsNullOrEmpty(newAgreement.DealerId) &&
                        !string.IsNullOrEmpty(newAgreementTemplate.DealerName))
                    {
                        var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        var user = userManager?.FindByNameAsync(newAgreementTemplate.DealerName).GetAwaiter().GetResult();
                        if (user != null)
                        {
                            newAgreement.DealerId = user.Id;
                        }
                    }

                    var addedAgreement = _fileRepository.AddOrUpdateAgreementTemplate(newAgreement);
                    _unitOfWork.Save();
                    return addedAgreement;
                });
                var resDto = Mapper.Map<AgreementTemplateDTO>(result);                
                return Ok(resDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [AllowAnonymous]
        [Route("NotifySignatureStatus")]
        [HttpPost]
        public IHttpActionResult PostNotifySignatureStatus(HttpRequestMessage request)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(request.Content.ReadAsStreamAsync().Result);

                var mgr = new XmlNamespaceManager(xmldoc.NameTable);
                mgr.AddNamespace("a", "http://www.docusign.net/API/3.0");

                XmlNode envelopeStatus = xmldoc.SelectSingleNode("//a:EnvelopeStatus", mgr);
                XmlNode envelopeId = envelopeStatus.SelectSingleNode("//a:EnvelopeID", mgr);
                XmlNode status = envelopeStatus.SelectSingleNode("//a:Status", mgr);                                            
            
                if (status?.InnerText == "Completed" && !string.IsNullOrEmpty(envelopeId?.InnerText))
                {                    
                    LoggingService.LogInfo($"DocuSign envelope {envelopeId?.InnerText} status changed to {status.InnerText}");

                    var contract = _contractRepository.FindContractBySignatureId(envelopeId?.InnerText);
                    if (contract != null)
                    {
                        XmlNode docs = xmldoc.SelectSingleNode("//a:DocumentPDFs", mgr);
                        if (docs != null)
                        {                            
                            foreach (XmlNode doc in docs.ChildNodes)
                            {
                                string documentName = doc.ChildNodes[0].InnerText;
                                if (!documentName.Contains("CertificateOfCompletion"))
                                {
                                    // pdf.SelectSingleNode("//a:Name", mgr).InnerText;
                                    string documentId = doc.ChildNodes[2].InnerText;
                                    // pdf.SelectSingleNode("//a:DocumentID", mgr).InnerText;
                                    string byteStr = doc.ChildNodes[1].InnerText;
                                    // pdf.SelectSingleNode("//a:PDFBytes", mgr).InnerText;                                    

                                    byte[] bytes = new byte[byteStr.Length * sizeof(char)];
                                    System.Buffer.BlockCopy(byteStr.ToCharArray(), 0, bytes, 0, bytes.Length);                                    

                                    ContractDocumentDTO document = new ContractDocumentDTO()
                                    {
                                        ContractId = contract.Id,
                                        CreationDate = DateTime.Now,
                                        DocumentTypeId = 1, // Signed contract !!
                                        DocumentName = documentName,
                                        DocumentBytes = bytes
                                    };

                                    _contractService.AddDocumentToContract(document, contract.DealerId);                                   
                                    LoggingService.LogInfo($"Document {documentName} with size {byteStr.Length} recieved");
                                    break; // other docs dosn't metter
                                }                                
                            }
                        }
                    }
                    else
                    {
                        LoggingService.LogWarning($"Cannot find contract for signature transactionId {envelopeId.InnerText}");
                    }                                        
                }
                return Ok();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error occurred when received request from DocuSign", ex);
                return InternalServerError();
            }
            
        }
         
    }
}
