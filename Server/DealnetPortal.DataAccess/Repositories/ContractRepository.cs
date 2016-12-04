using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Policy;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Repositories
{
    public class ContractRepository : BaseRepository, IContractRepository
    {
        public ContractRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public Contract CreateContract(string contractOwnerId)
        {
            Contract contract = null;
            var dealer = GetUserById(contractOwnerId);
            if (dealer != null)
            {
                contract = new Contract()
                {
                    ContractState = ContractState.Started,
                    CreationTime = DateTime.Now,
                    Dealer = dealer
                };
                _dbContext.Contracts.Add(contract);
            }
            return contract;
        }

        public IList<Contract> GetContracts(string ownerUserId)
        {
            var contracts = _dbContext.Contracts
                    .Include(c => c.PrimaryCustomer)
                    .Include(c => c.PrimaryCustomer.Locations)
                    .Include(c => c.SecondaryCustomers)
                    .Include(c => c.Equipment)
                    .Include(c => c.Equipment.ExistingEquipment)
                    .Include(c => c.Equipment.NewEquipment)
                    .Where(c => c.Dealer.Id == ownerUserId || c.Dealer.ParentDealerId == ownerUserId).ToList();
            contracts.ForEach(c => c.Documents = _dbContext.ContractDocuments.Where(cd => cd.ContractId == c.Id)
                .Select(cd => new { DocumentTypeId = cd.DocumentTypeId }).ToList().Select(x => new ContractDocument()
                {
                    DocumentTypeId = x.DocumentTypeId
                }).ToList());
            return contracts;
        }

        public IList<Contract> GetContracts(IEnumerable<int> ids, string ownerUserId)
        {
            var contracts = _dbContext.Contracts
                    .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                    .Where(c => ids.Any(id => id == c.Id) && (c.Dealer.Id == ownerUserId || c.Dealer.ParentDealerId == ownerUserId)).ToList();
            contracts.ForEach(c => c.Documents = _dbContext.ContractDocuments.Where(cd => cd.ContractId == c.Id)
                .Select(cd => new { DocumentTypeId = cd.DocumentTypeId}).ToList().Select(x => new ContractDocument()
                {
                    DocumentTypeId = x.DocumentTypeId
                }).ToList());
            return contracts;
        }

        public Contract UpdateContractState(int contractId, string contractOwnerId, ContractState newState)
        {
            var contract = GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                contract.ContractState = newState;
                contract.LastUpdateTime = DateTime.Now;
            }
            return contract;
        }

        public Contract GetContract(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)                
                .FirstOrDefault(c => c.Id == contractId && (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId));
        }

        public Contract GetContractAsUntracked(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .AsNoTracking().
                FirstOrDefault(c => c.Id == contractId && (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId));
        }

        public bool DeleteContract(string contractOwnerId, int contractId)
        {
            bool deleted = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId) && c.Id == contractId);
            if (contract != null)
            {
                //remove clients for contract?
                contract.SecondaryCustomers.Clear();

                if (contract.Equipment != null)
                {
                    var entriesForDelete = contract.Equipment.NewEquipment.ToList();
                    entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

                    var entriesForDeleteE = contract.Equipment.ExistingEquipment.ToList();
                    entriesForDeleteE.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);                    
                }                

                _dbContext.Contracts.Remove(contract);
                deleted = true;
            }
            return deleted;
        }

        public bool CleanContract(string contractOwnerId, int contractId)
        {
            bool cleaned = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId) && c.Id == contractId);
            if (contract != null)
            {
                contract.SecondaryCustomers.Clear();
                cleaned = true;
            }
            return cleaned;
        }

        public Contract UpdateContract(Contract contract, string contractOwnerId)
        {
            contract.ContractState = ContractState.CustomerInfoInputted;
            _dbContext.Entry(contract).State = EntityState.Modified;
            contract.LastUpdateTime = DateTime.Now;
            return contract;
        }

        public Contract UpdateContractData(ContractData contractData, string contractOwnerId)
        {
            if (contractData != null)
            {
                var contract = GetContract(contractData.Id, contractOwnerId);
                if (contract != null)
                {
                    if (!string.IsNullOrEmpty(contractData.DealerId))
                    {
                        if (contract.DealerId != contractData.DealerId)
                        {
                            //TODO: check availability to change dealer Id (we have ability to change t)
                            contract.DealerId = contractData.DealerId;
                            contract.LastUpdateTime = DateTime.Now;
                        }
                    }

                    if (contract.ExternalSubDealerId != null && contract.ExternalSubDealerId != contractData.ExternalSubDealerId)
                    {
                        contract.ExternalSubDealerId = contractData.ExternalSubDealerId;
                        contract.LastUpdateTime = DateTime.Now;
                    }
                    if (contract.ExternalSubDealerName != null && contract.ExternalSubDealerName != contractData.ExternalSubDealerName)
                    {
                        contract.ExternalSubDealerName = contractData.ExternalSubDealerName;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.PrimaryCustomer != null)
                    {
                        // ?
                        if (contractData.PrimaryCustomer.Id == 0 && contract.PrimaryCustomer != null)
                        {
                            contractData.PrimaryCustomer.Id = contract.PrimaryCustomer.Id;
                        }

                        var homeOwnerLocations = contractData.PrimaryCustomer.Locations;
                        var homeOwner = AddOrUpdateCustomer(contractData.PrimaryCustomer);
                        if (homeOwner != null)
                        {                            
                            contract.PrimaryCustomer = homeOwner;
                            AddOrUpdateCustomerLocations(contract.PrimaryCustomer, homeOwnerLocations);
                            contract.ContractState = ContractState.CustomerInfoInputted;
                            contract.LastUpdateTime = DateTime.Now;
                        }
                    }

                    if (contractData.SecondaryCustomers != null)
                    {
                        AddOrUpdateAdditionalApplicants(contract, contractData.SecondaryCustomers);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.Equipment != null)
                    {
                        AddOrUpdateEquipment(contract, contractData.Equipment);
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.Details != null)
                    {
                        AddOrUpdateContactDetails(contract, contractData.Details);
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.PaymentInfo != null)
                    {
                        AddOrUpdatePaymentInfo(contract, contractData.PaymentInfo);
                        contract.LastUpdateTime = DateTime.Now;
                    }                    

                    return contract;
                }
            }
            return null;
        }

        public Customer UpdateCustomer(Customer customer)
        {
            if (customer != null)
            {
                var locations = customer.Locations;
                var emails = customer.Emails;
                var phones = customer.Phones;

                //customer.Locations = null;
                //customer.Phones = null;
                //customer.Emails = null;

                var dbCustomer = AddOrUpdateCustomer(customer);
                if (dbCustomer != null)
                {
                    if (locations != null)
                    {
                        AddOrUpdateCustomerLocations(dbCustomer, locations.ToList());
                    }
                    if (phones != null)
                    {
                        AddOrUpdateCustomerPhones(dbCustomer, phones.ToList());
                    }
                    if (emails != null)
                    {
                        AddOrUpdateCustomerEmails(dbCustomer, emails.ToList());
                    }
                }
                return dbCustomer;
            }
            return null;
        }

        public Customer UpdateCustomerData(int customerId, Customer customerInfo, IList<Location> locations,
            IList<Phone> phones, IList<Email> emails)
        {
            var dbCustomer = GetCustomer(customerId);
            if (dbCustomer != null)
            {
                if (customerInfo != null)
                {
                    if (customerInfo.AllowCommunicate.HasValue)
                    {
                        dbCustomer.AllowCommunicate = customerInfo.AllowCommunicate;
                    }
                    if (customerInfo.DriverLicenseNumber != null)
                    {
                        dbCustomer.DriverLicenseNumber = customerInfo.DriverLicenseNumber;
                    }
                    if (customerInfo.Sin != null)
                    {
                        dbCustomer.Sin = customerInfo.Sin;
                    }
                    //AddOrUpdateCustomer(customerInfo);
                }

                if (locations != null)
                {
                    AddOrUpdateCustomerLocations(dbCustomer, locations.ToList());
                }
                if (phones != null)
                {
                    AddOrUpdateCustomerPhones(dbCustomer, phones.ToList());
                }
                if (emails != null)
                {
                    AddOrUpdateCustomerEmails(dbCustomer, emails.ToList());
                }
                return dbCustomer;
            }
            return null;
        }

        public Customer GetCustomer(int customerId)
        {
            return _dbContext.Customers
                .Include(c => c.Phones)
                .Include(c => c.Locations)
                .Include(c => c.Emails)
                .FirstOrDefault(c => c.Id == customerId);
        }

        public IList<EquipmentType> GetEquipmentTypes()
        {
            return _dbContext.EquipmentTypes.ToList();
        }

        public IList<DocumentType> GetDocumentTypes()
        {
            return _dbContext.DocumentTypes.ToList();
        }

        public ProvinceTaxRate GetProvinceTaxRate(string province)
        {
            return _dbContext.ProvinceTaxRates.FirstOrDefault(x => x.Province == province);
        }

        public decimal GetContractTotalMonthlyPayment(int contractId)
        {
            decimal totalMp = 0.0M;

            var contract = _dbContext.Contracts.Find(contractId);
            if (contract != null)
            {
                var rate =
                    GetProvinceTaxRate(
                        (contract.PrimaryCustomer?.Locations.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress) ??
                         contract.PrimaryCustomer?.Locations.First())?.State.ToProvinceCode());
                if (rate != null && contract.Equipment != null)
                {
                    if (contract.Equipment.AgreementType == AgreementType.LoanApplication)
                    {
                        var loanCalculatorInput = new LoanCalculator.Input
                        {
                            TaxRate = rate.Rate,
                            LoanTerm = contract.Equipment.LoanTerm ?? 0,
                            AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                            EquipmentCashPrice = (double?)contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                            AdminFee = contract.Equipment.AdminFee ?? 0,
                            DownPayment = contract.Equipment.DownPayment ?? 0,
                            CustomerRate = contract.Equipment.CustomerRate ?? 0
                        };
                        var loanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
                        totalMp = (decimal)loanCalculatorOutput.TotalMonthlyPayment;
                    }
                    else
                    {
                        totalMp = (contract.Equipment.TotalMonthlyPayment ?? 0) + (contract.Equipment.TotalMonthlyPayment ?? 0)*(decimal) (rate.Rate / 100);
                    }
                }
            }

            return totalMp;
        }

        public PaymentSummary GetContractPaymentsSummary(int contractId)
        {
            PaymentSummary paymentSummary = new PaymentSummary();

            var contract = _dbContext.Contracts.Find(contractId);
            if (contract != null)
            {
                var rate =
                    GetProvinceTaxRate(
                        (contract.PrimaryCustomer?.Locations.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress) ??
                         contract.PrimaryCustomer?.Locations.First())?.State.ToProvinceCode());
                if (rate != null)
                {
                    if (contract.Equipment.AgreementType == AgreementType.LoanApplication)
                    {
                        var loanCalculatorInput = new LoanCalculator.Input
                        {
                            TaxRate = rate.Rate,
                            LoanTerm = contract.Equipment.LoanTerm ?? 0,
                            AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                            EquipmentCashPrice = (double?) contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                            AdminFee = contract.Equipment.AdminFee ?? 0,
                            DownPayment = contract.Equipment.DownPayment ?? 0,
                            CustomerRate = contract.Equipment.CustomerRate ?? 0
                        };
                        var loanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
                        paymentSummary.Hst = (decimal) loanCalculatorOutput.Hst;
                        paymentSummary.TotalPayment = (decimal) loanCalculatorOutput.TotalAllMonthlyPayments;
                        paymentSummary.MonthlyPayment = (decimal) loanCalculatorOutput.TotalMonthlyPayment;
                        paymentSummary.TotalAllMonthlyPayment = (decimal)loanCalculatorOutput.TotalAllMonthlyPayments;
                    }
                    else
                    {
                        paymentSummary.MonthlyPayment = contract.Equipment.TotalMonthlyPayment;
                        paymentSummary.Hst = (contract.Equipment.TotalMonthlyPayment ?? 0)*(decimal) (rate.Rate/100);
                        paymentSummary.TotalPayment = (contract.Equipment.TotalMonthlyPayment ?? 0) +
                                                      (contract.Equipment.TotalMonthlyPayment ?? 0)*
                                                      (decimal) (rate.Rate/100);
                        paymentSummary.TotalAllMonthlyPayment = paymentSummary.TotalPayment * (contract.Equipment.RequestedTerm ?? 0);
                    }
                }
            }

            return paymentSummary;
        }

        public ContractDocument AddDocumentToContract(int contractId, ContractDocument document, string contractOwnerId)
        {
            var contract = _dbContext.Contracts.Include(c => c.Documents).FirstOrDefault(c => c.Id == contractId);            
            var docTypeId = document.DocumentTypeId;
            if (document.DocumentType != null)
            {
                docTypeId =
                    _dbContext.DocumentTypes.FirstOrDefault(t => t.Prefix == document.DocumentType.Prefix)?.Id ??
                    docTypeId;                
            }
            var docType = _dbContext.DocumentTypes.Find(docTypeId);
            if (!string.IsNullOrEmpty(docType?.Prefix))
            {
                if (string.IsNullOrEmpty(document.DocumentName) || !document.DocumentName.StartsWith(docType.Prefix) )
                {
                    document.DocumentName = docType.Prefix + document.DocumentName;
                }
            }

            document.CreationDate = DateTime.Now;           

            if (contract != null)
            {
                var otherType = _dbContext.DocumentTypes.FirstOrDefault(t => string.IsNullOrEmpty(t.Prefix));
                var dbDocument = contract.Documents.FirstOrDefault(d => otherType != null && otherType.Id == docTypeId ? d.DocumentTypeId == docTypeId && d.Id == document.Id : d.DocumentTypeId == docTypeId);
                if (dbDocument != null)
                {
                    //var otherType = _dbContext.DocumentTypes.FirstOrDefault(t => string.IsNullOrEmpty(t.Prefix));
                    //if (otherType != null && dbDocument.DocumentType.Id == otherType.Id && dbDocument.DocumentName != document.DocumentName)
                    //{
                    //    document.Contract = contract;
                    //    contract.Documents.Add(document);
                    //}
                    //else
                    //{
                    document.Id = dbDocument.Id;
                        document.Contract = contract;
                        _dbContext.ContractDocuments.AddOrUpdate(document);
                    //}                    
                }
                else
                {
                    document.Contract = contract;
                    contract.Documents.Add(document);
                }                
            }
            return document;
        }

        public bool TryRemoveContractDocument(int documentId, string contractOwnerId)
        {
            var document = _dbContext.ContractDocuments.FirstOrDefault(x => x.Id == documentId);
            if (document == null) { return false; }
            if (!CheckContractAccess(document.ContractId, contractOwnerId)) { return false; }
            _dbContext.ContractDocuments.Remove(document);
            return true;
        }       

        public IList<ContractDocument> GetContractDocumentsList(int contractId, string contractOwnerId)
        {
            var contract = _dbContext.Contracts.Find(contractId);
            return contract.Documents.ToList();
        }

        public IList<ApplicationUser> GetSubDealers(string dealerId)
        {
            var dealer = _dbContext.Users.Find(dealerId);
            return dealer?.SubDealers.ToList() ?? new List<ApplicationUser>();
        }

        public ApplicationUser GetDealer(string dealerId)
        {
            return _dbContext.Users.Find(dealerId);            
        }

        private EquipmentInfo AddOrUpdateEquipment(Contract contract, EquipmentInfo equipmentInfo)
        {
            var newEquipments = equipmentInfo.NewEquipment;
            var existingEquipments = equipmentInfo.ExistingEquipment;
            var dbEquipment = _dbContext.EquipmentInfo.Find(equipmentInfo.Id);//(contract.Id);

            if (dbEquipment == null || dbEquipment.Id == 0)
            {
                equipmentInfo.ExistingEquipment = new List<ExistingEquipment>();
                equipmentInfo.NewEquipment = new List<NewEquipment>();
                equipmentInfo.Contract = contract;
                dbEquipment = _dbContext.EquipmentInfo.Add(equipmentInfo);
            }
            else
            {
                equipmentInfo.Contract = contract;
                equipmentInfo.ExistingEquipment = dbEquipment.ExistingEquipment;
                equipmentInfo.NewEquipment = dbEquipment.NewEquipment;
                _dbContext.EquipmentInfo.AddOrUpdate(equipmentInfo);
                dbEquipment = _dbContext.EquipmentInfo.Find(equipmentInfo.Id);
            }          

            if (newEquipments != null)
            {
                var existingEntities =
                    dbEquipment.NewEquipment.Where(
                    a => newEquipments.Any(ne => ne.Id == a.Id)).ToList();
                var entriesForDelete = dbEquipment.NewEquipment.Except(existingEntities).ToList();
                entriesForDelete.ForEach(e => _dbContext.NewEquipment.Remove(e));

                newEquipments.ForEach(ne =>
                {
                    var curEquipment =
                        dbEquipment.NewEquipment.FirstOrDefault(eq => eq.Id == ne.Id);
                    if (curEquipment == null || ne.Id == 0)
                    {
                        dbEquipment.NewEquipment.Add(ne);
                    }
                    else
                    {
                        ne.EquipmentInfoId = curEquipment.EquipmentInfoId;
                        if (ne.AssetNumber == null)
                        {
                            ne.AssetNumber = curEquipment.AssetNumber;
                        }
                        _dbContext.NewEquipment.AddOrUpdate(ne);
                    }
                });
            }

            if (existingEquipments != null)
            {
                var existingEntities =
                    dbEquipment.ExistingEquipment.Where(
                    a => existingEquipments.Any(ee => ee.Id == a.Id)).ToList();
                var entriesForDelete = dbEquipment.ExistingEquipment.Except(existingEntities).ToList();
                entriesForDelete.ForEach(e => _dbContext.ExistingEquipment.Remove(e));

                existingEquipments.ForEach(ee =>
                {                    
                    var curEquipment =
                        dbEquipment.ExistingEquipment.FirstOrDefault(ex => ex.Id == ee.Id);
                    if (curEquipment == null || ee.Id == 0)
                    {
                        dbEquipment.ExistingEquipment.Add(ee);
                    }
                    else
                    {
                        ee.EquipmentInfoId = curEquipment.EquipmentInfoId;
                        _dbContext.ExistingEquipment.AddOrUpdate(ee);
                    }
                });
            }

            if (dbEquipment.AgreementType == AgreementType.LoanApplication)
            {
                var provinceCode = contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                    l => l.AddressType == AddressType.MainAddress)?.State.ToProvinceCode();
                var taxRate = GetProvinceTaxRate(provinceCode);
                var loanCalculatorInput = new LoanCalculator.Input
                {
                    TaxRate = taxRate.Rate,
                    LoanTerm = contract.Equipment.LoanTerm ?? 0,
                    AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                    EquipmentCashPrice = (double?) contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                    AdminFee = contract.Equipment.AdminFee ?? 0,
                    DownPayment = contract.Equipment.DownPayment ?? 0,
                    CustomerRate = contract.Equipment.CustomerRate ?? 0
                };
                dbEquipment.ValueOfDeal = LoanCalculator.Calculate(loanCalculatorInput).TotalBorowingCost;
            }
            else
            {
                dbEquipment.ValueOfDeal = (double)(dbEquipment.TotalMonthlyPayment*dbEquipment.RequestedTerm);
            }            
            
            return dbEquipment;
        }        

        public ContractData GetContractData(int contractId, string contractOwnerId)
        {
            ContractData contractData = new ContractData()
            {
                Id = contractId
            };
            var contract = GetContractAsUntracked(contractId, contractOwnerId);
            if (contract != null)
            {
                contractData.SecondaryCustomers = contract.SecondaryCustomers?.ToList();
                contractData.Equipment = contract.Equipment;
            }
            return contractData;
        }        

        public Comment TryAddComment(Comment comment, string contractOwnerId)
        {
            //if (!CheckContractAccess(comment.ContractId, contractOwnerId)) { return false; }
            var dealer = GetUserById(contractOwnerId);
            comment.Date = DateTime.Now;
            comment.Dealer = dealer;
            _dbContext.Comments.AddOrUpdate(comment);
            return comment;
        }

        public bool TryRemoveComment(int commentId, string contractOwnerId)
        {
            var cmmnt = _dbContext.Comments.FirstOrDefault(x => x.Id == commentId && x.DealerId == contractOwnerId);
            if (cmmnt == null || cmmnt.Replies.Any()) { return false; }
            _dbContext.Comments.Remove(cmmnt);
            return true;
        }

        private bool CheckContractAccess(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Any(c => c.Id == contractId && c.Dealer.Id == contractOwnerId);
        }

        private Customer AddOrUpdateCustomerLocations(Customer customer, IEnumerable<Location> locations)
        {
            //??
            locations = locations.ToList();
            var existingEntities =
                customer.Locations.Where(
                    a => locations.Any(ca => ca.Id == a.Id || ca.AddressType == a.AddressType)).ToList();
            var entriesForDelete = customer.Locations.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            locations.ForEach(addr =>
            {
                var curAddress =
                    customer.Locations.FirstOrDefault(ca => ca.AddressType == addr.AddressType);
                if (curAddress == null)
                {
                    addr.Customer = customer;
                    customer.Locations.Add(addr);
                }
                else
                {
                    addr.Customer = customer;
                    addr.Id = curAddress.Id;
                    _dbContext.Locations.AddOrUpdate(addr);
                    //_dbContext.Entry<Location>(addr).State = EntityState.Modified;                    
                }               
            });

            return customer;
        }

        private Customer AddOrUpdateCustomerPhones(Customer customer, IList<Phone> phones)
        {
            var existingEntities =
                customer.Phones.Where(
                    p => phones.Any(pa => pa.Id == p.Id || pa.PhoneType == p.PhoneType)).ToList();
            var entriesForDelete = customer.Phones.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            phones.ForEach(phone =>
            {
                var curPhone =
                    customer.Phones.FirstOrDefault(p => p.PhoneType == phone.PhoneType);
                if (curPhone == null)
                {
                    phone.Customer = customer;
                    customer.Phones.Add(phone);
                }
                else
                {
                    phone.Customer = customer;
                    phone.Id = curPhone.Id;
                    _dbContext.Phones.AddOrUpdate(phone);
                    //_dbContext.Entry<Phone>(phone).State = EntityState.Modified;
                }
            });

            return customer;
        }

        private Customer AddOrUpdateCustomerEmails(Customer customer, IList<Email> emails)
        {
            var existingEntities =
                customer.Emails.Where(
                    e => emails.Any(em => e.Id == em.Id || e.EmailType == em.EmailType)).ToList();
            var entriesForDelete = customer.Emails.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            emails.ForEach(email =>
            {
                var curEmail =
                    customer.Emails.FirstOrDefault(e => e.EmailType == email.EmailType);
                if (curEmail == null)
                {
                    email.Customer = customer;
                    customer.Emails.Add(email);
                }
                else
                {
                    email.Customer = customer;
                    email.Id = curEmail.Id;
                    _dbContext.Emails.AddOrUpdate(email);
                    //_dbContext.Entry<Email>(email).State = EntityState.Modified;
                }
            });

            return customer;
        }        

        private void AddOrUpdateContactDetails(Contract contract, ContractDetails contractDetails)
        {
            if (contract.Details == null)
            {
                contract.Details = new ContractDetails();
            }

            if (contractDetails.AgreementType.HasValue)
            {
                contract.Details.AgreementType = contractDetails.AgreementType;
            }
            if (contractDetails.HouseSize.HasValue)
            {
                contract.Details.HouseSize = contractDetails.HouseSize;
            }
            if (contractDetails.SignatureDocumentId != null)
            {
                contract.Details.SignatureDocumentId = contractDetails.SignatureDocumentId;
            }
            if (contractDetails.SignatureTransactionId != null)
            {
                contract.Details.SignatureTransactionId = contractDetails.SignatureTransactionId;
            }
            if (contractDetails.Status != null)
            {
                contract.Details.Status = contractDetails.Status;
            }
            if (contractDetails.TransactionId != null)
            {
                contract.Details.TransactionId = contractDetails.TransactionId;
            }
            if (contractDetails.SignatureInitiatedTime.HasValue)
            {
                contract.Details.SignatureInitiatedTime = contractDetails.SignatureInitiatedTime;
            }
            if (contractDetails.SignatureStatus.HasValue)
            {
                contract.Details.SignatureStatus = contractDetails.SignatureStatus;
            }
            if (contractDetails.SignatureTime.HasValue)
            {
                contract.Details.SignatureTime = contractDetails.SignatureTime;
            }
            if (contractDetails.ScorecardPoints.HasValue)
            {
                contract.Details.ScorecardPoints = contractDetails.ScorecardPoints;
            }
            if (contractDetails.CreditAmount.HasValue)
            {
                contract.Details.CreditAmount = contractDetails.CreditAmount;
            }
        }

        private void AddOrUpdatePaymentInfo(Contract contract, PaymentInfo newData)
        {
            if (contract.PaymentInfo != null)
            {
                contract.PaymentInfo.PaymentType = newData.PaymentType;
                contract.PaymentInfo.PrefferedWithdrawalDate = newData.PrefferedWithdrawalDate;
                contract.PaymentInfo.AccountNumber = newData.AccountNumber;
                contract.PaymentInfo.BlankNumber = newData.BlankNumber;
                contract.PaymentInfo.TransitNumber = newData.TransitNumber;
                contract.PaymentInfo.EnbridgeGasDistributionAccount = newData.EnbridgeGasDistributionAccount;
                contract.PaymentInfo.MeterNumber = newData.MeterNumber;
            }
            else
            {
                contract.PaymentInfo = newData;
            }
        }

        private Customer AddOrUpdateCustomer(Customer customer)
        {
            var dbCustomer = customer.Id == 0 ? null : _dbContext.Customers.Find(customer.Id);
            if (dbCustomer == null)
            {
                customer.Phones = new List<Phone>();
                customer.Locations = new List<Location>();
                customer.Emails = new List<Email>();
                dbCustomer = _dbContext.Customers.Add(customer);                    
            }
            else
            {
                dbCustomer.FirstName = customer.FirstName;
                dbCustomer.LastName = customer.LastName;
                dbCustomer.DateOfBirth = customer.DateOfBirth;
                dbCustomer.Sin = customer.Sin;
                dbCustomer.DriverLicenseNumber = customer.DriverLicenseNumber;
                //if (customer.AllowCommunicate.HasValue)
                //{
                //    dbCustomer.AllowCommunicate = customer.AllowCommunicate;
                //}
            }
            //if (dbCustomer.Locations == null)
            //{
            //    dbCustomer.Locations = new List<Location>();
            //}
            return dbCustomer;
        }

        private Customer DeleteCustomer(int customerId)
        {
            var customer = GetCustomer(customerId);
            customer.Phones.ForEach(p => customer.Phones.Remove(p));
            customer.Locations.ForEach(l => customer.Locations.Remove(l));
            customer.Emails.ForEach(e => customer.Emails.Remove(e));
            customer = _dbContext.Customers.Remove(customer);
            return customer;
        }

        private bool AddOrUpdateAdditionalApplicants(Contract contract, IList<Customer> customers)
        {
            var existingEntities =
                contract.SecondaryCustomers.Where(
                    ho => customers.Any(cho => cho.Id == ho.Id)).ToList();

            var entriesForDelete = contract.SecondaryCustomers.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => contract.SecondaryCustomers.Remove(e));
            //entriesForDelete.ForEach(e => _dbContext.Customers.Remove(e));
            //_dbContext.Entry(e).State = EntityState.Deleted);
            customers.ForEach(ho =>
            {
                var customerLocations = ho.Locations;
                var customer = AddOrUpdateCustomer(ho);
                AddOrUpdateCustomerLocations(customer, customerLocations);
                if (existingEntities.Find(e => e.Id == customer.Id) == null)
                {
                    contract.SecondaryCustomers.Add(ho);
                }
            });

            contract.LastUpdateTime = DateTime.Now;

            return true;
        }
    }
}
