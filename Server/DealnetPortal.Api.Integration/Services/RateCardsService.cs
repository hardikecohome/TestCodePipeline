using AutoMapper;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain.Repositories;

namespace DealnetPortal.Api.Integration.Services
{
    public class RateCardsService : IRateCardsService
    {
        private readonly IRateCardsRepository _rateCardsRepository;
        private readonly IContractRepository _contractRepository;

        public RateCardsService(IRateCardsRepository rateCardsRepository, IContractRepository contractRepository)
        {
            _rateCardsRepository = rateCardsRepository;
            _contractRepository = contractRepository;
        }

        public TierDTO GetRateCardsByDealerId(string dealerId)
        {
            var tier = _rateCardsRepository.GetTierByDealerId(dealerId, null, null);
            return Mapper.Map<TierDTO>(tier);
        }

        public TierDTO GetRateCardsByDealerId(int contractId, string dealerId)
        {
            var contract = _contractRepository.GetContract(contractId, dealerId);
            int beacons = contract.PrimaryCustomer.CreditReport?.Beacon ?? 0;
            var tier = _rateCardsRepository.GetTierByDealerId(dealerId, beacons, contract.DateOfSubmit);
            return Mapper.Map<TierDTO>(tier);
        }
    }
}
