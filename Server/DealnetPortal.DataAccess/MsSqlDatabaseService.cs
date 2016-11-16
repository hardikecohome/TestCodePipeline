using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.DataAccess
{
    public class MsSqlDatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public MsSqlDatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDataReader ExecuteReader(string query)
        {
            var connection = GetConnection();
            var command = new SqlCommand(query, connection);
            return command.ExecuteReader();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
