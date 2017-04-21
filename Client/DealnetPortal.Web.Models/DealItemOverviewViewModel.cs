using System;

namespace DealnetPortal.Web.Models
{
    public class DealItemOverviewViewModel
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

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
    }
}