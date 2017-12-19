using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Infrastructure.Attributes
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
