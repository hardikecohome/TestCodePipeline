namespace DealnetPortal.Web.Models
{
    public class DealItemOverviewViewModel
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }
        public string StatusColor { get; set; }

        public string LocalizedStatus { get; set; }
        public string SignatureStatus { get; set; }
        public string SignatureStatusColor { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string Action { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }

        public string Date { get; set; }
        public string Equipment { get; set; }
        public string SalesRep { get; set; }
        public string Value { get; set; }
        public string RemainingDescription { get; set; }
        public string AgreementType { get; set; }
        public string PaymentType { get; set; }
        public bool IsNewlyCreated { get; set; }
        public bool IsCreatedByCustomer { get; set; }
        public string PostalCode { get; set; }
        public string PreApprovalAmount { get; set; }
        public string CustomerComment { get; set; }
        public string ContractNotes { get; set; }

        public bool IsInternal { get; set; }
        public int? RateCardId { get; set; }

        public string CreditExpiry { get; set; }
        public string Address { get; set; }
        public string ProgramOption { get; set; }
        public bool HasRateReduction { get; set; }
        public string LoanAmount { get; set; }
        public int? Term { get; set; }
        public int? Amort { get; set; }
        public string MonthlyPayment { get; set; }
        public string EnteredBy { get; set; }
        public bool Lead { get; set; }
    }
}