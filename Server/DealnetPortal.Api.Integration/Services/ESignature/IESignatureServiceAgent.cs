using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Api.Integration.Services.ESignature
{
    public interface IESignatureServiceAgent
    {
        Task<IList<Alert>> Login(string userName, string organisation, string password);

        Task<bool> Logout();

        Task<Tuple<EOriginalTypes.transactionType, IList<Alert>>> CreateTransaction(string transactionName);

        Task<Tuple<EOriginalTypes.documentProfileType, IList<Alert>>> CreateDocumentProfile(long transactionSid, string dptName, string dpName = null);
        //TODO: check returned value
        Task<Tuple<EOriginalTypes.documentVersionType, IList<Alert>>> UploadDocument(long dpSid, byte[] pdfDocumentData, string documentFileName);
        //TODO: check returned value
        Task<Tuple<EOriginalTypes.documentVersionType, IList<Alert>>> InsertFormFields(long dpSid, textField[] textFields, TextData[] textData, SigBlock[] sigBlocks);

        Task<IList<Alert>> ConfigureSortOrder(long transactionSid, long[] dpSids);

        Task<IList<Alert>> ConfigureRoles(long transactionSid, eoConfigureRolesRole[] roles);

        Task<IList<Alert>> ConfigureInvitation(long transactionSid, string roleName, string senderFirstName, string senderLastName, string senderEmail);
    }
}
