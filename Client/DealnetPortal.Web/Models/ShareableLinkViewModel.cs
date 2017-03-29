using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class ShareableLinkViewModel
    {
        public bool EnglishLinkEnabled { get; set; }
        public bool FrenchLinkEnabled { get; set; }
        public List<string> EnglishServices { get; set; } 
        public List<string> FrenchServices { get; set; } 
    }
}
