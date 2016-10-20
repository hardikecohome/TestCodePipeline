using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services.Signature
{

    public enum FieldType
    {
        Text = 0,
        CheckBox = 1
    };
    public class FormField
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public FieldType FieldType { get; set; }
    }
}
