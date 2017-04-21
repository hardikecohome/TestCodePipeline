using System.Data;

namespace DealnetPortal.Aspire.Integration.Services
{
    public interface IDatabaseService
    {
        IDataReader ExecuteReader(string query);
    }
}
