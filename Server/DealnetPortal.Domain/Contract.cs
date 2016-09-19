﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class Contract
    {
        public Contract()
        {
            SecondaryCustomers = new List<Customer>();
            //Locations = new List<Location>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        public int Id { get; set; }

        public ApplicationUser Dealer { get; set; }
        public ContractState ContractState { get; set; }        

        public DateTime CreationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }
        
        public Customer PrimaryCustomer { get; set; }

        public ICollection<Customer> SecondaryCustomers { get; set; }
        
        public EquipmentInfo Equipment { get; set; }


    }
}
