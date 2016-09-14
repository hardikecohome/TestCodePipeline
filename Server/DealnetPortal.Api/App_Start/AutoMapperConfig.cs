using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.App_Start
{
    public static class AutoMapperConfig
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                MapDomainsToModels(cfg);
                MapModelsToDomains(cfg);
            });
        }

        private static void MapDomainsToModels(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<Location, LocationDTO>();
            mapperConfig.CreateMap<Phone, PhoneDTO>();
            mapperConfig.CreateMap<Customer, CustomerDTO>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.Phones, o => o.MapFrom(src => src.Phones));
            //mapperConfig.CreateMap<ContractCustomer, CustomerDTO>()
            //    .ForMember(x => x.Id, o => o.MapFrom(src => src.CustomerId))
            //    .ForMember(x => x.FirstName, o => o.MapFrom(src => src.Customer.FirstName))
            //    .ForMember(x => x.LastName, o => o.MapFrom(src => src.Customer.LastName))
            //    .ForMember(x => x.DateOfBirth, o => o.MapFrom(src => src.Customer.DateOfBirth));
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.PrimaryCustomer, o => o.MapFrom(src => src.PrimaryCustomer))
                .ForMember(x => x.SecondaryCustomers, o => o.MapFrom(src => src.SecondaryCustomers));
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<LocationDTO, Location>();
            mapperConfig.CreateMap<PhoneDTO, Phone>();
            mapperConfig.CreateMap<CustomerDTO, Customer>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.Phones, o => o.MapFrom(src => src.Phones));
            //mapperConfig.CreateMap<CustomerDTO, ContractCustomer>()
            //    .ForMember(d => d.Id, s => s.Ignore())
            //    .ForMember(d => d.Customer, s => s.MapFrom(src => src))
            //    .ForMember(d => d.CustomerId, s => s.MapFrom(src => src.Id));

            mapperConfig.CreateMap<ContractDataDTO, ContractData>();                
            //mapperConfig.CreateMap<ContractDTO, Contract>()
            //    .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
            //    .ForMember(x => x.ContractCustomers, o => o.MapFrom(src => src.Customers))
            //    .ForMember(d => d.Dealer, s => s.Ignore());
        }

    }
}