﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    public class ExistingEquipmentInformationDTO
    {
        public int Id { get; set; }
        public bool DealerIsReplacing { get; set; }
        public bool IsRental { get; set; }

        
        public string RentalCompany { get; set; }

       
        public string EstimatedAge { get; set; }

       
        public string Make { get; set; }

        
        public string Model { get; set; }

        
        public string SerialNumber { get; set; }

        
        public string GeneralCondition { get; set; }

        
        public string Notes { get; set; }
    }
}
