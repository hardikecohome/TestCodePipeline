using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace DealnetPortal.DataAccess
{
    public class QueriesFileStorage : IQueriesStorage
    {
        private Dictionary<string, string> _sqlQueries;

        public QueriesFileStorage()
        {
            var folderName = ConfigurationManager.AppSettings["QueriesFolder"] ?? "Queries";
            var folder = HostingEnvironment.MapPath($"~/{folderName}") ?? folderName;
            _sqlQueries = ReadQueries(folder);
        }

        public Dictionary<string, string> Queries => _sqlQueries;        

        public string GetQuery(string queryName)
        {
            if (_sqlQueries?.ContainsKey(queryName) ?? false)
            {
                return _sqlQueries[queryName];
            }
            return string.Empty;
        }

        private Dictionary<string, string> ReadQueries(string queryFolder)
        {
            var result = new Dictionary<string, string>();

            try
            {            
                var filePaths = Directory.GetFiles(queryFolder);

                foreach (var filename in filePaths)
                {
                    if (filename.EndsWith(".sql"))
                    {
                        result[Path.GetFileNameWithoutExtension(filename)] =
                            System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(filename), @"\s+", " ")
                                .Replace("\"", "'");
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
    }
}
