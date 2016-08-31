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
            mapperConfig.CreateMap<HomeOwner, HomeOwnerDTO>();
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.ContractAddress, o => o.MapFrom(src => src.ContractAddress))
                .ForMember(x => x.HomeOwners, o => o.MapFrom(src => src.HomeOwners));
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<ContractAddressDTO, ContractAddress>();
            mapperConfig.CreateMap<HomeOwnerDTO, HomeOwner>();
            mapperConfig.CreateMap<ContractDTO, ContractData>()
                .ForMember(x => x.ContractAddress, o => o.MapFrom(src => src.ContractAddress))
                .ForMember(x => x.HomeOwners, o => o.MapFrom(src => src.HomeOwners));
        }

    }
}