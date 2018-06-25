﻿using System.Collections.Generic;

namespace DealnetPortal.Api.Models.CustomerWallet
{
    public class RegisterCustomerBindingModel
    {
        public RegisterBindingModel RegisterInfo { get; set; }

        public CustomerProfileDTO Profile { get; set; }

        public List<TransactionInfoDTO> TransactionsInfo { get; set; }
    }
}
