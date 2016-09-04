using System;
using System.Collections.Generic;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests
{
    [TestClass]
    public class MappingTest
    {
        [TestInitialize]
        public void Intialize()
        {
            DealnetPortal.Api.App_Start.AutoMapperConfig.Configure();
        }

        [TestMethod]
        public void AssertMapperConfiguration()
        {
            try
            {
                Mapper.AssertConfigurationIsValid();
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }

        }

        [TestMethod]
        public void ContractToContractDTOTest()
        {
            var contract = new Contract()
            {
                Id = 1,
                ContractState = ContractState.Started,
                Addresses = new List<ContractAddress>()
                { 
                    new ContractAddress()
                    {
                        City = "Paris",
                        Id = 1,
                    }
                },
                Customers = new List<Customer>()
                {
                    new Customer()
                    {
                        FirstName = "FstName",
                        LastName = "LstName",
                        DateOfBirth = DateTime.Today,
                        Id = 1
                    }
                }
            };

            var contractDTO = Mapper.Map<ContractDTO>(contract);
        }

        [TestMethod]
        public void ContractDTOToContractTest()
        {
            var contractDTO = new ContractDTO()
            {
                Id = 1,
                ContractState = Models.Enumeration.ContractState.Started,
                Addresses = new List<ContractAddressDTO>()
                { 
                    new ContractAddressDTO()
                    {
                        City = "Paris"
                    }
                },
                Customers = new List<CustomerDTO>()
                {
                    new CustomerDTO()
                    {
                        FirstName = "FstName",
                        LastName = "LstName",
                        DateOfBirth = DateTime.Today,
                        Id = 1
                    }
                }
            };

            var contract = Mapper.Map<Contract>(contractDTO);
        }
    }
}
