using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class DriverLicenseViewModel
    {
        public bool IsRecognized => DriverLicense != null;
        public RecognizedLicense DriverLicense { get; set; }
        public IList<string> RecognitionErrors { get; set; }
        
    }
}
