using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Api.Integration.Services
{
    /// <summary>
    /// Service for e-signature and PDF-versions of contracts
    /// </summary>
    public interface ISignatureService
    {
        /// <summary>
        /// Initiate contract signature process
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="ownerUserId"></param>
        /// <param name="signatureUsers"></param>
        /// <returns></returns>
        Task<Tuple<SignatureSummaryDTO, IList<Alert>>> ProcessContract(int contractId, string ownerUserId, SignatureUser[] signatureUsers);

        Task<Tuple<SignatureSummaryDTO, IList<Alert>>> CancelSignatureProcess(int contractId, string ownerUserId);

        Task<IList<Alert>> UpdateSignatureUsers(int contractId, string ownerUserId, SignatureUser[] signatureUsers);

        Task<IList<Alert>> ProcessSignatureEvent(string notificationMsg);

        Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId, string ownerUserId);

        Task<Tuple<bool, IList<Alert>>> CheckPrintAgreementAvailable(int contractId, int documentTypeId, string ownerUserId);

        /// <summary>
        /// Get contract agreement for print. Try to use PDF template in the Database, if it not exist, try to use DocuSign template
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="ownerUserId"></param>
        /// <returns></returns>
        Task<Tuple<AgreementDocument, IList<Alert>>> GetPrintAgreement(int contractId, string ownerUserId);

        /// <summary>
        /// Get Signed contract (document). if signed doc not exists or still not completed, returns null
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="ownerUserId"></param>
        /// <returns></returns>
        Task<Tuple<AgreementDocument, IList<Alert>>> GetSignedAgreement(int contractId, string ownerUserId);

        Task<Tuple<AgreementDocument, IList<Alert>>> GetInstallCertificate(int contractId, string ownerUserId);

        IList<Alert> GetSignatureResults(int contractId, string ownerUserId);

        SignatureStatus GetSignatureStatus(int contractId, string ownerUserId);
    }
}
