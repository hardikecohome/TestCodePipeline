using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    [RoutePrefix("api/dict")]
    public class DictionaryController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private IContractRepository ContractRepository { get; set; }
        private IAspireStorageService AspireStorageService { get; set; }

        public DictionaryController(IUnitOfWork unitOfWork, IContractRepository contractRepository, ILoggingService loggingService, IAspireStorageService aspireStorageService)
            : base(loggingService)
        {
            _unitOfWork = unitOfWork;
            ContractRepository = contractRepository;
            AspireStorageService = aspireStorageService;
        }             

        [Route("DocumentTypes")]
        [HttpGet]
        public IHttpActionResult GetDocumentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var docTypes = Mapper.Map<IList<DocumentTypeDTO>>(ContractRepository.GetDocumentTypes());
                if (docTypes == null)
                {
                    var errorMsg = "Cannot retrieve Document Types";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.EquipmentTypesRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<IList<DocumentTypeDTO>, IList<Alert>>(docTypes, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Document Types", ex);
                return InternalServerError(ex);
            }
        }

        [Route("EquipmentTypes")]
        [HttpGet]
        public IHttpActionResult GetEquipmentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var equipmentTypes = ContractRepository.GetEquipmentTypes();
                var equipmentTypeDtos = Mapper.Map<IList<EquipmentTypeDTO>>(equipmentTypes);
                if (equipmentTypes == null)
                {
                    var errorMsg = "Cannot retrieve Equipment Types";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.EquipmentTypesRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<IList<EquipmentTypeDTO>, IList<Alert>>(equipmentTypeDtos, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Equipment Types", ex);
                return InternalServerError(ex);
            }
        }

        [Route("{province}/ProvinceTaxRate")]
        [HttpGet]
        public IHttpActionResult GetProvinceTaxRate(string province)
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceTaxRate = ContractRepository.GetProvinceTaxRate(province);
                var provinceTaxRateDto = Mapper.Map<ProvinceTaxRateDTO>(provinceTaxRate);
                if (provinceTaxRate == null)
                {
                    var errorMsg = "Cannot retrieve Province Tax Rate";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ProvinceTaxRateRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<ProvinceTaxRateDTO, IList<Alert>>(provinceTaxRateDto, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Province Tax Rate", ex);
                return InternalServerError(ex);
            }
        }

        [Route("AllProvinceTaxRates")]
        [HttpGet]
        public IHttpActionResult GetAllProvinceTaxRates()
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceTaxRates = ContractRepository.GetAllProvinceTaxRates();
                var provinceTaxRateDtos = Mapper.Map<IList<ProvinceTaxRateDTO>>(provinceTaxRates);
                if (provinceTaxRates == null)
                {
                    var errorMsg = "Cannot retrieve all Province Tax Rates";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ProvinceTaxRateRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<IList<ProvinceTaxRateDTO>, IList<Alert>>(provinceTaxRateDtos, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve all Province Tax Rates", ex);
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [Route("GetDealerInfo")]
        [HttpGet]
        public IHttpActionResult GetDealerInfo()
        {
            try
            {
                var dealer = ContractRepository.GetDealer(LoggedInUser?.UserId);
                var dealerDto = Mapper.Map<ApplicationUserDTO>(dealer);

                try
                {                
                    dealerDto.UdfSubDealers = AspireStorageService.GetSubDealersList(dealer.AspireLogin ?? dealer.UserName);
                }
                catch (Exception ex)
                {
                    //it's a not critical error on this step and we continue flow
                    LoggingService.LogError("Failed to get subdealers from Aspire", ex);
                }

                return Ok(dealerDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet]
        // GET api/Account/GetDealerCulture
        [Route("GetDealerCulture")]
        public string GetDealerCulture()
        {
            return ContractRepository.GetDealer(LoggedInUser?.UserId).Culture;            
        }

        [Authorize]
        [HttpPut]
        // GET api/Account/PutDealerCulture
        [Route("PutDealerCulture")]
        public IHttpActionResult PutDealerCulture(string culture)
        {
            try
            {
                ContractRepository.GetDealer(LoggedInUser?.UserId).Culture = culture;
                _unitOfWork.Save();
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
