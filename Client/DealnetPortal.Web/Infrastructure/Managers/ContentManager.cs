using System;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Models;
using WebGrease.Css.Extensions;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class ContentManager : IContentManager
    {
        private static string BannerFolderName = @"banners";
        private static string ResourcesFolderName = @"resources";
        private static string ContentPath = @"Content\files";
        private static string QuebecPrefic = "qc";
        private static string MobilePostfix = "-mobile";
        private static string _rootPath = HostingEnvironment.MapPath(@"~\");
        private static string _contentFolderFullPath = Path.Combine(_rootPath, ContentPath);
        private static string MortgageBrokerPrefic = "MortgageBroker";
	    private static string GeneralFiles = "general";
	    private static string NoDirectoryFlag = "NO_DIR";

	    private Func<string, ResourceListModel> _resourceSelector = file => new ResourceListModel
	    {
		    FileName = FormatFileName(file),
		    FilePath = GetRelativePathToFile(file)
	    };

        public string GetMaitanenceBannerByCulture(string culture, bool quebecDealer = false)
        {
            return string.Empty;
        }

        public string GetBannerByCulture(string culture, bool quebecDealer = false, bool isMobile = false)
        {
            var bannerPath = Path.Combine(_contentFolderFullPath, BannerFolderName);

            return !Directory.Exists(bannerPath) ? string.Empty : GetRelativeBannerPath(culture, quebecDealer, isMobile);
        }

        public Dictionary<string, List<ResourceListModel>> GetResourceFilesByCulture(string culture, ClaimsIdentity userIdentity)
        {
            //identity.HasClaim("QuebecDealer", "True")
            //var resourcesFullPathWithCulture = Path.Combine(_contentFolderFullPath, ResourcesFolderName, culture, quebecDealer ? QuebecPrefic : string.Empty);
            var resourcesFullPathWithCulture = Path.Combine(_contentFolderFullPath, ResourcesFolderName, culture, userIdentity.HasClaim(ClaimContstants.QuebecDealer, "True") ? QuebecPrefic :
                                                                                                                   userIdentity.HasClaim("MortgageBroker", "True") ? MortgageBrokerPrefic : GeneralFiles);
            if (!Directory.Exists(resourcesFullPathWithCulture)) return null;

            return GetResourcecFilesDictionary(resourcesFullPathWithCulture);
        }

        private Dictionary<string, List<ResourceListModel>> GetResourcecFilesDictionary(string fullPath)
        {
	        if (Directory.GetDirectories(fullPath).Any())
	        {
		        return Directory.GetDirectories(fullPath).Aggregate(new Dictionary<string, List<ResourceListModel>>(), (dict, dir) =>
		        {
			        var dictName = Path.GetFileName(dir) ?? NoDirectoryFlag;
			        if (!dict.ContainsKey(dictName))
			        {
				        dict.Add(dictName, new List<ResourceListModel>());
			        }

			        dict[dictName].AddRange(Directory.GetFiles(dictName == NoDirectoryFlag ? fullPath : dir).Select(_resourceSelector));

			        return dict;
		        });
	        }
	        return new Dictionary<string, List<ResourceListModel>>
	        {
		        {
			        NoDirectoryFlag, Directory.GetFiles(fullPath).Select(_resourceSelector).ToList()
		        }
	        };
        }

        private static string FormatFileName(string fullPath)
        {
            var fileInfo = new FileInfo(fullPath);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);

            return Regex.Replace(nameWithoutExtension, @"[^\w\.@-\\%]+", " ");
        }

        private string GetRelativeBannerPath(string culture, bool quebecDealer, bool isModile)
        {
            var qubecPrefix = quebecDealer ? QuebecPrefic : string.Empty;
            var bannerName = ConfigurationManager.AppSettings["HomeBannerName"];
            var extension = Path.GetExtension(bannerName);
            var bannerWithoutExtension = Path.GetFileNameWithoutExtension(bannerName);
            var fullBannerName = isModile ? $"{bannerWithoutExtension}{MobilePostfix}{extension}" : bannerName;

            var fullPath = Path.Combine(_contentFolderFullPath, BannerFolderName, culture, qubecPrefix, fullBannerName);

            return File.Exists(fullPath) ? GetRelativePathToFile(fullPath) : string.Empty;
        }

        private static string GetRelativePathToFile(string fullPath)
        {
            var fullDirectoryInfo = new DirectoryInfo(_rootPath);
            var relativePath = fullPath.Substring(fullDirectoryInfo.FullName.Length);

            return Path.Combine("~", relativePath);
        }
    }
}