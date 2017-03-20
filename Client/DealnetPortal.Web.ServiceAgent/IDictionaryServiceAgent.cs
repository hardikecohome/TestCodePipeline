using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.UserSettings;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IDictionaryServiceAgent
    {
        /// <summary>
        /// Get Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        Task<Tuple<IList<EquipmentTypeDTO>, IList<Alert>>> GetEquipmentTypes();

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
        /// Get Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetDocumentTypes();

        Task<ApplicationUserDTO> GetDealerInfo();

        Task<string> GetDealerCulture();

        Task ChangeDealerCulture(string culture);

        Task<bool> CheckDealerSkinExistence();

        Task<IList<StringSettingDTO>> GetDealerSettings();

        Task<BinarySettingDTO> GetDealerBinSetting(SettingType type);
    }
}
