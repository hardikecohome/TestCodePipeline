using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration.Dealer
{
    public enum ProgramServices
    {
        [Display(ResourceType =typeof(Resources.Resources), Name = "Financing")]
        Financing,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Leasing")]
        Leasing,
        [Display(ResourceType = typeof(Resources.Resources), Name = "FinancingLeasing")]
        FinancingLeasing
    }
}
