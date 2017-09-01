using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class RequiredDocumentViewModel
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public int DocumentTypeId { get; set; }
    }
}