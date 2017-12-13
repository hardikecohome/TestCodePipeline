﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DealnetPortal.Aspire.Integration.Models
{
    [Serializable]
    [XmlRoot(ElementName = "DocumentUploadXML")]
    public class DocumentUploadRequest : DealUploadRequest
    {
        //public RequestHeader Header { get; set; }

        public new DocumentUploadPayload Payload { get; set; }
    }

    [Serializable]
    public class DocumentUploadPayload : Payload
    {
        public string ContractId { get; set; }
        //public string TransactionId { get; set; }
        public string Status { get; set; }

        [XmlElement("Document")]
        public List<Document> Documents { get; set; }
    }

    [Serializable]
    public class Document
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public string Ext { get; set; }
    }
}