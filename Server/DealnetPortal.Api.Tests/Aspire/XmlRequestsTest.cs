using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using DealnetPortal.Api.Models.Aspire;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Aspire
{
    [TestClass]
    public class XmlRequestsTest
    {
        [Ignore]
        [TestMethod]
        public void TestRequestForCustomerUpload()
        {
            DealUploadRequest request = new DealUploadRequest();

            request.Header = new Header()
            {
                From = new From()
                {
                    AccountNumber = "Admin",
                    Password = "b"
                }
            };

            request.Payload = new Payload()
            {
                Lease = new Lease()
                {   
                    Application = new Application(),
                    Accounts = new List<Account>()
                    {
                        new Account()
                        {
                            Role = "CUST",
                            EmailAddress = "custname@domain.com",
                            IsPrimary = true,
                            Personal = new Personal()
                            {
                                Firstname = "Customer",
                                Lastname = "Name",
                                Dob = DateTime.Today.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                            }
                        }
                    }
                }
            };

            var x = new XmlSerializer(request.GetType());
            var settings = new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize };
            MemoryStream ms = new MemoryStream();
            FileStream fs = new FileStream("testResponse.xml", FileMode.Create);
            var writer = XmlWriter.Create(fs, settings);
            x.Serialize(writer, request);
            writer.Flush();
        }
    }
}
