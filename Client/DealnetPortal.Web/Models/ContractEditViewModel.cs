using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{
    public class ContractEditViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        //public SendEmailsViewModel SendEmails { get; set; }
        public AdditionalInfoViewModel AdditionalInfo { get; set; }
        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public LoanCalculator.Output LoanCalculatorOutput { get; set; }
        public UploadDocumentsViewModel UploadDocumentsInfo { get; set; }
        public CertificateInformationViewModel InstallCertificateInformation { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public List<string> CustomerComments { get; set; }
    }
}