using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class RateCardsService : IRateCardsService
    {
        private readonly IRateCardsRepository _rateCardsRepository;
        private readonly ILoggingService _loggingService;

        public RateCardsService(ILoggingService loggingService, IRateCardsRepository rateCardsRepository)
        {
            _loggingService = loggingService;
            _rateCardsRepository = rateCardsRepository;
        }

        public TierDTO GetRateCardsByDealerId(string id)
        {
            var tier = _rateCardsRepository.GetTierByDealerId(id);
            return Mapper.Map<TierDTO>(tier);
        }
    }
}
