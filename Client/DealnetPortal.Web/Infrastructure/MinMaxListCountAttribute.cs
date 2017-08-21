using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Infrastructure
{
    public class MinMaxListCountAttribute : ValidationAttribute
    {
        private readonly int _max;
        private readonly int _min;

        public MinMaxListCountAttribute(int max, int min)
        {
            _max = max;
            _min = min;
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list != null)
            {
                return list.Count >= _min && list.Count <= _max; 
            }
            return false;
        }
    }
}