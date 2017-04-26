using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class NewCustomerViewModel
    {
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public CustomerContactInfoViewModel HomeOwnerContactInfo { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedMoveInDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedMoveInDate { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "HomeImprovementType")]
        public List<string> HomeImprovementTypes { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public string CustomerComment { get; set; }

        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
    }
}
