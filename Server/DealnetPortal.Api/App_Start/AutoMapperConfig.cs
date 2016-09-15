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
    using Models.Contract.EquipmentInformation;

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
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.PrimaryCustomer, o => o.MapFrom(src => src.PrimaryCustomer))
                .ForMember(x => x.SecondaryCustomers, o => o.MapFrom(src => src.SecondaryCustomers));

            mapperConfig.CreateMap<EquipmentInfo, EquipmentInformationDTO>();
            mapperConfig.CreateMap<ExistingEquipment, ExistingEquipmentInformationDTO>();
            mapperConfig.CreateMap<NewEquipment, NewEquipmentInformationDTO>();
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<LocationDTO, Location>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<PhoneDTO, Phone>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<CustomerDTO, Customer>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations))
                .ForMember(x => x.Phones, o => o.MapFrom(src => src.Phones));

            mapperConfig.CreateMap<ContractDataDTO, ContractData>();                
            mapperConfig.CreateMap<ContractDTO, Contract>()
                .ForMember(d => d.Dealer, s => s.Ignore());

            mapperConfig.CreateMap<EquipmentInformationDTO, EquipmentInfo>();
            mapperConfig.CreateMap<NewEquipmentInformationDTO, NewEquipment>();
            mapperConfig.CreateMap<ExistingEquipmentInformationDTO, ExistingEquipment>();
        }
    }
}