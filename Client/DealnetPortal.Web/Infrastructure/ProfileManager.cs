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
        private readonly IDealerServiceAgent _dealerServiceAgent;

        public ProfileManager(IDictionaryServiceAgent dictionaryServiceAgent, IDealerServiceAgent dealerServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _dealerServiceAgent = dealerServiceAgent;
        }

        public async Task<ProfileViewModel> GetDealerProfile()
        {
            //model.PostalCodes = new List<PostalCodeInformation>
            //{
            //    new PostalCodeInformation {Id = 1, Value = "12345"},
            //    new PostalCodeInformation {Id = 2, Value = "qwerty"}
            //};

            //model.Categories = new List<CategoryInformation>()
            //{
            //    new CategoryInformation
            //    {
            //        Type = "ECO1",
            //        Description = "Air Conditioner"
            //    },
            //    new CategoryInformation
            //    {
            //        Type = "ECO2",
            //        Description = "Boiler"
            //    },
            //};
            var  dealerProfile = await _dealerServiceAgent.GetDealerProfile();
            var model = AutoMapper.Mapper.Map<ProfileViewModel>(dealerProfile);
            var equipment = await _dictionaryServiceAgent.GetEquipmentTypes();
            model.EquipmentTypes = equipment.Item1?.OrderBy(x => x.Description).ToList() ?? new List<EquipmentTypeDTO>();

            return model;
        }
    }
}