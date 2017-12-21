using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class ContentManager : IContentManager
    {
        private static string RootDirectory = "Content";
        private static string BannerPath = @"files\banners";
        private static string QuebecPrefic = "qc";
        private static string MobilePostfix = "-mobile";
        private readonly string _bannersFolder = $@"{RootDirectory}\{BannerPath}";
        private string _contentFolderFullPath;

        public string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false)
        {
            _contentFolderFullPath = HostingEnvironment.MapPath($@"~\{_bannersFolder}");

            return !Directory.Exists(_contentFolderFullPath) ? string.Empty : GetRelativePathToFile(culture, quebecDealer, isMobile);
        }

        private string GetRelativePathToFile(string culture, bool quebecDealer, bool isModile)
        {
            var qubecPrefix = quebecDealer ? QuebecPrefic : string.Empty;
            var bannerName = ConfigurationManager.AppSettings["HomeBannerName"];
            var extension = Path.GetExtension(bannerName);
            var bannerWithoutExtension = Path.GetFileNameWithoutExtension(bannerName);
            var fullBannerName = isModile ? $"{bannerWithoutExtension}{MobilePostfix}{extension}" : bannerName;

            var fullPath = Path.Combine(_contentFolderFullPath, culture, qubecPrefix, fullBannerName);

            return File.Exists(fullPath) ? Path.Combine("~", _bannersFolder, culture, qubecPrefix, fullBannerName) : string.Empty;
        }

    }
}