using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IAspireService
    {
        /// <summary>
        /// Prepare request and call Aspire CustomerUpload
        /// </summary>
        /// <param name="contractId">Id of contract</param>
        /// <param name="contractOwnerId">dealer Id</param>
        /// <returns></returns>
        Task<IList<Alert>> UpdateContractCustomer(int contractId, string contractOwnerId);

        Task<Tuple<CreditCheckDTO, IList<Alert>>> InitiateCreditCheck(int contractId, string contractOwnerId);

        Task<IList<Alert>> LoginUser(string userName, string password);

        /// <summary>
        /// Submit deal with equipment and payment information
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="contractOwnerId"></param>
        /// <returns></returns>
        Task<IList<Alert>> SubmitDeal(int contractId, string contractOwnerId);
    }
}
