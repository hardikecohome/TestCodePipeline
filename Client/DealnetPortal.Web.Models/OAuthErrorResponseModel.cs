using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class OAuthErrorResponseModel
    {
        public string Error { get; set; }
        public string Error_Description { get; set; }
        public string Error_Uri { get; set; }
    }

}
