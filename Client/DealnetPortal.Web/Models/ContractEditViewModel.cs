using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{

    public class DocumentType
    {
        public int DocumentTypeId { get; set; }

        public string Description { get; set; }
    }
    
    public class ExistingDocument
    {
        public int DocumentId { get; set; }

        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
    }

    public class DocumentForUpload
    {
        public int ContractId { get; set; }
        //[Required]
        public string DocumentName { get; set; }

        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "Please, select a document file")]
        public HttpPostedFileBase File { get; set; }

        public string OperationGuid { get; set; }
    }

    public class UploadDocumentsViewModel
    {
        public List<SelectListItem> DocumentTypes { get; set; }

        public List<ExistingDocument> ExistingDocuments { get; set; }

        public List<DocumentForUpload> DocumentsForUpload { get; set; }
    }

    public class ContractEditViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        //public SendEmailsViewModel SendEmails { get; set; }
        public AdditionalInfoViewModel AdditionalInfo { get; set; }
        public double ProvinceTaxRate { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public LoanCalculator.Output LoanCalculatorOutput { get; set; }

        public UploadDocumentsViewModel UploadDocumentsInfo { get; set; }
        
    }
}