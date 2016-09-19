using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    public class NewEquipmentDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        public string Description { get; set; }


        public decimal Cost { get; set; }

        public decimal MonthlyCost { get; set; }

        public DateTime EstimatedInstallationDate { get; set; }
        
        public decimal TotalMonthlyPayment { get; set; }
    }
}
