﻿using System;
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
        public DateTime? LastUpdateTime { get; set; }

        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; }
    }

    public class OnboardingDocumentForUpload
    {
        public string DocumentName { get; set; }

        public int DocumentTypeId { get; set; }

        public int? DealerInfoId { get; set; }

        public HttpPostedFileBase File { get; set; }

    }

    public class OnboardingDocumentForDelete
    {
        public int DocumentId { get; set; }

        public int? DealerInfoId { get; set; }
    }

    public class DocumentForUpload
    {
        public int? Id { get; set; }
        public int ContractId { get; set; }
        //[CustomRequired]
        public string DocumentName { get; set; }

        public int DocumentTypeId { get; set; }

        [Required(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "PleaseSelectDocumentFile")]
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