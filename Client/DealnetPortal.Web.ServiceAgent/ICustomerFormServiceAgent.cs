using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface ICustomerFormServiceAgent
    {
        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormDTO customerForm);

        Task<CustomerContractInfoDTO> GetCustomerContractInfo(int contractId, string dealerName);
    }
}
