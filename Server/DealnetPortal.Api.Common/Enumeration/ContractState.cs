using System.ComponentModel;

namespace DealnetPortal.Api.Common.Enumeration
{
    /// <summary>
    /// A current state of contract
    /// </summary>
    public enum ContractState
    {
        [Description("Started to fill new contract")]
        Started = 0,
        [Description("Client data inputted")]
        CustomerInfoInputted = 1,
        [Description("Credit check confirmed")]
        CreditContirmed = 2,
        [Description("Contract filled and sent to Aspire")]
        Completed = 3
    }
}
