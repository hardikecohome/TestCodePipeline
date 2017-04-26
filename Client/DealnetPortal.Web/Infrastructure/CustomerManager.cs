using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class CustomerManager : ICustomerManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;

        public CustomerManager(
            IDictionaryServiceAgent dictionaryServiceAgent, 
            IContractServiceAgent contractServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractServiceAgent = contractServiceAgent;
        }

        public async Task<NewCustomerViewModel> GetTemplateAsync()
        {
            var model = new NewCustomerViewModel();

            var equipment = await _dictionaryServiceAgent.GetEquipmentTypes();
            model.EquipmentTypes = equipment.Item1?.OrderBy(x => x.Description).ToList() ?? new List<EquipmentTypeDTO>();

            var taxes = await _dictionaryServiceAgent.GetAllProvinceTaxRates();
            model.ProvinceTaxRates = taxes.Item1 ?? new List<ProvinceTaxRateDTO>();

            return model;
        }

        public void Add(NewCustomerViewModel customer)
        {
            throw new NotImplementedException();
        }
    }
}