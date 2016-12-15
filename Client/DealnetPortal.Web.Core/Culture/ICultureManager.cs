using System.Threading.Tasks;

namespace DealnetPortal.Web.Core.Culture
{
    public interface ICultureManager
    {
        void EnsureCorrectCulture();
        void SetCulture(string culture);
        Task ChangeCulture(int cultureNumber);
    }
}