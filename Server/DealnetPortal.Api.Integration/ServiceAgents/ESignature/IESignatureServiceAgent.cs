using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Api.Models;
using textField = DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation.textField;

namespace DealnetPortal.Api.Integration.ServiceAgents.ESignature
{
    public interface IESignatureServiceAgent
    {
        Task<IList<Alert>> Login(string userName, string organisation, string password);

        Task<bool> Logout();

        Task<Tuple<transactionType, IList<Alert>>> CreateTransaction(string transactionName);

        Task<Tuple<documentProfileType, IList<Alert>>> CreateDocumentProfile(long transactionSid, string dptName, string dpName = null);

        Task<Tuple<documentVersionType, IList<Alert>>> UploadDocument(long dpSid, byte[] pdfDocumentData, string documentFileName);

        Task<Tuple<documentVersionType, IList<Alert>>> InsertFormFields(long dpSid, textField[] textFields, TextData[] textData, SigBlock[] sigBlocks);

        Task<Tuple<documentVersionType, IList<Alert>>> EditFormFields(long dpSid, FormField[] formFields);

        Task<IList<Alert>> MergeData(long dpSid, TextData[] textData);

        Task<IList<Alert>> ConfigureSortOrder(long transactionSid, long[] dpSids);

        Task<IList<Alert>> ConfigureRoles(long transactionSid, eoConfigureRolesRole[] roles);

        Task<IList<Alert>> ConfigureInvitation(long transactionSid, string roleName, string senderFirstName, string senderLastName, string senderEmail);
    }
}
