using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess
{
    public interface IDatabaseService
    {
        IDataReader ExecuteReader(string query);
    }
}
