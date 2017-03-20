using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Infrastructure
{
    public class CustomRequiredAttribute : RequiredAttribute
    {
        public CustomRequiredAttribute()
        {
            ErrorMessageResourceType = typeof (Resources.Resources);
            ErrorMessageResourceName = "ThisFieldIsRequired";
        }
    }
}
