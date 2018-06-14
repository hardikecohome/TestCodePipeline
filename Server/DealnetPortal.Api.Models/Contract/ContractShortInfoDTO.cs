using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    /// <summary>
    /// Contract short information
    /// </summary>
    public class ContractShortInfoDTO
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public ContractState ContractState { get; set; }
        public AgreementType? AgreementType { get; set; }
        public string ProgramOption { get; set; }
        public string Status { get; set; }
        public string LocalizedStatus { get; set; }        
        public DateTime LastUpdateTime { get; set; }
        public SignatureStatus? SignatureStatus { get; set; }
        public DateTime? SignatureStatusLastUpdateTime { get; set; }

        public string PrimaryCustomerFirstName { get; set; }
        public string PrimaryCustomerLastName { get; set; }
        public string PrimaryCustomerEmail { get; set; }
        public string PrimaryCustomerPhone { get; set; }
        public string PrimaryCustomerAddress { get; set; }
        public string PrimaryCustomerPostalCode { get; set; }
        public string CustomerComments { get; set; }

        public string Equipment { get; set; }
        public decimal? ValueOfDeal { get; set; }
        public decimal? PreApprovalAmount { get; set; }
        public int? LoanTerm { get; set; }
        public int? AmortizationTerm { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public PaymentType? PaymentType { get; set; }

        public string CreatedBy { get; set; }
        public string SalesRep { get; set; }

        public bool IsNewlyCreated { get; set; }
        public bool IsCreatedByCustomer { get; set; }
        public bool IsLead { get; set; }

        public string ActionRequired { get; set; }
        /// <summary>
        /// Used for concatenate all important deal information for search purposes
        /// </summary>
        public string SearchDescription { get; set; }
    }
}
