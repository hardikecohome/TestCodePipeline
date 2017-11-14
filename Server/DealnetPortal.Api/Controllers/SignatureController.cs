using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Signature")]
    public class SignatureController : BaseApiController
    {
        private readonly ISignatureService SignatureService;
        public SignatureController(ILoggingService loggingService, ISignatureService signatureService)
            : base(loggingService)
        {
            SignatureService = signatureService;
        }

        //// GET: api/Signature
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Signature/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Signature
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Signature/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        [Route("Cancel")]
        [HttpPost]
        public async Task<IHttpActionResult> Cancel(int contractId)
        {
            try
            {
                var result = await SignatureService.CancelSignatureProcess(contractId, LoggedInUser.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return InternalServerError(ex);
            }
        }
    }
}
