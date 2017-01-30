using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

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
        public int? Id { get; set; }
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

        public List<int> MandatoryDocumentTypes { get; set; }
    }
}