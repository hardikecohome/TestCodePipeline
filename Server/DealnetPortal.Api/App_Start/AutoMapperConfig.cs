using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using AutoMapper;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Storage;
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
                cfg.AllowNullCollections = true;
                MapDomainsToModels(cfg);
                MapModelsToDomains(cfg);
            });
        }

        private static void MapDomainsToModels(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<Location, LocationDTO>();
            mapperConfig.CreateMap<Phone, PhoneDTO>();
            mapperConfig.CreateMap<EquipmentInfo, EquipmentInfoDTO>();
            mapperConfig.CreateMap<ExistingEquipment, ExistingEquipmentDTO>();
            mapperConfig.CreateMap<NewEquipment, NewEquipmentDTO>();
            mapperConfig.CreateMap<ContactInfo, ContactInfoDTO>()
                .ForMember(x => x.Phones, o => o.MapFrom(src => src.Phones));
            mapperConfig.CreateMap<Customer, CustomerDTO>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations));
            mapperConfig.CreateMap<PaymentInfo, PaymentInfoDTO>();
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.PrimaryCustomer, o => o.MapFrom(src => src.PrimaryCustomer))
                .ForMember(x => x.SecondaryCustomers, o => o.MapFrom(src => src.SecondaryCustomers))
                .ForMember(x => x.ContactInfo, o => o.MapFrom(src => src.ContactInfo))
                .ForMember(x => x.PaymentInfo, o => o.MapFrom(src => src.PaymentInfo));
            mapperConfig.CreateMap<EquipmentType, EquipmentTypeDTO>();
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<LocationDTO, Location>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<PhoneDTO, Phone>()
                .ForMember(x => x.ContactInfo, s => s.Ignore());

            mapperConfig.CreateMap<EquipmentInfoDTO, EquipmentInfo>()
                .ForMember(d => d.Contract, x => x.Ignore());
            mapperConfig.CreateMap<NewEquipmentDTO, NewEquipment>()
                .ForMember(x => x.EquipmentInfo, d => d.Ignore())
                .ForMember(x => x.EquipmentInfoId, d => d.Ignore());
            mapperConfig.CreateMap<ExistingEquipmentDTO, ExistingEquipment>()
                .ForMember(x => x.EquipmentInfo, d => d.Ignore())
                .ForMember(x => x.EquipmentInfoId, d => d.Ignore());

            mapperConfig.CreateMap<CustomerDTO, Customer>()
                .ForMember(x => x.Locations, o => o.MapFrom(src => src.Locations));

            mapperConfig.CreateMap<ContractDataDTO, ContractData>();
            mapperConfig.CreateMap<PaymentInfoDTO, PaymentInfo>()
                .ForMember(d => d.Contract, s => s.Ignore());
            mapperConfig.CreateMap<ContactInfoDTO, ContactInfo>()
                .ForMember(d => d.Contract, s => s.Ignore());
            mapperConfig.CreateMap<ContractDataDTO, ContractData>();                
            mapperConfig.CreateMap<ContractDTO, Contract>()
                .ForMember(d => d.Dealer, s => s.Ignore())
                .ForMember(d => d.PaymentInfo, s => s.Ignore())
                .ForMember(d => d.ContactInfo, s => s.Ignore())
                .ForMember(d => d.Details, s => s.Ignore());

            mapperConfig.CreateMap<AgreementTemplateDTO, AgreementTemplate>()
                .ForMember(d => d.AgreementForm, s => s.MapFrom(src => src.AgreementFormRaw))
                .ForMember(d => d.EquipmentTypes, s => s.Ignore());
        }
    }
}