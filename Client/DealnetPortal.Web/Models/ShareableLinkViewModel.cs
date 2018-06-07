using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class ShareableLinkViewModel
    {
        public bool EnglishLinkEnabled { get; set; }
        public bool FrenchLinkEnabled { get; set; }
        public List<string> EnglishServices { get; set; } 
        public List<string> FrenchServices { get; set; } 
        public string HashDealerName { get; set; } 
    }
}
