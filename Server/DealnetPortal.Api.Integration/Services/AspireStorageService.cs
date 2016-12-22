using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
                                      FROM GenericField (NOLOCK) A
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
            string sqlStatement = @"SELECT               
                ACC.NAME as dealer_name, 
                Contract.ContractOid AS transaction#,    
                Contract.ContractId                       as Contract_id, 
                upper(ISNULL(ENTTY.name,''))   as Customer_name,
                ISNULL(STAT.descr,'')      as [Deal_Status],
                CONVERT(varchar,Contract.LastChangeDateTime,101)     as [Last_Update_Date],
                CONVERT(varchar,Contract.LastChangeDateTime,108)     as [Last_Update_Time],
 
                EQUIP.Description AS Equipment_Description,
                ISNULL(EQPTYPE.descr,'')as [Equipment_Type],
 
                CAST(ISNULL((select dbo.[GetContractAmountFinancedFN](contract.ContractOid)),0) AS numeric(11,2)) as [Amount Financed],
 
                ISNULL(CTYPE.data_value,'') AS [Contract_Type_Code],
                ISNULL(ENTTY.entt_id,'') AS [Customer ID],           
                
                               
                                                             ISNULL(ContractTerm.Term,0)     as [Term]
                                            
                  FROM Contract  (NOLOCK)
                                              LEFT JOIN ContractTerm (NOLOCK)
                                                             ON ContractTerm.ContractOid = Contract.ContractOid
                                                             AND ContractTerm.IsPrimary = 1
                                              LEFT JOIN Product (NOLOCK)
                                                             ON ContractTerm.ProductOid = Product.Oid
                                              LEFT OUTER JOIN Entity (NOLOCK) ENTTY
                                                             ON ENTTY.oid = Contract.EntityOid
                                              LEFT OUTER JOIN LTIValues (NOLOCK) CTYPE
                                                             ON CTYPE.oid = Contract.ContractTypeOid
                                              LEFT OUTER JOIN LTIValues (NOLOCK) FPRGM
                                                             ON FPRGM.oid = Contract.ProgramOid
                                              LEFT OUTER JOIN LTIValues (NOLOCK) POTYPE
                                                             ON POTYPE.oid = Contract.PurchaseOptionTypeOid
                                              LEFT OUTER JOIN Status (NOLOCK) STAT 
                                                             ON Contract.StatusOid = STAT.oid
                                              LEFT OUTER JOIN LPAcctDistCodeVW (NOLOCK) ADC
                                                             ON Contract.AccountDistributionCodeOid = ADC.oid
                                              LEFT OUTER JOIN dbo.LPTranCodeVW  (NOLOCK) TRNCD
                                                             ON Contract.TransactionCodeOid = TRNCD.oid 
                                              LEFT OUTER JOIN LPInvCodeVW (NOLOCK) INVCD 
                                                             ON INVCD.oid = Contract.InvoiceCodeOid
                                              LEFT OUTER JOIN lPDelinqCodeVW (NOLOCK) DELCD
                                                             ON DELCD.oid = Contract.DelinquencyCodeOid
                                              LEFT JOIN LTIValues (NOLOCK) SAL
                                                             ON ENTTY.Salutation = SAL.data_value
                                                             AND SAL.table_key = 'SALUTATION'
                                              LEFT JOIN LTIValues (NOLOCK) LtiSuffix
                                                             ON ENTTY.Suffix = LtiSuffix.data_value
                                                             AND LtiSuffix.table_key = 'SUFFIX'
                                              LEFT JOIN ChildEntity (NOLOCK) CHILD
                                                             ON CHILD.ref_oid = Contract.ContractOid 
                                                             AND CHILD.ref_type = 'CNTRCT'
                                                             AND CHILD.role_type = 'FNCOMP'
                                                             AND CHILD.is_primary <> 0
                                              LEFT JOIN Entity (NOLOCK) FNPGM
                                                             ON FNPGM.oid = CHILD.entt_oid
                                              LEFT OUTER JOIN 
                                                             (SELECT SHSUB.ref_oid, 
                                                                                           MIN(SHSUB.current_status_effective_date) AS [Date Quoted]
                                                             FROM StatusHistory (NOLOCK) SHSUB
                                                                            INNER JOIN Contract (NOLOCK) CNTRSUB
                                                                                           ON CNTRSUB.ContractOid = SHSUB.ref_oid
                                                                                           AND status_type = 'CONTRACT'
                                                             GROUP BY SHSUB.ref_oid            ) AS STATHIST
                                                             ON Contract.ContractOid= STATHIST.ref_oid        
                                              Left Join contract (NOLOCK) ParentContract on ParentContract.contractOid = contract.PurposeOfFinanceParentOid
                                              LEFT JOIN CreditDecision (NOLOCK)  ON 
                                                             Contract.CreditDecisionOid = CreditDecision.CreditDecisionOid
                                              LEFT OUTER JOIN ApplicationSetting (NOLOCK)
                                                  ON ApplicationSetting.appl_name = 'Auto Sequence' AND ApplicationSetting.setting = 'Contract ID Separator'          
                                              LEFT OUTER JOIN ContractPurposeOfFinance (NOLOCK) PrimaryContractPurposeOfFinance
                                                             ON Contract.ContractOid = PrimaryContractPurposeOfFinance.ContractOid AND PrimaryContractPurposeOfFinance.IsPrimary = 1
                                              LEFT JOIN LTIValues (NOLOCK) PrimaryPurposeOfFinance
                                                             ON PrimaryPurposeOfFinance.oid = PrimaryContractPurposeOfFinance.PurposeOfFinanceTypeOid
                                              LEFT OUTER JOIN ContractPurposeOfFinance (NOLOCK) AllContractPurposeOfFinance
                                                             ON Contract.ContractOid = AllContractPurposeOfFinance.ContractOid
                                              LEFT JOIN LTIValues (NOLOCK) AllPurposeOfFinance
                                                             ON AllPurposeOfFinance.oid = AllContractPurposeOfFinance.PurposeOfFinanceTypeOid
                    LEFT JOIN dbo.DocGenConContractTaxRatesAndAmounts (NOLOCK) DGTaxRates
                      ON DGTaxRates.ContractOid = Contract.ContractOid
 
                               LEFT JOIN ChildEntity (NOLOCK) CHILD1
                ON CHILD1.ref_oid = Contract.ContractOid 
                INNER JOIN ROLE (NOLOCK) RL ON CHILD1.role_oid = RL.oid  AND CHILD1.role_type = RL.role_type  
 
                inner JOIN DocGenAccAccount (NOLOCK) ACC
                                                                                                                                                                      ON ACC.oid = CHILD1.entt_oid
                                                                                                                        
                LEFT JOIN ContractEquipment (NOLOCK) CONEQ
                                                             ON Contract.ContractOid = CONEQ.ContractOid
                LEFT JOIN Equipment (NOLOCK) EQUIP
                                                             ON EQUIP.EquipmentOid = CONEQ.EquipmentOid
 
                                                                            LEFT OUTER JOIN EquipmentType EQPTYPE
                                                             ON EQPTYPE.oid = EQUIP.EquipmentTypeOid

 
                 where acc.[account id] in (SELECT  
 
                      en.entt_id
                  FROM SecurityUser (nolock) sc
                  inner join entity (nolock) en on sc.oid = en.secu_oid
                 where sc.user_id  LIKE '{0}%')
                 order by 2 desc;";
            sqlStatement = string.Format(sqlStatement, dealerName);

            var list = GetListFromQuery(sqlStatement, _databaseService, ReadSampleDealItem);
            return list;
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
            string sqlStatement = @"SELECT
                                        * FROM Entity (nolock) as e
                                        LEFT JOIN Location (nolock) as l
                                            on (e.oid = l.entt_oid and e.loca_oid = l.oid)
                                        LEFT JOIN Phone (nolock) as p
                                            on (e.oid = p.entt_oid)
                                        where e.entt_id LIKE '{0}%'
                                        and e.fname is not null
                                        and e.lname is not null;";

            sqlStatement = string.Format(sqlStatement, customerId);
            var list = GetListFromQuery(sqlStatement, _databaseService, ReadCustomerItem);
            if (list?.Any() ?? false)
            {
                return list[0];
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
            string sqlStatement = @"SELECT e.entt_id, e.fname, e.lname, e.date_of_birth, e.email_addr, 
		                                    l.postal_code, l.city, l.state, l.postal_code, l.addr_line1,
		                                    p.phone_num
                                    FROM Entity (nolock) as e
                                    LEFT JOIN Location (nolock) as l
                                        on (e.oid = l.entt_oid and e.loca_oid = l.oid)
                                    LEFT JOIN Phone (nolock) as p
                                        on (e.oid = p.entt_oid)
                                    where   e.fname LIKE '{0}%'
                                        and e.lname LIKE '{1}%'
                                        and e.date_of_birth = '{2}'
                                        and l.postal_code LIKE '{3}%';";
            var dob = dateOfBirth.ToString(CultureInfo.InvariantCulture);            
            sqlStatement = string.Format(sqlStatement, firstName, lastName, dob, postalCode.Replace(" ", ""));
            var list = GetListFromQuery(sqlStatement, _databaseService, ReadCustomerItem);
            if (list?.Any() ?? false)
            {
                return list[0];
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
