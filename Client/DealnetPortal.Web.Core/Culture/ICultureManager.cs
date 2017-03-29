using System.Threading.Tasks;

namespace DealnetPortal.Web.Core.Culture
{
    public interface ICultureManager
    {
        void EnsureCorrectCulture(string cultureFromRoute = null);
        void SetCulture(string culture);
        Task ChangeCulture(string cultureNumber);
    }
}