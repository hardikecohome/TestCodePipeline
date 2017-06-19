using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Culture
{
    public interface ICultureManager
    {
        void EnsureCorrectCulture(string cultureFromRoute = null);
        void SetCulture(string culture, bool createCookie = true);
    }
}