﻿using System.Collections.Generic;

namespace DealnetPortal.Api.Models.Contract
{
    public class NewCustomerDTO
    {
        public CustomerDTO PrimaryCustomer { get; set; }
        public string CustomerComment { get; set; }
        //public List<string> HomeImprovementTypes { get; set; }//to remove
        public List<EquipmentTypeDTO> SelectedEquipmentTypes { get; set; }
        // Lead source of a client web-portal (DP, MB, OB, CW)
        public string LeadSource { get; set; }
    }
}
