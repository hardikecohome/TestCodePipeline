using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DealnetPortal.Api.Models.Aspire
{
    [Serializable]
    [XmlRoot(ElementName = "DocumentUploadXML")]
    public class DocumentUploadRequest
    {
        public RequestHeader Header { get; set; }

        public DocumentUploadPayload Payload { get; set; }
    }

    [Serializable]
    public class DocumentUploadPayload
    {
        public string ContractId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        [XmlElement(ElementName = "documentName")]
        public string DocumentName { get; set; }
        [XmlElement(ElementName = "documentData")]
        public string DocumentData { get; set; }
    }
}