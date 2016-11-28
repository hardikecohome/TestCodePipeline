using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire.AspireDb;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IAspireStorageService
    {
        IList<DropDownItem> GetGenericFieldValues();
    }
}
