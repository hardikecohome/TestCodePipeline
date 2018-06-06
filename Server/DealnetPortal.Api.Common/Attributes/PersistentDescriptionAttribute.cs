using System.ComponentModel;

namespace DealnetPortal.Api.Common.Attributes
{
    public class PersistentDescriptionAttribute : DescriptionAttribute
    {
        public PersistentDescriptionAttribute(string description): base(description) {}
    }
}
