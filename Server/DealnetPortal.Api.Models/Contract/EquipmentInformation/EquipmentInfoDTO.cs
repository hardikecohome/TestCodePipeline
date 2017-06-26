using System;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EquipmentInfoDTO
    {
        public int Id { get; set; }
        public AgreementType AgreementType { get; set; }
        public List<NewEquipmentDTO> NewEquipment { get; set; }
        public List<ExistingEquipmentDTO> ExistingEquipment { get; set; }
        public decimal? TotalMonthlyPayment { get; set; }

        public int? RequestedTerm { get; set; }

        public int? LoanTerm { get; set; }

        public int? AmortizationTerm { get; set; }

        public DeferralType DeferralType { get; set; }

        public double? CustomerRate { get; set; }

        public double? AdminFee { get; set; }

        public double? DownPayment { get; set; }

        public double? ValueOfDeal { get; set; }

        public string SalesRep { get; set; }

        //public string Notes { get; set; }
        public DateTime? PreferredStartDate { get; set; }

        public DateTime? EstimatedInstallationDate { get; set; }

        public DateTime? InstallationDate { get; set; }

        public string InstallerFirstName { get; set; }

        public string InstallerLastName { get; set; }

        public int ContractId { get; set; }
    }
}
