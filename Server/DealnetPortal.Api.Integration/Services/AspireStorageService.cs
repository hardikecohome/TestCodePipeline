using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.Aspire.Integration.Models.AspireDb;
using DealnetPortal.Utilities.DataAccess;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireStorageService : IAspireStorageService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IQueriesStorage _queriesStorage;
        private readonly ILoggingService _loggingService;        
        public AspireStorageService(IDatabaseService databaseService, IQueriesStorage queriesStorage, ILoggingService loggingService)
        {
            _databaseService = databaseService;
            _queriesStorage = queriesStorage;
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

        public IList<GenericSubDealer> GetSubDealersList(string dealerUserName)
        {           
            string sqlStatement = _queriesStorage.GetQuery("GetSubDealers");
            if (!string.IsNullOrEmpty(sqlStatement))
            {
                sqlStatement = string.Format(sqlStatement, dealerUserName);
                var list = GetListFromQuery(sqlStatement, _databaseService, ReadSubDealerItem);
                return list;
            }
            else
            {
                _loggingService.LogWarning("Cannot get GetSubDealers query for request");
            }
            return new List<GenericSubDealer>();
        }

        public IList<ContractDTO> GetDealerDeals(string dealerUserName)
        {
            string sqlStatement = _queriesStorage.GetQuery("GetDealerDeals");
            if (!string.IsNullOrEmpty(sqlStatement))
            {
                sqlStatement = string.Format(sqlStatement, dealerUserName);
                var list = GetListFromQuery(sqlStatement, _databaseService, ReadSampleDealItem);
                return list;
            }
            else
            {
                _loggingService.LogWarning("Cannot get GetDealerDeals query for request");
            }
            return new List<ContractDTO>();
        }

        public DealerDTO GetDealerInfo(string dealerUserName)
        {
            string sqlStatement = _queriesStorage.GetQuery("GetDealerInfoByUserId");

            if (!string.IsNullOrEmpty(sqlStatement))
            {
                sqlStatement = string.Format(sqlStatement, dealerUserName);
                var list = GetListFromQuery(sqlStatement, _databaseService, ReadDealerInfoItem);
                if (list?.Any() ?? false)
                {
                    return list[0];
                }
            }
            else
            {
                _loggingService.LogWarning("Cannot get GetDealerDeals query for request");
            }
            return null;            
        }

        public ContractDTO GetDealById(int transactionId)
        {
            string sqlStatement = @"SELECT transaction#, Deal_Status, Contract_Type_Code, Last_Update_Date, Last_Update_Time,
		                                Term, [Amount Financed], Equipment_Description, Equipment_Type, Customer_name, [Customer ID]
                                      FROM sample_mydeals (NOLOCK)                                    
									  where transaction# = {0};";
            sqlStatement = string.Format(sqlStatement, transactionId);
            var list = GetListFromQuery(sqlStatement, _databaseService, ReadSampleDealItem);
            if (list?.Any() ?? false)
            {
                if (!string.IsNullOrEmpty(list[0].PrimaryCustomer?.AccountId))
                {
                    try
                    {                    
                        var customer = GetCustomerById(list[0].PrimaryCustomer?.AccountId);
                        if (customer != null)
                        {
                            list[0].PrimaryCustomer = customer;
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Cannot get customer {list[0].PrimaryCustomer?.AccountId} from Aspire", ex);
                    }
                }
                return list[0];
            }
            return null;
        }

        public CustomerDTO GetCustomerById(string customerId)
        {           
            string sqlStatement = _queriesStorage.GetQuery("GetCustomerById");
            if (!string.IsNullOrEmpty(sqlStatement))
            {

                sqlStatement = string.Format(sqlStatement, customerId);
                var list = GetListFromQuery(sqlStatement, _databaseService, ReadCustomerItem);
                if (list?.Any() ?? false)
                {
                    return list[0];
                }
            }
            else
            {
                _loggingService.LogWarning("Cannot get GetCustomerById query for request");
            }
            return null;
        }

        public CustomerDTO FindCustomer(CustomerDTO customer)
        {
            var dob = customer?.DateOfBirth ?? new DateTime();
            var postalCode =
                customer?.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.PostalCode ??
                customer?.Locations?.FirstOrDefault()?.PostalCode;
            return FindCustomer(customer?.FirstName, customer?.LastName, dob, postalCode?.Replace(" ", ""));           
        }

        public CustomerDTO FindCustomer(string firstName, string lastName, DateTime dateOfBirth, string postalCode)
        {            
            string sqlStatement = _queriesStorage.GetQuery("FindCustomer");
            if (!string.IsNullOrEmpty(sqlStatement))
            {
                var dob = dateOfBirth.ToString(CultureInfo.InvariantCulture);
                sqlStatement = string.Format(sqlStatement, firstName, lastName, dob, postalCode.Replace(" ", ""));
                var list = GetListFromQuery(sqlStatement, _databaseService, ReadCustomerItem);
                if (list?.Any() ?? false)
                {
                    return list[0];
                }                
            }
            else
            {
                _loggingService.LogWarning("Cannot get FindCustomer query for request");
            }
            return null;
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
                    AgreementType = ConvertFromDbVal<string>(dr["Contract_Type_Code"]) == "RENTAL" ? AgreementType.RentalApplication : AgreementType.LoanApplication
                };                

                item.Equipment = new EquipmentInfoDTO()
                {
                    Id = 0,
                    LoanTerm = ConvertFromDbVal<int>(dr["Term"]),                
                    RequestedTerm = ConvertFromDbVal<int>(dr["Term"]),
                    ValueOfDeal = (double)ConvertFromDbVal<decimal>(dr["Amount Financed"]),
                    AgreementType = ConvertFromDbVal<string>(dr["Contract_Type_Code"]) == "RENTAL" ? AgreementType.RentalApplication : AgreementType.LoanApplication,

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
                    FirstName = fstName,                    
                };

                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private CustomerDTO ReadCustomerItem(IDataReader dr)
        {
            try
            {
                CustomerDTO customer = new CustomerDTO()
                {
                    FirstName = ConvertFromDbVal<string>(dr["fname"]),
                    LastName = ConvertFromDbVal<string>(dr["lname"]),
                    AccountId = ConvertFromDbVal<string>(dr["entt_id"]),
                };

                customer.DateOfBirth = ConvertFromDbVal<DateTime>(dr["date_of_birth"]);
                var email = ConvertFromDbVal<string>(dr["email_addr"]);
                if (!string.IsNullOrEmpty(email))
                {
                    customer.Emails = new List<EmailDTO>()
                    {
                        new EmailDTO()
                        {
                            EmailType = EmailType.Main,
                            EmailAddress = email
                        }
                    };
                }

                var postalCode = ConvertFromDbVal<string>(dr["postal_code"]);
                if (!string.IsNullOrEmpty(postalCode))
                {
                    customer.Locations = new List<LocationDTO>()
                    {
                        new LocationDTO()
                        {
                            AddressType = AddressType.MainAddress,
                            City = ConvertFromDbVal<string>(dr["city"]),
                            State = ConvertFromDbVal<string>(dr["state"]),
                            PostalCode = ConvertFromDbVal<string>(dr["postal_code"]),
                            ResidenceType = ResidenceType.Own,
                            Street = ConvertFromDbVal<string>(dr["addr_line1"])
                        }
                    };
                }


                var phoneNum = ConvertFromDbVal<string>(dr["phone_num"]);
                if (!string.IsNullOrEmpty(phoneNum))
                {
                    customer.Phones = new List<PhoneDTO>()
                    {
                        //var isPrimary = ConvertFromDbVal<string>(dr["addr_line1"]);
                        new PhoneDTO()
                        {
                            PhoneNum = phoneNum,
                            PhoneType = PhoneType.Home
                        }
                    };
                }

                return customer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DealerDTO ReadDealerInfoItem(IDataReader dr)
        {            
            var dealerCustomerInfo = ReadCustomerItem(dr);
            DealerDTO dealerInfo = null;

            if (dealerCustomerInfo != null)
            {
                dealerInfo = new DealerDTO()
                {
                    Id = dealerCustomerInfo.Id,
                    Emails = dealerCustomerInfo.Emails,
                    Phones = dealerCustomerInfo.Phones,
                    Locations = dealerCustomerInfo.Locations,
                    AccountId = dealerCustomerInfo.AccountId,
                    DateOfBirth = dealerCustomerInfo.DateOfBirth,
                    FirstName = dealerCustomerInfo.FirstName,
                    LastName = dealerCustomerInfo.LastName                    
                };

                try
                {
                    var name = ConvertFromDbVal<string>(dr["name"]);
                    if (!string.IsNullOrEmpty(name))
                    {
                        dealerInfo.FirstName = name;
                        dealerInfo.LastName = string.Empty;
                    }

                    var pname = ConvertFromDbVal<string>(dr["parent_uname"]);
                    if (!string.IsNullOrEmpty(pname))
                    {
                        dealerInfo.ParentDealerUserName = pname;
                    }
                }
                catch (Exception ex)
                {       
                    _loggingService.LogError("Cannot read dealer info record from Aspire DB", ex);             
                }
            }

            return dealerInfo;
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
