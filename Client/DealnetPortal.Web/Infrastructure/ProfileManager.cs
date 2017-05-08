using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.MyProfile;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class ProfileManager : IProfileManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public ProfileManager(IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<ProfileViewModel> Get()
        {
            var model = new ProfileViewModel();
            model.PostalCodes = new List<PostalCodeInformation>
            {
                new PostalCodeInformation {Id = 1, Value = "12345"},
                new PostalCodeInformation {Id = 2, Value = "qwerty"},
            };

            var equipment = await _dictionaryServiceAgent.GetEquipmentTypes();
            model.EquipmentTypes = equipment.Item1?.OrderBy(x => x.Description).ToList() ?? new List<EquipmentTypeDTO>();

            return model;
        }
    }
}