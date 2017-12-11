using System;
using System.Collections.Generic;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICreditCheckService
    {
        IList<Alert> InitiateCreditCheck(int contractId, string contractOwnerId);

        Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId);
    }
}
