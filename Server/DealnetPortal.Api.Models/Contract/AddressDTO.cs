﻿using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Models.Contract
{
    public class AddressDTO
    {
        [MaxLength(100)]
        public string Street { get; set; }
        [MaxLength(10)]
        public string Unit { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(30)]
        public string State { get; set; }
        [MaxLength(10)]
        public string PostalCode { get; set; }
    }
}
