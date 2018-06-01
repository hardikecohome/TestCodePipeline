using System.Collections.Generic;
using System.Security.Claims;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IContentManager
    {
        string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false);
	    Dictionary<string, List<ResourceListModel>> GetResourceFilesByCulture(string culture, ClaimsIdentity userIdentity);
    }
}