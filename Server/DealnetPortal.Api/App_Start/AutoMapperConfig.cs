using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
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
            mapperConfig.CreateMap<ContractAddress, ContractAddressDTO>();
            mapperConfig.CreateMap<Customer, CustomerDTO>();
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.ContractAddress, o => o.MapFrom(src => src.ContractAddress))
                .ForMember(x => x.Customers, o => o.MapFrom(src => src.Customers));
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<ContractAddressDTO, ContractAddress>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Contract, o => o.Ignore());
            mapperConfig.CreateMap<CustomerDTO, Customer>()
                .ForMember(d => d.Contract, s => s.Ignore());
            mapperConfig.CreateMap<ContractDTO, ContractData>()
                .ForMember(x => x.ContractAddress, o => o.MapFrom(src => src.ContractAddress))
                .ForMember(x => x.Customers, o => o.MapFrom(src => src.Customers));
            mapperConfig.CreateMap<ContractDTO, Contract>()
                .ForMember(x => x.ContractAddress, o => o.MapFrom(src => src.ContractAddress))
                .ForMember(x => x.Customers, o => o.MapFrom(src => src.Customers))
                .ForMember(d => d.Dealer, s => s.Ignore());
        }

    }
}