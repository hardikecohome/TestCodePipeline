using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IContractManager
    {
        //New Version 
        Task<EquipmentInformationViewModelNew> GetEquipmentInfoAsyncNew(int contractId);
        Task<IList<Alert>> UpdateContractAsyncNew(EquipmentInformationViewModelNew equipmnetInfo);
        Task<IList<Alert>> UpdateContractAsyncNew(ContactAndPaymentInfoViewModelNew equipmnetInfo);

        Task<ContactAndPaymentInfoViewModelNew> GetAdditionalContactInfoAsyncNew(int contractId);

        Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId);

        Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId);

        Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId);

        Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId, ContractDTO contract = null);

        Task<ContractViewModel> GetContractAsync(int contractId);

        Task<IList<ContractViewModel>> GetContractsAsync(IEnumerable<int> ids);

        Task<ContractEditViewModel> GetContractEditAsync(int contractId);

        Task MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract);

        void MapContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo, ContractDTO contract);

        Task<IList<Alert>> UpdateContractAsync(BasicInfoViewModel basicInfo);

        Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo);

        Task<IList<Alert>> UpdateContractAsync(ContactAndPaymentInfoViewModel contactAndPaymentInfo);

        Task<IList<Alert>> UpdateApplicants(ApplicantsViewModel basicInfo);

        /// <summary>
        /// Create a new contract (application) with a same home owner
        /// </summary>
        Task<Tuple<int?, IList<Alert>>> CreateNewCustomerContract(int contractId);
    }
}
