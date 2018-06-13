using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Aspire.Integration.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.Domain.Repositories;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [RoutePrefix("api/dict")]
    public class DictionaryController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private IContractRepository _contractRepository { get; set; }
        private ISettingsRepository _settingsRepository { get; set; }
        private IAspireStorageReader _aspireStorageReader { get; set; }
        private IRateCardsService _rateCardsService { get; set; }
        private readonly IDealerRepository _dealerRepository;
        private readonly ILicenseDocumentRepository _licenseDocumentRepository;

        public DictionaryController(IUnitOfWork unitOfWork, IContractRepository contractRepository, ISettingsRepository settingsRepository, ILoggingService loggingService, 
            IAspireStorageReader aspireStorageReader, IDealerRepository dealerRepository, ILicenseDocumentRepository licenseDocumentRepository, IRateCardsService rateCardsService)
            : base(loggingService)
        {
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
            _settingsRepository = settingsRepository;
            _aspireStorageReader = aspireStorageReader;
            _dealerRepository = dealerRepository;
            _licenseDocumentRepository = licenseDocumentRepository;            
            _rateCardsService = rateCardsService;
        }             

        [Route("DocumentTypes")]
        [HttpGet]
        public IHttpActionResult GetAllDocumentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var docTypes = Mapper.Map<IList<DocumentTypeDTO>>(_contractRepository.GetAllDocumentTypes());
                if (docTypes == null)
                {
                    var errorMsg = "Cannot retrieve Document Types";
                    alerts.Add(new Alert
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

        [Route("DocumentTypes/{state}")]
        [HttpGet]
        public IHttpActionResult GetStateDocumentTypes(string state)
        {
            var alerts = new List<Alert>();
            try
            {
                var docTypes = Mapper.Map<IList<DocumentTypeDTO>>(_contractRepository.GetStateDocumentTypes(state));
                if (docTypes == null)
                {
                    var errorMsg = "Cannot retrieve Document Types";
                    alerts.Add(new Alert
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

        [Route("dealer/DocumentTypes/{state}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult GetDealerDocumentTypes(string state)
        {
            var alerts = new List<Alert>();
            try
            {
                var docTypes = Mapper.Map<IList<DocumentTypeDTO>>(_contractRepository.GetDealerDocumentTypes(state, LoggedInUser?.UserId));
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

        [Route("dealer/DocumentTypes")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult GetDealerDocumentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceDocTypes = Mapper.Map<IDictionary<string, IList<DocumentTypeDTO>>>(_contractRepository.GetDealerDocumentTypes(LoggedInUser?.UserId));
                if (provinceDocTypes == null)
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
                var result = new Tuple<IDictionary<string, IList<DocumentTypeDTO>>, IList<Alert>>(provinceDocTypes, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Document Types", ex);
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [Route("Dealer/EquipmentTypes")]
        [HttpGet]
        public IHttpActionResult GetDealerEquipmentTypes()
        {
            try
            {
                var alerts = new List<Alert>();
                var dealerProfile = _dealerRepository.GetDealerProfile(LoggedInUser?.UserId);
                IList<EquipmentTypeDTO> equipmentTypes;
                if (dealerProfile != null && dealerProfile.Equipments.Any())
                {
                    equipmentTypes = Mapper.Map<IList<EquipmentTypeDTO>>(dealerProfile.Equipments.Select(x => x.Equipment).ToList());
                }
                else
                {
                    equipmentTypes = Mapper.Map<IList<EquipmentTypeDTO>>(_contractRepository.GetEquipmentTypes());
                }

                if (!equipmentTypes.Any())
                {
                    var errorMsg = "Cannot retrieve Equipment Types";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.EquipmentTypesRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                return Ok(new Tuple<IList<EquipmentTypeDTO>, IList<Alert>>(equipmentTypes, alerts));
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Equipment Types", ex);
                return InternalServerError(ex);
            }
        }

        [Route("EquipmentTypes")]
        [Route("AllEquipmentTypes")]
        [HttpGet]
        public IHttpActionResult GetAllEquipmentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var equipmentTypes = _contractRepository.GetEquipmentTypes();
                var equipmentTypeDtos = Mapper.Map<IList<EquipmentTypeDTO>>(equipmentTypes);
                if (equipmentTypes == null)
                {
                    var errorMsg = "Cannot retrieve Equipment Types";
                    alerts.Add(new Alert
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

        [Route("LicenseDocuments")]
        [HttpGet]
        public IHttpActionResult GetAllLicenseDocuments()
        {
            var alerts = new List<Alert>();
            try
            {
                var licenseDocuments = _licenseDocumentRepository.GetAllLicenseDocuments();
                var licenseDocumentDtos = Mapper.Map<IList<LicenseDocumentDTO>>(licenseDocuments);
                if (licenseDocuments == null)
                {
                    var errorMsg = "Cannot retrieve License documents";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.LicenseDocumentsRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<IList<LicenseDocumentDTO>, IList<Alert>>(licenseDocumentDtos, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve License documents", ex);
                return InternalServerError(ex);
            }
        }

        [Route("ProvinceTaxRates/{province}")]
        [HttpGet]
        public IHttpActionResult GetProvinceTaxRate(string province)
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceTaxRate = _contractRepository.GetProvinceTaxRate(province);
                var provinceTaxRateDto = Mapper.Map<ProvinceTaxRateDTO>(provinceTaxRate);
                if (provinceTaxRate == null)
                {
                    var errorMsg = "Cannot retrieve Province Tax Rate";
                    alerts.Add(new Alert
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

        [Route("ProvinceTaxRates")]
        [Route("AllProvinceTaxRates")]
        [HttpGet]
        public IHttpActionResult GetAllProvinceTaxRates()
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceTaxRates = _contractRepository.GetAllProvinceTaxRates();
                var provinceTaxRateDtos = Mapper.Map<IList<ProvinceTaxRateDTO>>(provinceTaxRates);
                if (provinceTaxRates == null)
                {
                    var errorMsg = "Cannot retrieve all Province Tax Rates";
                    alerts.Add(new Alert
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

        [Route("VerificationIds/{id}")]
        [HttpGet]
        public IHttpActionResult GetVerificationId(int id)
        {
            var alerts = new List<Alert>();
            try
            {
                var verificationId = _contractRepository.GetVerficationId(id);
                var verificationIdsDto = Mapper.Map<VarificationIdsDTO>(verificationId);
                if (verificationId == null)
                {
                    var errorMsg = "Cannot retrieve Province Tax Rate";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ProvinceTaxRateRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<VarificationIdsDTO, IList<Alert>>(verificationIdsDto, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Verification Id", ex);
                return InternalServerError(ex);
            }
        }

        [Route("VerificationIds")]
        [HttpGet]
        public IHttpActionResult GetAllVerificationIds()
        {
            var alerts = new List<Alert>();
            try
            {
                var verificationIds = _contractRepository.GetAllVerificationIds();
                var verificationIdsDtos = Mapper.Map<IList<VarificationIdsDTO>>(verificationIds);
                if (verificationIds == null)
                {
                    var errorMsg = "Cannot retrieve all Verification Ids";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ProvinceTaxRateRetrievalFailed,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                var result = new Tuple<IList<VarificationIdsDTO>, IList<Alert>>(verificationIdsDtos, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve all Province Tax Rates", ex);
                return InternalServerError(ex);
            }
        }

        [Route("CreditAmount")]
        [Route("CreditAmount/{creditScore}")]
        [HttpGet]
        // GET api/dict/CreditAmount/{creditScore}
        public IHttpActionResult GetCreditAmount(int creditScore)
        {
            try
            {
                var creditAmount = _rateCardsService.GetCreditAmount(creditScore);
                return Ok(creditAmount);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to retrieve credit amount settings for creditScore = {creditScore}", ex);
                return InternalServerError(ex);
            }            
        }


        [Authorize]
        [Route("Dealer/Info")]
        [HttpGet]
        public IHttpActionResult GetDealerInfo()
        {
            try
            {
                var dealer = _contractRepository.GetDealer(LoggedInUser?.UserId);
                var dealerDto = Mapper.Map<ApplicationUserDTO>(dealer);

                try
                {                
                    dealerDto.UdfSubDealers = Mapper.Map<IList<SubDealerDTO>>(_aspireStorageReader.GetSubDealersList(dealer.AspireLogin ?? dealer.UserName));
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
        // GET api/dict/Dealer/Culture
        [Route("Dealer/Culture")]
        public string GetDealerCulture()
        {
            return _contractRepository.GetDealer(LoggedInUser?.UserId).Culture ?? _dealerRepository.GetDealerProfile(LoggedInUser?.UserId)?.Culture;
        }

        [HttpGet]
        // GET api/dict/Dealer/Culture/{dealer}
        [Route("Dealer/{dealer}/Culture")]
        public string GetDealerCulture(string dealer)
        {
            var dealerId = _dealerRepository.GetUserIdByName(dealer);
            var culture = _contractRepository.GetDealer(dealerId).Culture ?? _dealerRepository.GetDealerProfile(dealerId)?.Culture;
            return culture;
        }

        [Authorize]
        [HttpPut]
        // GET api/dict/Dealer/Culture
        [Route("Dealer/Culture/{culture}")]
        public IHttpActionResult PutDealerCulture(string culture)
        {
            try
            {
                _contractRepository.GetDealer(LoggedInUser?.UserId).Culture = culture;
                var profile = _dealerRepository.GetDealerProfile(LoggedInUser?.UserId);
                if (profile != null)
                {
                    profile.Culture = culture;
                }                    
                _unitOfWork.Save();
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet]
        // GET api/dict/Dealer/Settings
        [Route("Dealer/Settings")]
        public IHttpActionResult GetDealerSettings()
        {
            IList<StringSettingDTO> list = null;
	        LoggingService.LogInfo($"Get dealer skins settings for dealer: {LoggedInUser.UserName}");
            var settings = _settingsRepository.GetUserStringSettings(LoggedInUser?.UserId);
            if (settings?.Any() ?? false)
            {
	            LoggingService.LogInfo($"There are {settings.Count} variables for dealer: {LoggedInUser.UserName}");
                list = Mapper.Map<IList<StringSettingDTO>>(settings);
            }
            return Ok(list);
        }

        [HttpGet]
        // GET api/dict/Dealer/{hashDealerName}/Settings
        [Route("Dealer/{hashDealerName}/Settings")]
        public IHttpActionResult GetDealerSettings(string hashDealerName)
        {
            IList<StringSettingDTO> list = null;
	        LoggingService.LogInfo($"Get dealer skins settings for dealer: {hashDealerName}");
            var settings = _settingsRepository.GetUserStringSettingsByHashDealerName(hashDealerName);
            if (settings?.Any() ?? false)
            {
	            LoggingService.LogInfo($"There are {settings.Count} variables for dealer: {hashDealerName}");
                list = Mapper.Map<IList<StringSettingDTO>>(settings);
            }
            return Ok(list);
        }

        [Authorize]
        [HttpGet]
        [Route("Dealer/BinSettings/{settingType}")]
        public IHttpActionResult GetDealerBinSetting(int settingType)
        {
            SettingType sType = (SettingType) settingType;
            var binSetting = _settingsRepository.GetUserBinarySetting(sType, LoggedInUser?.UserId);
            if (binSetting != null)
            {
                var bin = new BinarySettingDTO
                {
                    Name = binSetting.Item?.Name,
                    ValueBytes = binSetting.BinaryValue
                };
                return Ok(bin);
            }
            return Ok();
        }

        [HttpGet]
        [Route("Dealer/{hashDealerName}/BinSettings/{settingType}")]
        public IHttpActionResult GetDealerBinSetting(int settingType, string hashDealerName)
        {
            SettingType sType = (SettingType)settingType;
            var binSetting = _settingsRepository.GetUserBinarySettingByHashDealerName(sType, hashDealerName);
            if (binSetting != null)
            {
                var bin = new BinarySettingDTO
                {
                    Name = binSetting.Item?.Name,
                    ValueBytes = binSetting.BinaryValue
                };
                return Ok(bin);
            }
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("Dealer/Skin/check")]
        public IHttpActionResult CheckDealerSkinExist()
        {
            try
            {
                return Ok(_settingsRepository.CheckUserSkinExist(LoggedInUser?.UserId));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }            
        }

        [HttpGet]
        [Route("Dealer/{dealer}/Skin/check")]
        public IHttpActionResult CheckDealerSkinExist(string dealer)
        {
            try
            {
                return Ok(_settingsRepository.CheckUserSkinExist(dealer));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }        

        [Authorize]
        [Route("RateReductionCards")]
        [HttpGet]
        public IHttpActionResult GetAllRateReductionCards()
        {
            var alerts = new List<Alert>();
            try
            {
                var reductionCards = _rateCardsService.GetRateReductionCards();
                if (reductionCards == null)
                {
                    var errorMsg = "Cannot retrieve Rate Reduction Cards";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Message = errorMsg
                    });
                    LoggingService.LogError(errorMsg);
                }
                
                var result = new Tuple<IList<RateReductionCardDTO>, IList<Alert>>(reductionCards, alerts);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to retrieve Equipment Types", ex);
                return InternalServerError(ex);
            }
        }

        [Route("Dealer/Tier")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDealerTier()
        {
            try
            {
                var submitResult = _rateCardsService.GetRateCardsByDealerId(LoggedInUser?.UserId);

                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Dealer/Tier/contract/{contractId}")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDealerTier(int contractId)
        {
            try
            {
                var submitResult = _rateCardsService.GetRateCardsForContract(contractId, LoggedInUser?.UserId);

                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
