using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire.AspireDb;
using DealnetPortal.DataAccess;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireStorageService : IAspireStorageService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILoggingService _loggingService;        
        public AspireStorageService(IDatabaseService databaseService, ILoggingService loggingService)
        {
            _databaseService = databaseService;
            _loggingService = loggingService;
        }

        public IList<DropDownItem> GetGenericFieldValues()
        {
            string sqlStatement =
                @"SELECT a.ref_type,a.ShowInAllRoles,a.field_size,a.UseInWorkQueues,a.OID,A.SEQ_NUM,A.DESCR AS UDF_COLUMN_NAME,
                                    B.inactive_date,
                                    B.descr as Field_Description,
                                    B.active,
                                    B.OID as Field_Oid,
                                    B.valid_value as SubmissionValue
                                      FROM GenericField (NOLOCK)A
                                      LEFT JOIN GenericFieldValidValue(NOLOCK) B ON A.oid = B.genf_oid
                                      and b.active = 1 and a.active = 1                                      
                                      ORDER BY 3, 5";

            var list = GetListFromQuery(sqlStatement, _databaseService, ReadDropDownItem);
            return list;
        }

        public IList<DropDownItem> GetSubDealersList(string dealerName)
        {
            throw new NotImplementedException();
        }

        private List<T> GetListFromQuery<T>(string query, IDatabaseService ds, Func<IDataReader, T> func)
        {
            //Define list
            var resultList = new List<T>();

            //Execute query and populate data 
            using (IDataReader reader = ds.ExecuteReader(query))
            {                
                while (reader.Read())
                {
                    resultList.Add(func(reader));
                }
            }

            return resultList;
        }

        private DropDownItem ReadDropDownItem(IDataReader dr)
        {
            try
            {            
                var row = new DropDownItem();

                row.Oid = (int)(long) dr["OID"];
                row.Name = (string) dr["UDF_COLUMN_NAME"];
                row.Description = ConvertFromDbVal<string>(dr["Field_Description"]);
                row.FieldNum = ConvertFromDbVal<long>(dr["Field_Oid"]);
                row.RefType = ConvertFromDbVal<string>(dr["ref_type"]);
                row.SeqNum = ConvertFromDbVal<int>(dr["SEQ_NUM"]);
                row.SubmissionValue = ConvertFromDbVal<string>(dr["SubmissionValue"]);

                return row;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static T ConvertFromDbVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }
    }
}
