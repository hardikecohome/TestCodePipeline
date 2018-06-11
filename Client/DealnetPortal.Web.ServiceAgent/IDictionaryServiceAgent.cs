﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.UserSettings;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IDictionaryServiceAgent
    {
        /// <summary>
        /// Get Dealer Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetEquipmentTypes();
        
        /// <summary>
        /// Get Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetAllEquipmentTypes();

        Task<Tuple<IList<LicenseDocumentDTO>, IList<Alert>>> GetAllLicenseDocuments();

        /// <summary>
        /// Get Province Tax Rate
        /// </summary>
        /// <param name="province">Province abbreviation</param>
        /// <returns>Tax Rate for particular Province</returns>
        Task<Tuple<ProvinceTaxRateDTO, IList<Alert>>> GetProvinceTaxRate(string province);

        /// <summary>
        /// Get all Province Tax Rates
        /// </summary>
        /// <returns>All Tax Rates</returns>
        Task<Tuple<IList<ProvinceTaxRateDTO>, IList<Alert>>> GetAllProvinceTaxRates();

        /// <summary>
        /// Get Verification Id
        /// </summary>
        /// <param name="Id">Province abbreviation</param>
        /// <returns>Tax Rate for particular Province</returns>
        Task<Tuple<VarificationIdsDTO, IList<Alert>>> GetVerificationId(int id);

        /// <summary>
        /// Get all Province Tax Rates
        /// </summary>
        /// <returns>All Tax Rates</returns>
        Task<Tuple<IList<VarificationIdsDTO>, IList<Alert>>> GetAllVerificationIds();

        /// <summary>
        /// Get all document types (funding checklist)
        /// </summary>
        /// <returns>List of Document Type</returns>
        Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetDocumentTypes();

        Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetStateDocumentTypes(string state);

        Task<Tuple<IDictionary<string, IList<DocumentTypeDTO>>, IList<Alert>>> GetAllStateDocumentTypes();

        Task<ApplicationUserDTO> GetDealerInfo();

        Task<string> GetDealerCulture(string dealerName = null);

        Task ChangeDealerCulture(string culture);

        Task<bool> CheckDealerSkinExistence();

        Task<IList<StringSettingDTO>> GetDealerSettings(string dealerName);

        Task<BinarySettingDTO> GetDealerBinSetting(SettingType type);

        Task<CustomerLinkDTO> GetShareableLinkSettings();

        Task<IList<Alert>> UpdateShareableLinkSettings(CustomerLinkDTO customerLink);

        Task<CustomerLinkLanguageOptionsDTO> GetCustomerLinkLanguageOptions(string hashDealerName, string culture);

        Task<Tuple<IList<RateReductionCardDTO>, IList<Alert>>> GetAllRateReductionCards();

        /// <summary>
        /// Get dealer tier by Id
        /// </summary>
        Task<TierDTO> GetDealerTier();
        /// <summary>
        /// Get dealer tier by contract Id
        /// </summary>
        Task<TierDTO> GetDealerTier(int contractId);
    }
}
