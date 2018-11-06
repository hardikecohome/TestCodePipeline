using System.Collections.Generic;
using System.Security.Claims;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IContentManager
    {
        string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false);
        Dictionary<string, string> GetResourceFilesByCulture(string culture, ClaimsIdentity userIdentity);
    }
}