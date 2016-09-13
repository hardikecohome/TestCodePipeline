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
            mapperConfig.CreateMap<Customer, CustomerDTO>()
                .ForMember(x => x.CustomerOrder, o => o.Ignore());
            mapperConfig.CreateMap<ContractCustomer, CustomerDTO>()
                .ForMember(x => x.Id, o => o.MapFrom(src => src.CustomerId))
                .ForMember(x => x.FirstName, o => o.MapFrom(src => src.Customer.FirstName))
                .ForMember(x => x.LastName, o => o.MapFrom(src => src.Customer.LastName))
                .ForMember(x => x.DateOfBirth, o => o.MapFrom(src => src.Customer.DateOfBirth));
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.Customers, o => o.MapFrom(src => src.ContractCustomers));
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<LocationDTO, Location>()
                .ForMember(x => x.Contract, o => o.Ignore());
            mapperConfig.CreateMap<CustomerDTO, Customer>();
            mapperConfig.CreateMap<CustomerDTO, ContractCustomer>()
                .ForMember(d => d.Id, s => s.Ignore())
                .ForMember(d => d.Customer, s => s.MapFrom(src => src))
                .ForMember(d => d.CustomerId, s => s.MapFrom(src => src.Id));

            mapperConfig.CreateMap<ContractDTO, ContractData>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.Customers, o => o.MapFrom(src => src.Customers));
            mapperConfig.CreateMap<ContractDTO, Contract>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.ContractCustomers, o => o.MapFrom(src => src.Customers))
                .ForMember(d => d.Dealer, s => s.Ignore());
        }

    }
}