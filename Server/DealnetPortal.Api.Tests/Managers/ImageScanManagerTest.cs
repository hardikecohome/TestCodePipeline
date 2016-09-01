using System;
using System.IO;
using System.Linq;
using DealnetPortal.Api.Integration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models.Scanning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Managers
{
    [TestClass]
    public class ImageScanManagerTest
    {

        [TestMethod]
        public void TestReadDriverLicense()
        {
            var imgRaw = File.ReadAllBytes("Img//Barcode-Driver_License.CA.jpg");
            ScanningRequest scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = imgRaw
            };

            var imageManager = new ImageScanService();
            var result = imageManager.ReadDriverLicense(scanningRequest);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.IsFalse(result.Item2.Any());
            Assert.IsTrue(result.Item1.FirstName.Contains("First"));
            Assert.IsTrue(result.Item1.LastName.Contains("Last"));
        }
    }
}
