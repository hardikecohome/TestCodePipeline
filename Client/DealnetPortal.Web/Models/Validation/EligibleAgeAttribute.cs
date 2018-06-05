using System;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Validation
{
    public sealed class EligibleAgeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if(!(value is DateTime))
            { return false; }
            var date = (DateTime)value;
            var todayDate = DateTime.Today;
            //In case of 29 February we make it 1 March (because date with 29 February minus 18 years equals date with 28 February)
            return date <= (todayDate.Day == 29 ? todayDate.AddDays(1).AddYears(-18) : todayDate.AddYears(-18));
        }
    }
}