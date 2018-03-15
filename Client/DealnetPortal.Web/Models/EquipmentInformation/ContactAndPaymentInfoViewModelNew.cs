using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DealnetPortal.Web.Models.Enumeration;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class ContactAndPaymentInfoViewModelNew
    {
        public int? ContractId { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }
    }
}