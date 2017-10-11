using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.ESignature
{
    [TestClass]
    public class DocuSignTest
    {
        [TestMethod]
        public void AnalyzeNotificationResponse()
        {
            var path = "Files/DocuSignNotification.xml";
            XDocument xDocument = XDocument.Load(path);            
            Assert.IsNotNull(xDocument);
            var xmlns = xDocument?.Root?.Attribute(XName.Get("xmlns"))?.Value ?? "http://www.docusign.net/API/3.0";
            var envelopeStatus = xDocument.Root.Element(XName.Get("EnvelopeStatus", xmlns)); //xDocument.Root.Elements().FirstOrDefault(x => x.Name.LocalName == "EnvelopeStatus");
            var status = envelopeStatus.Element(XName.Get("Status", xmlns));
            Assert.AreEqual(status.Value, "Completed");

            var documents =
                xDocument.Root.Element(XName.Get("DocumentPDFs", xmlns));  
            var document = documents.Elements().FirstOrDefault(x => !(x.FirstNode as XElement).Value.Contains("CertificateOfCompletion"));
            Assert.IsNotNull(document);
            var docType = document.Elements().FirstOrDefault(x => x.Name.LocalName == "DocumentType");
            Assert.AreEqual(docType.Value, "CONTENT");
        }
    }
}
