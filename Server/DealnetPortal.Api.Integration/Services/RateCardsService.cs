using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class RateCardsService : IRateCardsService
    {
        private readonly IRateCardsRepository _rateCardsRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;

        public RateCardsService(ILoggingService loggingService, IRateCardsRepository rateCardsRepository, IContractRepository contractRepository)
        {
            _loggingService = loggingService;
            _rateCardsRepository = rateCardsRepository;
            _contractRepository = contractRepository;
        }

        public TierDTO GetRateCardsByDealerId(string dealerId)
        {
            var tier = _rateCardsRepository.GetTierByDealerId(dealerId, null);
            return Mapper.Map<TierDTO>(tier);
        }

        public TierDTO GetRateCardsByDealerId(int contractId, string dealerId)
        {
            var contract = _contractRepository.GetContract(contractId, dealerId);
            var tier = _rateCardsRepository.GetTierByDealerId(dealerId, contract.DateOfSubmit);
            return Mapper.Map<TierDTO>(tier);
        }
    }
}
