using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models
{
    public class AdditionalInfoViewModel
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Status")]
        public Enumeration.ContractState ContractState { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "Status")]
        public string Status { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "Date")]
        public DateTime? LastUpdateTime { get; set; }
        public string TransactionId { get; set; }
        public bool IsCreatedByCustomer { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public List<string> CustomerComments { get; set; }
    }
}