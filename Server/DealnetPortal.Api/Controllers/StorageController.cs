﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using AutoMapper;
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

        public StorageController(ILoggingService loggingService, 
            IContractRepository contractRepository, IFileRepository fileRepository, IUnitOfWork unitOfWork) : base(loggingService)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
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

                    //var addEquipments = newAgreementTemplate.EquipmentTypes;
                    //if (addEquipments?.Any() ?? false)
                    //{
                    //    var dbEquipments = _contractRepository.GetEquipmentTypes();
                    //    var eqToAdd = dbEquipments.Where(eq => addEquipments.Any(a => a == eq.Type)).ToList();
                    //    newAgreement.EquipmentTypes = eqToAdd;
                    //}

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
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(request.Content.ReadAsStreamAsync().Result);

            LoggingService.LogInfo("DocuSign message recieved");
            LoggingService.LogInfo(xmldoc.ToString());

            return Ok();
        }


        //// GET: api/Storage
            //public IEnumerable<string> Get()
            //{
            //    return new string[] { "value1", "value2" };
            //}

            //// GET: api/Storage/5
            //public string Get(int id)
            //{
            //    return "value";
            //}

            //// POST: api/Storage
            //public void Post([FromBody]string value)
            //{
            //}

            //// PUT: api/Storage/5
            //public void Put(int id, [FromBody]string value)
            //{
            //}

            //// DELETE: api/Storage/5
            //public void Delete(int id)
            //{
            //}        
        }
}
