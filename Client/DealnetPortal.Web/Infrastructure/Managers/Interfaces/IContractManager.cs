using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IContractManager
    {
        //New Version 
        Task<EquipmentInformationViewModelNew> GetEquipmentInfoAsync(int contractId);
        Task<IList<Alert>> UpdateContractAsyncNew(EquipmentInformationViewModelNew equipmnetInfo);

        Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId);

        Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId);

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

	    Task<StandaloneCalculatorViewModel> GetStandaloneCalculatorInfoAsync();

        Task<bool> CheckRateCard(int contractId, int? rateCardId);
        Task<ESignatureViewModel> GetContractSignatureStatus(int contractId);
    }
}
