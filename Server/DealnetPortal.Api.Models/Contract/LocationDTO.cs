﻿using System;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class LocationDTO : AddressDTO
    {
        public int Id { get; set; }

        public AddressType AddressType { get; set; }
        public ResidenceType ResidenceType { get; set; }

        public DateTime? MoveInDate { get; set; }

        public int CustomerId { get; set; }
    }
}
