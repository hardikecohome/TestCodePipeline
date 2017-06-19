using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class RecognizedLicense
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Sex { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class DriverLicenseViewModel
    {
        public bool IsRecognized => DriverLicense != null;
        public RecognizedLicense DriverLicense { get; set; }
        public IList<string> RecognitionErrors { get; set; }
        
    }
}
