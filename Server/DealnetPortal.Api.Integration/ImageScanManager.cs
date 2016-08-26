using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Enumeration;
using DealnetPortal.Api.Models.Scanning;
using Inlite.ClearImageNet;

namespace DealnetPortal.Api.Integration
{
    /// <summary>
    /// Read, recognize and extract data from images manager
    /// </summary>
    public class ImageScanManager
    {
        /// <summary>
        /// Recognize Driver License image
        /// </summary>
        /// <param name="scanningRequest">Scanned image request</param>
        /// <returns></returns>
        public Tuple<DriverLicenseData, IList<Alert>>  ReadDriverLicense(ScanningRequest scanningRequest)
        {
            List<Alert> alerts = new List<Alert>();
            DriverLicenseData driverLicense = new DriverLicenseData();

            if (scanningRequest?.ImageForReadRaw != null)
            {
                var mStream = new MemoryStream(scanningRequest.ImageForReadRaw);
                string aamva = string.Empty;
                try
                {
                    ImageEditor repair = new ImageEditor();
                    MemoryStream ms = new MemoryStream(scanningRequest.ImageForReadRaw);
                    repair.Image.Open(ms, 0);
                    repair.AutoDeskew();
                    repair.AutoRotate();
                    repair.CleanNoise(3);                    


                    BarcodeReader reader = new BarcodeReader()
                    {
                        Horizontal = true,
                        Vertical = true,
                        Diagonal = true,
                        DrvLicID = true,
                    };
                    Barcode[] barcodes = reader.Read(repair);
                    aamva = barcodes.First().Decode(BarcodeDecoding.aamva);
                }
                catch (Exception ex)
                {
                    alerts.Add(new Alert() { Type = AlertType.Error, Header = "Can't recognize license", Message = ex.ToString() });
                    return new Tuple<DriverLicenseData, IList<Alert>>(driverLicense, alerts);
                }
                
                if (!string.IsNullOrEmpty(aamva))
                {
                    try
                    {
                        DriverLicenseParser parser = new DriverLicenseParser(aamva);
                        driverLicense = parser.DriverLicense;
                    }
                    catch (Exception ex)
                    {
                        alerts.Add(new Alert() { Type = AlertType.Error, Header = "Can't parse recognized license", Message = ex.ToString() });
                    }
                }
                else
                {
                    alerts.Add(new Alert() { Type = AlertType.Error, Header = "Can't recognize license", Message = "Image wasn't recognized" });
                }
            }
            else
            {
                alerts.Add(new Alert() {Type = AlertType.Error, Header = "Can't recognize license", Message = "Data for recognize is empty"});
            }
            return new Tuple<DriverLicenseData, IList<Alert>>(driverLicense, alerts);
        }
    }
}
