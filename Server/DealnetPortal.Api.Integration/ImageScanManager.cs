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
    /// Read and extract data from images
    /// </summary>
    public class ImageScanManager
    {
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
                    BarcodeReader reader = new BarcodeReader()
                    {
                        Horizontal = true,
                        Vertical = true,
                        Diagonal = true,
                        DrvLicID = true,
                    };
                    Barcode[] barcodes = reader.Read(mStream);
                    aamva = barcodes.First().Decode(BarcodeDecoding.aamva);
                }
                catch (InvalidOperationException ex)
                {
                    alerts.Add(new Alert() { Type = AlertType.Error, Header = "Can't recognize license", Message = ex.ToString() });
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
                    alerts.Add(new Alert() { Type = AlertType.Error, Header = "Can't recognize license", Message = "" });
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
