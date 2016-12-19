using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Aspire.AspireDb;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
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

        public IList<GenericSubDealer> GetSubDealersList(string dealerName)
        {
            string sqlStatement = @"SELECT a.OID,A.SEQ_NUM,A.DESCR AS UDF_COLUMN_NAME,                                    
                                    B.descr as Field_Description,
                                    B.OID as Field_Oid,
                                    B.valid_value as SubmissionValue
                                      FROM GenericField (NOLOCK)A
                                      LEFT JOIN GenericFieldValidValue(NOLOCK) B ON A.oid = B.genf_oid
                                      and b.active = 1 and a.active = 1                                      
									  where a.ref_type = 'CNTRCT' and b.active = 1
											and a.descr LIKE '%{0}%'
                                      ORDER BY 3, 5";
            sqlStatement = string.Format(sqlStatement, dealerName);

            var list = GetListFromQuery(sqlStatement, _databaseService, ReadSubDealerItem);
            return list;
        }

        public IList<ContractDTO> GetDealerDeals(string dealerName)
        {
            string sqlStatement = @"SELECT *
                                      FROM sample_mydeals (NOLOCK)                                    
									  where dealer_name LIKE '%{0}%';";
            sqlStatement = string.Format(sqlStatement, dealerName);

            var list = GetListFromQuery(sqlStatement, _databaseService, ReadSampleDealItem);
            return list;
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

        private GenericSubDealer ReadSubDealerItem(IDataReader dr)
        {
            try
            {
                var row = new GenericSubDealer();

                row.DealerId = ((long)dr["OID"]).ToString();
                row.SeqNum = ConvertFromDbVal<int>(dr["SEQ_NUM"]).ToString();
                row.DealerName = (string)dr["UDF_COLUMN_NAME"];
                row.SubDealerName = ConvertFromDbVal<string>(dr["Field_Description"]);
                row.SubDealerId = ConvertFromDbVal<long>(dr["Field_Oid"]).ToString();                
                row.SubmissionValue = ConvertFromDbVal<string>(dr["SubmissionValue"]);                
                return row;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private ContractDTO ReadSampleDealItem(IDataReader dr)
        {
            try
            {
                var item = new ContractDTO()
                {
                    Id = 0 // for transactions from aspire
                };

                var date = ConvertFromDbVal<string>(dr["Last_Update_Date"]);
                var time = ConvertFromDbVal<string>(dr["Last_Update_Time"]);
                DateTime updateTime;
                DateTime.TryParse($"{date} {time}", out updateTime);

                item.LastUpdateTime = item.CreationTime = updateTime;

                item.Details = new ContractDetailsDTO()
                {
                    TransactionId = ConvertFromDbVal<long>(dr["transaction#"]).ToString(),
                    Status = ConvertFromDbVal<string>(dr["Deal_Status"]),
                    AgreementType = ConvertFromDbVal<string>(dr["Contract_Type_Code"]) == "RENTAL" ? AgreementType.RentalApplication : AgreementType.LoanApplication                    ,                    
                };                

                item.Equipment = new EquipmentInfoDTO()
                {
                    Id = 0,
                    LoanTerm = ConvertFromDbVal<int>(dr["Term"]),                
                    RequestedTerm = ConvertFromDbVal<int>(dr["Term"]),
                    ValueOfDeal = (double)ConvertFromDbVal<decimal>(dr["Amount Financed"]),


                    NewEquipment = new List<NewEquipmentDTO>()
                    {
                        new NewEquipmentDTO()
                        {
                            Id = 0,
                            Description = ConvertFromDbVal<string>(dr["Equipment_Description"]),
                            Type = ConvertFromDbVal<string>(dr["Equipment_Type"]),
                        }
                    }
                };

                var names = ConvertFromDbVal<string>(dr["Customer_name"])?.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                var fstName = names?.Count() > 1 ? names[0] : string.Empty;
                var lstName = names?.Count() > 1 ? names[1] : string.Empty;

                item.PrimaryCustomer = new CustomerDTO()
                {
                    Id = 0,
                    AccountId = ConvertFromDbVal<string>(dr["Customer ID"]),
                    LastName = lstName,
                    FirstName = fstName
                };


               
                return item;
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
