using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire.AspireDb;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IAspireStorageService
    {
        IList<DropDownItem> GetGenericFieldValues();

        IList<GenericSubDealer> GetSubDealersList(string dealerName);

        IList<ContractDTO> GetDealerDeals(string dealerName);
    }
}
