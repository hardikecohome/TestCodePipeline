using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess
{
    /// <summary>
    /// Get sql query by name
    /// </summary>
    public interface IQueriesStorage
    {
        Dictionary<string, string> Queries { get; }
        string GetQuery(string queryName);
    }
}
