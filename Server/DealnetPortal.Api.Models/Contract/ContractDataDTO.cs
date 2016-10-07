﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    using EquipmentInformation;

    public class ContractDataDTO
    {
        public int Id { get; set; }
        public CustomerDTO PrimaryCustomer { get; set; }
        public IList<CustomerDTO> SecondaryCustomers { get; set; }

        public ContractDetailsDTO Details { get; set; }

        // Here is locations for a primary customer
        public IList<LocationDTO> Locations { get; set; }

        public PaymentInfoDTO PaymentInfo { get; set; }
        public EquipmentInfoDTO Equipment { get; set; }
    }
}
