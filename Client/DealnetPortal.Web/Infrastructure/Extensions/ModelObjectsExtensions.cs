using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    public static class ModelObjectsExtensions
    {
        public static CustomerDTO ToCustomerDto(this ApplicantPersonalInfo api)
        {
            return new CustomerDTO
            {
                FirstName = api.FirstName,
                LastName = api.LastName,
                DateOfBirth = api.BirthDate
            };
        }

        public static ApplicantPersonalInfo ToApplicantPersonalInfo(this CustomerDTO api)
        {
            return new ApplicantPersonalInfo
            {
                FirstName = api.FirstName,
                LastName = api.LastName,
                BirthDate = api.DateOfBirth
            };
        }

        public static ContractAddressDTO ToContractAddressDto(this AddressInformation ai, AddressType addressType)
        {
            return new ContractAddressDTO
            {
                AddressType = addressType,
                Street = ai.InstallationAddress,
                Unit = ai.UnitNumber,
                City = ai.City,
                Province = ai.Province,
                PostalCode = ai.PostalCode,
            };
        }

        public static AddressInformation ToContractAddressDto(this ContractAddressDTO ai)
        {
            return new AddressInformation
            {
                InstallationAddress = ai.Street,
                UnitNumber = ai.Unit,
                City = ai.City,
                Province = ai.Province,
                PostalCode = ai.PostalCode,
            };
        }
    }
}
