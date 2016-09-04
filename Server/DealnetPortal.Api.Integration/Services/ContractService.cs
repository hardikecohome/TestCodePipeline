using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;

        public ContractService(IContractRepository contractRepository, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
        }

        public ContractDTO CreateContract(string contractOwnerId)
        {
            try
            {            
                var newContract = _contractRepository.CreateContract(contractOwnerId);
                var contractDTO = Mapper.Map<ContractDTO>(newContract);
                _loggingService.LogInfo($"A new contract [{newContract.Id}] created by user [{contractOwnerId}]");
                return contractDTO;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to create a new contract", ex);
                throw;
            }
        }

        public IList<ContractDTO> GetContracts(string contractOwnerId)
        {
            throw new NotImplementedException();
        }

        public ContractDTO GetContract(int contractId)
        {
            throw new NotImplementedException();
        }

        public bool UpdateContractClientData(int contractId, IList<ContractAddressDTO> addresses, IList<CustomerDTO> customers)
        {
            throw new NotImplementedException();
        }

        public bool InitiateCreditCheck(int contractId)
        {
            throw new NotImplementedException();
        }

        public bool GetCreditCheckResult(int contractId)
        {
            throw new NotImplementedException();
        }

        public bool SubmitContract(int contractId)
        {
            throw new NotImplementedException();
        }
    }
}
