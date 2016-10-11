﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IContractManager
    {
        Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId);

        Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId);

        Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId);

        Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId);

        void MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract);

        void MapContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo, ContractDTO contract);

        Task<IList<Alert>> UpdateContractAsync(BasicInfoViewModel basicInfo);

        Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo);

        Task<IList<Alert>> UpdateContractAsync(ContactAndPaymentInfoViewModel contactAndPaymentInfo);
    }
}
