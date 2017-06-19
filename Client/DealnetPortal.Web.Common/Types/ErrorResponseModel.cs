using System.Collections.Generic;

namespace DealnetPortal.Web.Common.Types
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
