using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    public class NewEquipmentInformationDTO
    {
  
        public int Quantity { get; set; }

        public string Description { get; set; }


        public double Cost { get; set; }

        public double MonthlyCost { get; set; }

        public DateTime EstimatedInstallationDate { get; set; }
        
        public double TotalMonthlyPayment { get; set; }
    }
}
