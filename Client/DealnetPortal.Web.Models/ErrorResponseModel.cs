using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class ErrorResponseModel
    {
        public string Message { get; set; }

        public Dictionary<string, string[]> ModelState { get; set; }

        public ErrorResponseModel()
        {
            Message = string.Empty;
            ModelState = new Dictionary<string, string[]>();
        }
    }
}
