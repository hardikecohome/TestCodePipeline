using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Storage")]
    public class StorageController : BaseApiController
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StorageController(ILoggingService loggingService, IFileRepository fileRepository, IUnitOfWork unitOfWork) : base(loggingService)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
        }

        [Route("UploadAgreementTemplate")]
        [HttpPost]
        public async Task<IHttpActionResult> PostUploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate)
        {
            throw new NotImplementedException();
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
