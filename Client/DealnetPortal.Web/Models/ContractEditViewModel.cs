using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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

    public class UploadDocument
    {
        //[Required]
        public string DocumentName { get; set; }

        public int DocumentType { get; set; }

        [Required(ErrorMessage = "Please, select a document file")]
        public HttpPostedFileBase File { get; set; }
    }

    public class UploadDocumentsViewModel
    {
        public List<DocumentType> DocumentTypes { get; set; }

        public List<ExistingDocument> ExistingDocuments { get; set; }

        public List<UploadDocument> UploadDocuments { get; set; }
    }

    public class ContractEditViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        //public SendEmailsViewModel SendEmails { get; set; }
        public AdditionalInfoViewModel AdditionalInfo { get; set; }
        public double ProvinceTaxRate { get; set; }
        
    }
}