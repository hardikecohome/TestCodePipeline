using System;
using System.Collections.Generic;
using System.Configuration;
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

        #region Public
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
                    LastUpdateTime = DateTime.Now,
                    Dealer = dealer,
                    CreateOperator = dealer.UserName,
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
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)
                .Where(c => c.Dealer.Id == ownerUserId || c.Dealer.ParentDealerId == ownerUserId).ToList();
            return contracts;
        }

        public IList<Contract> GetDealerLeads(string userId)
        {
            var creditReviewStates = ConfigurationManager.AppSettings["CreditReviewStatus"] != null
                ? ConfigurationManager.AppSettings["CreditReviewStatus"].Split(',').Select(s => s.Trim()).ToArray()
                : new string[] {"20-Credit Review"};
            //var aspireCreditReviewState = ConfigurationManager.AppSettings["CreditReviewStatus"] ?? "20-Credit Review";
            var contractCreatorRoleId = _dbContext.Roles.FirstOrDefault(r => r.Name == UserRole.CustomerCreator.ToString())?.Id;
            var dealerProfile = _dbContext.DealerProfiles.FirstOrDefault(p => p.DealerId == userId);
            var eqList = dealerProfile?.Equipments.Select(e => e.Equipment.Type);
            var pcList = dealerProfile?.Areas.Select(e => e.PostalCode);
            var contracts = _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)
                .Where(c => (c.IsCreatedByBroker == true
                || (contractCreatorRoleId == null || c.Dealer.Roles.Select(r => r.RoleId).Contains(contractCreatorRoleId))) &&
                c.Equipment.NewEquipment.Any() &&
                c.PrimaryCustomer.Locations.Any(l => l.AddressType == AddressType.InstallationAddress || l.AddressType == AddressType.MainAddress) &&
                (c.ContractState >= ContractState.CreditContirmed && !creditReviewStates.Contains(c.Details.Status))).ToList();
            if (eqList!=null && eqList.Any())
            {
                contracts = contracts.Where(c => eqList.Any(eq => eq == c.Equipment?.NewEquipment?.FirstOrDefault()?.Type)).ToList();
            }
            if (pcList!=null && pcList.Any())
            {
                contracts = contracts.Where(c => pcList.Any(pc => 
                    (c.PrimaryCustomer.Locations.FirstOrDefault(x => x.AddressType == AddressType.InstallationAddress) 
                    ?? c.PrimaryCustomer.Locations.FirstOrDefault(x => x.AddressType == AddressType.MainAddress)).PostalCode.Contains(pc))).ToList();
            }
             
            return contracts;
        }

        /// <summary>
        /// Get contract created by an user (dealer)
        /// </summary>
        /// <param name="ownerUserId">user Id</param>
        /// <returns>List of contracts</returns>
        public IList<Contract> GetContractsCreatedByUser(string userId)
        {
            var user = GetUserById(userId);
            var contracts = _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)
                .Where(c => c.CreateOperator == user.UserName && !string.IsNullOrEmpty(c.Details.TransactionId)).ToList();
            return contracts;
        }

        public int GetNewlyCreatedCustomersContractsCount(string ownerUserId)
        {
            var contractsCount = _dbContext.Contracts
                .Count(c => (c.Dealer.Id == ownerUserId || c.Dealer.ParentDealerId == ownerUserId) && c.IsNewlyCreated == true );
            return contractsCount;
        }

        public IList<Contract> GetContracts(IEnumerable<int> ids, string ownerUserId)
        {
            var contracts = _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)
                .Where(
                    c =>
                        ids.Any(id => id == c.Id) &&
                        (c.Dealer.Id == ownerUserId || c.Dealer.ParentDealerId == ownerUserId)).ToList();
            return contracts;
        }

        public Contract FindContractBySignatureId(string signatureTransactionId)
        {
            return _dbContext.Contracts.FirstOrDefault(c => c.Details.SignatureTransactionId == signatureTransactionId);
        }

        public ContractState? GetContractState(int contractId, string contractOwnerId)
        {
            return
                _dbContext.Contracts.AsNoTracking()
                    .FirstOrDefault(
                        c =>
                            c.Id == contractId &&
                            (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId))?
                    .ContractState;
        }

        public Contract UpdateContractState(int contractId, string contractOwnerId, ContractState newState)
        {
            var contract = GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                contract.ContractState = newState;

                if (contract.ContractState == ContractState.CreditCheckDeclined)
                {
                    AddOrUpdateInitialCustomers(contract);
                    contract.WasDeclined = true;                    
                }

                contract.LastUpdateTime = DateTime.Now;
                contract.LastUpdateOperator = GetDealer(contractOwnerId)?.UserName;
            }
            return contract;
        }

        public Contract GetContract(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .Include(c => c.Documents)
                .FirstOrDefault(
                    c =>
                        c.Id == contractId &&
                        (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId));
        }

        public Contract GetContractAsUntracked(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.HomeOwners)
                .Include(c => c.InitialCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .AsNoTracking().
                FirstOrDefault(
                    c =>
                        c.Id == contractId &&
                        (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId));
        }

        public Contract AssignContract(int contractId, string newContractOwnerId)
        {
            var updated = false;

            var contract = _dbContext.Contracts
               .Include(x => x.Equipment.NewEquipment)
               .Include(x => x.PrimaryCustomer)
               .Include(c => c.PrimaryCustomer.Locations)
               .FirstOrDefault(x => x.Id == contractId);            

            if (contract != null)
            {
                if (contract.IsCreatedByBroker == true)
                {
                    var locations = contract.PrimaryCustomer.Locations.ToList();
                    if (locations.Exists(l => l.AddressType == AddressType.InstallationAddress))
                    {
                        var mainLoc = locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress);
                        if (mainLoc != null)
                        {
                            locations.Remove(mainLoc);
                        }
                        //?
                        var installLoc = locations.FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress);
                        if (installLoc != null)
                        {
                            installLoc.AddressType = AddressType.MainAddress;                            
                        }
                    }
                    AddOrUpdateCustomerLocations(contract.PrimaryCustomer, locations);
                }

                var dealer = GetDealer(newContractOwnerId);

                if (dealer != null)
                {
                    contract.DealerId = dealer.Id;
                    contract.IsNewlyCreated = true;
                    contract.IsCreatedByBroker = false;
                    contract.LastUpdateTime = DateTime.Now;

                    _dbContext.Entry(contract).State = EntityState.Modified;

                    updated = true;
                }
            }

            return updated ? contract : null;
        }

        public bool DeleteContract(string contractOwnerId, int contractId)
        {
            bool deleted = false;
            var contract =
                _dbContext.Contracts.FirstOrDefault(
                    c =>
                        (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId) &&
                        c.Id == contractId);
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
            var contract =
                _dbContext.Contracts.FirstOrDefault(
                    c =>
                        (c.Dealer.Id == contractOwnerId || c.Dealer.ParentDealerId == contractOwnerId) &&
                        c.Id == contractId);
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
            contract.LastUpdateOperator = GetDealer(contractOwnerId)?.UserName;
            return contract;
        }

        public Contract UpdateContractData(ContractData contractData, string contractOwnerId)
        {
            if (contractData != null)
            {
                bool updated = false;
                var contract = GetContract(contractData.Id, contractOwnerId);
                if (contract != null)
                {
                    if (!string.IsNullOrEmpty(contractData.DealerId))
                    {
                        if (contract.DealerId != contractData.DealerId)
                        {
                            //TODO: check availability to change dealer Id (we have ability to change t)
                            contract.DealerId = contractData.DealerId;
                            updated = true;
                        }
                    }

                    if (contractData.ExternalSubDealerId != null &&
                        contract.ExternalSubDealerId != contractData.ExternalSubDealerId)
                    {
                        contract.ExternalSubDealerId = contractData.ExternalSubDealerId;
                        updated = true;
                    }
                    if (contractData.ExternalSubDealerName != null &&
                        contract.ExternalSubDealerName != contractData.ExternalSubDealerName)
                    {
                        contract.ExternalSubDealerName = contractData.ExternalSubDealerName;
                        updated = true;
                    }

                    if (contractData.PrimaryCustomer != null && contract.WasDeclined != true)
                    {
                        // ?
                        if (contractData.PrimaryCustomer.Id == 0 && contract.PrimaryCustomer != null)
                        {
                            contractData.PrimaryCustomer.Id = contract.PrimaryCustomer.Id;
                        }

                        var homeOwnerLocations = contractData.PrimaryCustomer.Locations;
                        var homeOwnerPhones = contractData.PrimaryCustomer.Phones;
                        var homeOwnerEmails = contractData.PrimaryCustomer.Emails;
                        var homeOwner = AddOrUpdateCustomer(contractData.PrimaryCustomer);
                        if (homeOwner != null)
                        {
                            contract.PrimaryCustomer = homeOwner;
                            AddOrUpdateCustomerLocations(contract.PrimaryCustomer, homeOwnerLocations);

                            if (homeOwnerPhones != null)
                            {
                                AddOrUpdateCustomerPhones(homeOwner, homeOwnerPhones.ToList());
                            }

                            if (homeOwnerEmails != null)
                            {
                                AddOrUpdateCustomerEmails(homeOwner, homeOwnerEmails.ToList());
                            }

                            contract.ContractState = ContractState.CustomerInfoInputted;
                            updated = true;
                        }
                    }

                    if (contractData.SecondaryCustomers != null)
                    {
                        AddOrUpdateAdditionalApplicants(contract, contractData.SecondaryCustomers);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        updated = true;
                    }

                    if (contractData.HomeOwners != null)
                    {
                        AddOrUpdateHomeOwners(contract, contractData.HomeOwners);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        updated = true;
                    }

                    if (!contract.WasDeclined.HasValue || contract.WasDeclined == false)
                    {
                        AddOrUpdateInitialCustomers(contract);
                        updated = true;
                    }

                    if (contractData.Equipment != null)
                    {
                        AddOrUpdateEquipment(contract, contractData.Equipment);
                        updated = true;
                    }

                    if (contractData.Details != null)
                    {
                        AddOrUpdateContactDetails(contract, contractData.Details);
                        updated = true;
                    }

                    if (contractData.PaymentInfo != null)
                    {
                        AddOrUpdatePaymentInfo(contract, contractData.PaymentInfo);
                        updated = true;
                    }

                    if (updated)
                    {
                        contract.LastUpdateTime = DateTime.Now;
                        contract.LastUpdateOperator = GetDealer(contractOwnerId)?.UserName;
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
                    if (customerInfo.PreferredContactMethod.HasValue)
                    {
                        dbCustomer.PreferredContactMethod = customerInfo.PreferredContactMethod;
                    }
                    if (!string.IsNullOrWhiteSpace(customerInfo.DriverLicenseNumber))
                    {
                        dbCustomer.DriverLicenseNumber = customerInfo.DriverLicenseNumber;
                    }
                    if (!string.IsNullOrWhiteSpace(customerInfo.Sin))
                    {
                        dbCustomer.Sin = customerInfo.Sin;
                    }
                    if (customerInfo.AccountId != null)
                    {
                        dbCustomer.AccountId = customerInfo.AccountId;
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

        public IList<ProvinceTaxRate> GetAllProvinceTaxRates()
        {
            return _dbContext.ProvinceTaxRates.ToList();
        }

        public AspireStatus GetAspireStatus(string status)
        {
            return _dbContext.AspireStatuses.FirstOrDefault(x => x.Status.Contains(status.Trim()));
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
                            EquipmentCashPrice = (double?) contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                            AdminFee = contract.Equipment.AdminFee ?? 0,
                            DownPayment = contract.Equipment.DownPayment ?? 0,
                            CustomerRate = contract.Equipment.CustomerRate ?? 0
                        };
                        var loanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
                        totalMp = (decimal) loanCalculatorOutput.TotalMonthlyPayment;
                    }
                    else
                    {
                        totalMp = (contract.Equipment.TotalMonthlyPayment ?? 0) +
                                  (contract.Equipment.TotalMonthlyPayment ?? 0)*(decimal) (rate.Rate/100);
                    }
                }
            }

            return totalMp;
        }

        public PaymentSummary GetContractPaymentsSummary(int contractId, string contractOwnerId)
        {
            PaymentSummary paymentSummary = new PaymentSummary();

            var contract = GetContract(contractId, contractOwnerId);
            //_dbContext.Contracts.Find(contractId);
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
                        paymentSummary.TotalAllMonthlyPayment = (decimal) loanCalculatorOutput.TotalAllMonthlyPayments;

                        paymentSummary.LoanDetails = loanCalculatorOutput;
                    }
                    else
                    {
                        paymentSummary.MonthlyPayment = contract.Equipment.TotalMonthlyPayment;
                        paymentSummary.Hst = (contract.Equipment.TotalMonthlyPayment ?? 0)*(decimal) (rate.Rate/100);
                        paymentSummary.TotalPayment = (contract.Equipment.TotalMonthlyPayment ?? 0) +
                                                      (contract.Equipment.TotalMonthlyPayment ?? 0)*
                                                      (decimal) (rate.Rate/100);
                        paymentSummary.TotalAllMonthlyPayment = paymentSummary.TotalPayment*
                                                                (contract.Equipment.RequestedTerm ?? 0);
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
                if (string.IsNullOrEmpty(document.DocumentName) || !document.DocumentName.StartsWith(docType.Prefix))
                {
                    document.DocumentName = docType.Prefix + document.DocumentName;
                }
            }

            document.CreationDate = DateTime.Now;

            if (contract != null)
            {
                var otherType = _dbContext.DocumentTypes.FirstOrDefault(t => string.IsNullOrEmpty(t.Prefix));
                var dbDocument =
                    contract.Documents.FirstOrDefault(
                        d =>
                            otherType != null && otherType.Id == docTypeId
                                ? d.DocumentTypeId == docTypeId && d.Id == document.Id
                                : d.DocumentTypeId == docTypeId);
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
            if (document == null)
            {
                return false;
            }
            if (!CheckContractAccess(document.ContractId, contractOwnerId))
            {
                return false;
            }
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

        public int UpdateSubDealersHierarchyByRelatedTransactions(IEnumerable<string> transactionIds, string ownerUserId)
        {
            int updated = 0;

            var dbTransactions =
                _dbContext.Contracts.Where(c => !string.IsNullOrEmpty(c.Details.TransactionId))
                    .Select(t => t.Details.TransactionId)
                    .ToList();
            dbTransactions = dbTransactions.Intersect(transactionIds).ToList();

            var dealers =
                _dbContext.Contracts.Where(
                    c =>
                        dbTransactions.Any(t => t == c.Details.TransactionId) &&
                        (c.Dealer.Id != ownerUserId && c.Dealer.ParentDealerId != ownerUserId))
                    .Select(c => c.Dealer).ToArray();
            if (dealers?.Any() ?? false)
            {
                updated = dealers.Length;
                dealers.ForEach(d => d.ParentDealerId = ownerUserId);
            }

            return updated;
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

        public int? RemoveComment(int commentId, string contractOwnerId)
        {
            var cmmnt = _dbContext.Comments.FirstOrDefault(x => x.Id == commentId && x.DealerId == contractOwnerId);
            if (cmmnt == null || cmmnt.Replies.Any())
            {
                return null;
            }
            var cmmntId = cmmnt.ContractId;
            _dbContext.Comments.Remove(cmmnt);
            return cmmntId;
        }

        public bool IsContractUnassignable(int contractId)
        {
            var creditReviewStates = ConfigurationManager.AppSettings["CreditReviewStatus"] != null
                ? ConfigurationManager.AppSettings["CreditReviewStatus"].Split(',').Select(s => s.Trim()).ToArray()
                : new string[] { "20-Credit Review" };
            var contractCreatorRoleId = _dbContext.Roles.FirstOrDefault(r => r.Name == UserRole.CustomerCreator.ToString())?.Id;
            
            if (_dbContext.Users.Any(u => !u.DealerProfileId.HasValue && (contractCreatorRoleId != null && !u.Roles.Select(r => r.RoleId).Contains(contractCreatorRoleId))))
                return false;

            var contract = _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.NewEquipment)
                .SingleOrDefault(c => c.Id == contractId);
            if (contract == null)
            {
                return false;
            }
            if (contract.ContractState < ContractState.CreditContirmed || creditReviewStates.Contains(contract.Details.Status))
            {
                return false;
            }
            var contractEquipment = contract?.Equipment?.NewEquipment.Select(e => e.Type).FirstOrDefault();
            var contractPostalCode = contract?.PrimaryCustomer?.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.PostalCode;

            return _dbContext.DealerProfiles.Any(dp => dp.Equipments.Any(e => e.Equipment.Type == contractEquipment)
                                                       && dp.Areas.Any( a => a.PostalCode.Length <= contractPostalCode.Length &&
                                                                contractPostalCode.Substring(0, a.PostalCode.Length) == a.PostalCode));
        }

        #endregion

        #region Private
        private bool CheckContractAccess(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Any(c => c.Id == contractId && c.Dealer.Id == contractOwnerId);
        }

        private EquipmentInfo AddOrUpdateEquipment(Contract contract, EquipmentInfo equipmentInfo)
        {
            var newEquipments = equipmentInfo.NewEquipment;
            var existingEquipments = equipmentInfo.ExistingEquipment;
            var dbEquipment = _dbContext.EquipmentInfo.Find(equipmentInfo.Id); //(contract.Id);

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

                if (string.IsNullOrEmpty(equipmentInfo.InstallerFirstName))
                {
                    equipmentInfo.InstallerFirstName = dbEquipment.InstallerFirstName;
                }
                if (string.IsNullOrEmpty(equipmentInfo.InstallerLastName))
                {
                    equipmentInfo.InstallerLastName = dbEquipment.InstallerLastName;
                }
                if (!equipmentInfo.InstallationDate.HasValue)
                {
                    equipmentInfo.InstallationDate = dbEquipment.InstallationDate;
                }
                if (!equipmentInfo.EstimatedInstallationDate.HasValue)
                {
                    equipmentInfo.EstimatedInstallationDate = dbEquipment.EstimatedInstallationDate;
                }

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
                        if (string.IsNullOrEmpty(ne.InstalledModel))
                        {
                            ne.InstalledModel = curEquipment.InstalledModel;
                        }
                        if (string.IsNullOrEmpty(ne.InstalledSerialNumber))
                        {
                            ne.InstalledSerialNumber = curEquipment.InstalledSerialNumber;
                        }
                        if (!ne.EstimatedInstallationDate.HasValue)
                        {
                            ne.EstimatedInstallationDate = curEquipment.EstimatedInstallationDate;
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

            var provinceCode = contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                l => l.AddressType == AddressType.MainAddress)?.State.ToProvinceCode();
            var taxRate = GetProvinceTaxRate(provinceCode);
            if (dbEquipment.AgreementType == AgreementType.LoanApplication)
            {
                if (dbEquipment.AmortizationTerm.HasValue && dbEquipment.AmortizationTerm > 0)
                {
                    var loanCalculatorInput = new LoanCalculator.Input
                    {
                        TaxRate = taxRate?.Rate ?? 0,
                        LoanTerm = contract.Equipment.LoanTerm ?? 0,
                        AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                        EquipmentCashPrice = (double?) contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                        AdminFee = contract.Equipment.AdminFee ?? 0,
                        DownPayment = contract.Equipment.DownPayment ?? 0,
                        CustomerRate = contract.Equipment.CustomerRate ?? 0
                    };
                    dbEquipment.ValueOfDeal = LoanCalculator.Calculate(loanCalculatorInput).TotalAmountFinanced;
                }                
            }
            else
            {
                dbEquipment.ValueOfDeal =
                    (double?)
                        ((dbEquipment.TotalMonthlyPayment ?? 0) +
                         (contract.Equipment.TotalMonthlyPayment ?? 0) * (decimal)(taxRate.Rate / 100));
            }

            return dbEquipment;
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
            if (contractDetails.Notes != null)
            {
                contract.Details.Notes = contractDetails.Notes;
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
            }
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
                    ho => customers.Any(cho => cho.Id == ho.Id) || (contract.WasDeclined == true && contract.InitialCustomers.Any(ic => ic.Id == ho.Id))).ToList();

            var entriesForDelete = contract.SecondaryCustomers.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => contract.SecondaryCustomers.Remove(e));
            //entriesForDelete.ForEach(e => _dbContext.Customers.Remove(e));
            //_dbContext.Entry(e).State = EntityState.Deleted);
            customers.ForEach(ho =>
            {
                if (contract.WasDeclined != true || contract.InitialCustomers.All(ic => ic.Id != ho.Id))
                {
                    var customerLocations = ho.Locations;
                    var customer = AddOrUpdateCustomer(ho);
                    AddOrUpdateCustomerLocations(customer, customerLocations);
                    if (existingEntities.Find(e => e.Id == customer.Id) == null)
                    {
                        contract.SecondaryCustomers.Add(ho);
                    }
                }
            });

            contract.LastUpdateTime = DateTime.Now;

            return true;
        }

        private bool AddOrUpdateHomeOwners(Contract contract, IList<Customer> homeOwners)
        {
            var existingEntities =
                contract.HomeOwners.Where(
                    ho => homeOwners.Any(cho => cho.Id == ho.Id) || (contract.WasDeclined == true && contract.InitialCustomers.Any(ic => ic.Id == ho.Id))).ToList();
            var entriesForDelete = contract.HomeOwners.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => contract.HomeOwners.Remove(e));

            homeOwners.ForEach(ho =>
            {
                Customer sc = null;
                if (ho.Id != 0)
                {
                    sc = _dbContext.Customers.Find(ho.Id);
                }
                // new created
                if (sc == null && ho.Id == 0)
                {
                    if (contract.PrimaryCustomer.FirstName == ho.FirstName &&
                        contract.PrimaryCustomer.LastName == ho.LastName &&
                        contract.PrimaryCustomer.DateOfBirth == ho.DateOfBirth)
                    {
                        sc = contract.PrimaryCustomer;
                    }
                    else
                    {
                        sc =
                            contract.SecondaryCustomers.FirstOrDefault(
                                c =>
                                    c.FirstName == ho.FirstName && c.LastName == ho.LastName &&
                                    c.DateOfBirth == ho.DateOfBirth);
                    }
                }
                if (sc != null && (contract.WasDeclined != true || contract.InitialCustomers.All(ic => ic.Id != sc.Id)))
                {
                    contract.HomeOwners.Add(sc);
                }
            });

            return true;
        }

        private bool AddOrUpdateInitialCustomers(Contract contract)
        {
            var currentCustomers = new List<Customer>();
            if (contract.PrimaryCustomer != null)
            {
                currentCustomers.Add(contract.PrimaryCustomer);
            }
            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                currentCustomers.AddRange(contract.SecondaryCustomers);
            }

            var existingEntities =
                contract.InitialCustomers.Where(
                    ic => currentCustomers.Any(cc => cc == ic)).ToList();
            var entriesForDelete = contract.InitialCustomers.Except(existingEntities).ToList();
            var entriesForAdd = currentCustomers.Except(contract.InitialCustomers).ToList();
            entriesForDelete.ForEach(e => contract.InitialCustomers.Remove(e));
            entriesForAdd.ForEach(e => contract.InitialCustomers.Add(e));

            return true;
        }
        #endregion

    }
}
