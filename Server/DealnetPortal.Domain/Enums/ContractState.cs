using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain.Enums
{
    /// <summary>
    /// A current state of contract
    /// </summary>
    public enum ContractState
    {
        [Description("Started to fill new contract")]
        Started = 0,
        [Description("Contract in progress, some data have been filled")]
        InProgress = 1,
        [Description("Contract filled and sent to Aspire")]
        Completed = 2
    }
}
