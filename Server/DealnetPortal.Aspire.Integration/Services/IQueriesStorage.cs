using System.Collections.Generic;

namespace DealnetPortal.Aspire.Integration.Services
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
