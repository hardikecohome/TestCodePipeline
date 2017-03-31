using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class SubmittedCustomerFormViewModel
    {
        public decimal CreditAmount { get; set; }
        public string DealerName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
