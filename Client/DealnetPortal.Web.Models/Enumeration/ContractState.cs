using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum ContractState
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "StartedToFillNewContract")]
        Started = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "ClientDataInputted")]
        CustomerInfoInputted = 1,
        [Display(ResourceType = typeof (Resources.Resources), Name = "CreditCheckInitiated")]
        CreditCheckInitiated = 2,
        [Display(ResourceType = typeof (Resources.Resources), Name = "CreditCheckDeclined")]
        CreditCheckDeclined = 3,
        [Display(ResourceType = typeof (Resources.Resources), Name = "CreditCheckApproved")]
        CreditConfirmed = 4,
        [Display(ResourceType = typeof (Resources.Resources), Name = "ApplicationSubmitted")]
        Completed = 5,
        [Display(ResourceType = typeof (Resources.Resources), Name = "SentToAudit")]
        SentToAudit = 6
    }
}
