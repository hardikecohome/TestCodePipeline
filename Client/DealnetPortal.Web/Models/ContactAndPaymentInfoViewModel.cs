using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models
{
    public class ContactAndPaymentInfoViewModel
    {
        public ContactInfoViewModel HomeOwnerContactInfo { get; set; }
        public List<ContactInfoViewModel> CoBorrowersContactInfo { get; set; }
        public PaymentInfoViewModel PaymentInfo { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }
        public int? ContractId { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }
        public bool IsAllPaymentTypesAvailable { get; set; }
    }
}