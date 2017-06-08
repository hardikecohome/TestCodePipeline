using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models
{
    public class ClientsInformationViewModel
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string Client { get; set; }

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
        public string SalesRep { get; set; }
        public string Value { get; set; }
        public string RemainingDescription { get; set; }
        public string PaymentType { get; set; }
        public bool IsNewlyCreated { get; set; }
        public bool IsCreatedByCustomer { get; set; }
        public string PreApprovalAmount { get; set; }
        public bool IsInternal { get; set; }
        public string Improvement { get; set; }
        public string SalesAgent { get; set; }
    }
}