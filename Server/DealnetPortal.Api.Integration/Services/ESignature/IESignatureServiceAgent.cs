using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Api.Integration.Services.ESignature
{
    public interface IESignatureServiceAgent
    {
        Task<IList<Alert>> Login(string userName, string organisation, string password);

        Task<bool> Logout();

        Task<Tuple<EOriginalTypes.transactionType, IList<Alert>>> CreateTransaction(string transactionName);

        Task<Tuple<EOriginalTypes.documentProfileType, IList<Alert>>> CreateDocumentProfile(long transactionSid, string dptName, string dpName = null);

        Task<Tuple<EOriginalTypes.documentVersionType, IList<Alert>>> UploadDocument(long dpSid, byte[] pdfDocumentData, string documentFileName);
    }
}
