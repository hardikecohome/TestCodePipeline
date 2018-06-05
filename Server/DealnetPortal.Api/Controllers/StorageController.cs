using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Api.Integration.Interfaces;

namespace DealnetPortal.Api.Controllers
{
    [RoutePrefix("api/Storage")]
    public class StorageController : BaseApiController
    {        
        private readonly IDocumentService _documentService;

        public StorageController(ILoggingService loggingService,IDocumentService documentService) : base(loggingService)
        {            
            _documentService = documentService;
        }

        [AllowAnonymous]
        [Route("NotifySignatureStatus")]
        [HttpPost]
        public async Task<IHttpActionResult> PostNotifySignatureStatus(HttpRequestMessage request)
        {
            try
            {
                var requestStr = await request.Content.ReadAsStringAsync();
                var alerts = await _documentService.ProcessSignatureEvent(requestStr);
                if (alerts?.All(a => a.Type != AlertType.Error) == true)
                {
                    return Ok();
                }
                return BadRequest(alerts?.FirstOrDefault()?.Message);

            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error occurred when received request from DocuSign", ex);
                return InternalServerError();
            }            
        }         
    }
}
