using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class CustomerFormService : ICustomerFormService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICustomerFormRepository _customerFormRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository, IUnitOfWork unitOfWork,
            ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
        }

        public CustomerLinkDTO GetCustomerLinkSettings(string dealerId)
        {
            var linkSettings = _customerFormRepository.GetCustomerLinkSettings(dealerId);
            if (linkSettings != null)
            {
                return Mapper.Map<CustomerLinkDTO>(linkSettings);
            }
            return null;
        }

        public IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId)
        {
            throw new NotImplementedException();
        }
    }
}
