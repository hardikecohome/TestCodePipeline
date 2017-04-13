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
    /// Service for e-signature of contracts
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
        Task<IList<Alert>> ProcessContract(int contractId, string ownerUserId, SignatureUser[] signatureUsers);

        Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId, string ownerUserId);

        Task<Tuple<bool, IList<Alert>>> CheckPrintAgreementAvailable(int contractId, int documentTypeId, string ownerUserId);

        Task<Tuple<AgreementDocument, IList<Alert>>> GetPrintAgreement(int contractId, string ownerUserId);

        Task<Tuple<AgreementDocument, IList<Alert>>> GetInstallCertificate(int contractId, string ownerUserId);

        IList<Alert> GetSignatureResults(int contractId, string ownerUserId);

        SignatureStatus GetSignatureStatus(int contractId, string ownerUserId);
    }
}
