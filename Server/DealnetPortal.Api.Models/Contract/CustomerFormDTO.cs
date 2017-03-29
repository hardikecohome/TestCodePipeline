﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerFormDTO
    {
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public CustomerDTO PrimaryCustomer { get; set; }
        public string CustomerComment { get; set; }
        /// <summary>
        /// Dealer service selected by customer
        /// </summary>
        public string SelectedService { get; set; }
    }
}
