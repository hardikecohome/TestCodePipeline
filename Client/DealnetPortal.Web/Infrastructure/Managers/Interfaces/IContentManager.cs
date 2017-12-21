namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IContentManager
    {
        string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false);
    }
}