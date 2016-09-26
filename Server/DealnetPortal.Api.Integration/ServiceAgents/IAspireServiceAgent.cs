using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire;

namespace DealnetPortal.Api.Integration.ServiceAgents
{
    /// <summary>
    /// Service agent for communicate with ASPIRE API
    /// </summary>
    public interface IAspireServiceAgent
    {
        /// <summary>
        /// Call deal upload submission
        /// </summary>
        /// <param name="dealUploadRequest"></param>
        /// <returns></returns>
        Task<DealUploadResponce> DealUploadSubmission(DealUploadRequest dealUploadRequest);
    }
}
