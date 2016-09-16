using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using AutoMapper.Mappers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.App_Start
{
    using Api.Models.Contract.EquipmentInformation;
    using Models.EquipmentInformation;

    public class AutoMapperConfig
    {
        public static void Configure()
        {
            MapModels();
        }

        private static void MapModels()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DriverLicenseData, RecognizedLicense>();
                cfg.CreateMap<Tuple<DriverLicenseData, IList<Alert>>, DriverLicenseViewModel>()
                    .ForMember(x => x.DriverLicense, o => o.MapFrom(src => src.Item1))
                    .ForMember(x => x.RecognitionErrors, o => o.ResolveUsing(src =>
                    src.Item2.Select(e => e.Header).ToList()));

                cfg.CreateMap<ContractDTO, DealItemOverviewViewModel>()
                    .ForMember(d => d.CustomerName, s => s.ResolveUsing(src =>
                    {
                        var customer = src.PrimaryCustomer;
                        if (customer != null)
                        {
                            return $"{customer.LastName} {customer.FirstName}";
                        }
                        return string.Empty;
                    }))
                    .ForMember(d => d.Status, s => s.ResolveUsing(src => src.ContractState.ToString()))
                    .ForMember(d => d.Action, s => s.Ignore())
                    .ForMember(d => d.Email, s => s.Ignore())
                    .ForMember(d => d.Phone, s => s.Ignore())
                    .ForMember(d => d.Date, s => s.ResolveUsing(src =>
                        (src.LastUpdateTime?.Date ?? src.CreationTime.Date).ToShortDateString()));

                cfg.CreateMap<CustomerDTO, ApplicantPersonalInfo>()
                    .ForMember(x => x.AgrreesToSendPersonalInfo, d => d.Ignore())
                    .ForMember(x => x.BirthDate, d => d.MapFrom(src => src.DateOfBirth));
                cfg.CreateMap<LocationDTO, AddressInformation>()
                    .ForMember(x => x.InstallationAddress, d => d.MapFrom(src => src.Street))
                    .ForMember(x => x.UnitNumber, d => d.MapFrom(src => src.Unit))
                    .ForMember(x => x.Province, d => d.MapFrom(src => src.State));

                cfg.CreateMap<ApplicantPersonalInfo, CustomerDTO>()
                    .ForMember(x => x.Locations, d => d.Ignore())
                    .ForMember(x => x.Phones, d => d.Ignore())
                    .ForMember(x => x.Id, d => d.Ignore())
                    .ForMember(x => x.DateOfBirth, d => d.MapFrom(src => src.BirthDate));

                cfg.CreateMap<AddressInformation, LocationDTO>()
                    .ForMember(x => x.Street, d => d.MapFrom(src => src.InstallationAddress))
                    .ForMember(x => x.Unit, d => d.MapFrom(src => src.UnitNumber))
                    .ForMember(x => x.State, d => d.MapFrom(src => src.Province))
                    .ForMember(x => x.AddressType, d => d.Ignore())
                    .ForMember(x => x.Id, d => d.Ignore())
                    .ForMember(x => x.CustomerId, d => d.Ignore());
                cfg.CreateMap<EquipmentInformationViewModel, EquipmentInformationDTO>();
                cfg.CreateMap<NewEquipmentInformation, NewEquipmentInformationDTO>();
                cfg.CreateMap<ExistingEquipmentInformation, ExistingEquipmentInformationDTO>();
            });


        }
    }
}