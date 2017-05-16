using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Profile;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IDealerService
    {
        DealerProfileDTO GetDealerProfile(string dealerId);
    }
}
