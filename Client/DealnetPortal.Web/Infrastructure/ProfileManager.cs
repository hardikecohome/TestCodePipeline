using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Web.Models.MyProfile;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class ProfileManager : IProfileManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly IDealerServiceAgent _dealerServiceAgent;

        public ProfileManager(IDictionaryServiceAgent dictionaryServiceAgent, IDealerServiceAgent dealerServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _dealerServiceAgent = dealerServiceAgent;
        }

        public async Task<ProfileViewModel> GetDealerProfile()
        {
            var  dealerProfile = await _dealerServiceAgent.GetDealerProfile();
            var model = AutoMapper.Mapper.Map<ProfileViewModel>(dealerProfile) ?? new ProfileViewModel();
            var equipment = await _dictionaryServiceAgent.GetEquipmentTypes();
            model.EquipmentTypes = equipment.Item1?.OrderBy(x => x.Description).ToList() ?? new List<EquipmentTypeDTO>();

            return model;
        }

        public async Task<IList<Alert>> UpdateDealerProfile(ProfileViewModel model)
        {
            var dealerProfile = AutoMapper.Mapper.Map<DealerProfileDTO>(model);
            return  await _dealerServiceAgent.UpdateDealerProfile(dealerProfile);
        }
    }
}