﻿using System.Threading.Tasks;
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

        /// <summary>
        /// Customer information update to Aspire
        /// </summary>
        /// <param name="dealUploadRequest"></param>
        /// <returns></returns>
        Task<DealUploadResponce> CustomerUploadSubmission(DealUploadRequest dealUploadRequest);

        /// <summary>
        /// Credit Check Submission
        /// </summary>
        /// <param name="dealUploadRequest"></param>
        /// <returns></returns>
        Task<DealUploadResponce> CreditCheckSubmission(DealUploadRequest dealUploadRequest);
    }
}
