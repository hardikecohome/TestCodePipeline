using System;

namespace DealnetPortal.Api.Models.Contract
{
    public class DocumentDTO
    {
        public string DocumentName { get; set; }

        public byte[] DocumentBytes { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
