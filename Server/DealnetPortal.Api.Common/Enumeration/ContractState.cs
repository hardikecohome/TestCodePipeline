using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration
{
    /// <summary>
    /// A current state of contract
    /// </summary>
    public enum ContractState
    {
        [Display(Name = "Started to fill new contract")]
        [Description("Started to fill new contract")]
        Started = 0,
        [Display(Name = "Client data inputted")]
        [Description("Client data inputted")]
        CustomerInfoInputted = 1,
        [Display(Name = "Credit check initiated")]
        [Description("Credit check initiated")]
        CreditCheckInitiated = 2,
        [Display(Name = "Credit check declined")]
        [Description("Credit check declined")]
        CreditCheckDeclined = 3,
        [Display(Name = "Credit check approved")]
        [Description("Credit check approved")]
        CreditContirmed = 4,
        [Display(Name = "Application submitted")]
        [Description("Application submitted")]
        Completed = 5,
        [Display(Name = "Sent to audit")]
        [Description("Sent to audit")]
        SentToAudit = 6
    }
}
