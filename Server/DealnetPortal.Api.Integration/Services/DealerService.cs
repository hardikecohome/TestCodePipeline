using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;
        private readonly ILoggingService _loggingService;


        public DealerService(IDealerRepository dealerRepository, ILoggingService loggingService)
        {
            _dealerRepository = dealerRepository;
            _loggingService = loggingService;
        }

        public DealerProfileDTO GetDealerProfile(string dealerId)
        {
            var profile = _dealerRepository.GetDealerProfile(dealerId);
            return Mapper.Map<DealerProfileDTO>(profile);
        }
    }

    
}
