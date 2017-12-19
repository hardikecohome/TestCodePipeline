using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Infrastructure.Attributes
{
    public class MinListCountAttribute : ValidationAttribute
    {
        private readonly int _min;
        public MinListCountAttribute(int min)
        {
            _min = min;
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list != null)
            {
                return list.Count >= _min;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }
}