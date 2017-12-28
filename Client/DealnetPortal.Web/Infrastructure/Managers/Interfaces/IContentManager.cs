using System.Collections.Generic;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IContentManager
    {
        string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false);
        Dictionary<string, string> GetResourceFilesByCulture(string culture, bool quebecDealer = false);
    }
}