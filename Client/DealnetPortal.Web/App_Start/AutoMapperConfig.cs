using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using AutoMapper;
using AutoMapper.Mappers;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
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
            Mapper.Initialize(cfg =>
            {
                cfg.AllowNullCollections = true;
                MapModelsToVMs(cfg);
                MapVMsToModels(cfg);
            });
        }        

        private static void MapVMsToModels(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ApplicantPersonalInfo, CustomerDTO>()
                    .ForMember(x => x.Locations, d => d.Ignore())
                    .ForMember(x => x.Id, d => d.ResolveUsing(src => src.CustomerId ?? 0))
                    .ForMember(x => x.Phones, d => d.Ignore())
                    .ForMember(x => x.Emails, d => d.Ignore())
                    .ForMember(x => x.DateOfBirth, d => d.MapFrom(src => src.BirthDate));

            cfg.CreateMap<AddressInformation, LocationDTO>()
                .ForMember(x => x.Street, d => d.MapFrom(src => src.InstallationAddress))
                .ForMember(x => x.Unit, d => d.MapFrom(src => src.UnitNumber))
                .ForMember(x => x.State, d => d.MapFrom(src => src.Province))
                .ForMember(x => x.AddressType, d => d.Ignore())
                .ForMember(x => x.Id, d => d.Ignore())
                .ForMember(x => x.CustomerId, d => d.Ignore());

            cfg.CreateMap<EquipmentInformationViewModel, EquipmentInfoDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.ContractId ?? 0));
            cfg.CreateMap<NewEquipmentInformation, NewEquipmentDTO>();
            cfg.CreateMap<ExistingEquipmentInformation, ExistingEquipmentDTO>();

            cfg.CreateMap<AddressInformation, LocationDTO>()
                .ForMember(x => x.Street, d => d.MapFrom(src => src.InstallationAddress))
                .ForMember(x => x.Unit, d => d.MapFrom(src => src.UnitNumber))
                .ForMember(x => x.State, d => d.MapFrom(src => src.Province))
                .ForMember(x => x.AddressType, d => d.Ignore())
                .ForMember(x => x.Id, d => d.Ignore())
                .ForMember(x => x.CustomerId, d => d.Ignore());

            cfg.CreateMap<PaymentInfoViewModel, PaymentInfoDTO>().ForMember(x => x.Id, d => d.Ignore());

            cfg.CreateMap<CommentViewModel, CommentDTO>()
                .ForMember(x => x.DealerId, d => d.Ignore());

            cfg.CreateMap<ContactInfoViewModel, CustomerDataDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.CustomerId))
                .ForMember(x => x.CustomerInfo, d => d.Ignore())
                .ForMember(x => x.Locations, d => d.Ignore())
                .ForMember(x => x.Phones, d => d.ResolveUsing(src =>
                {
                    List<PhoneDTO> phones = new List<PhoneDTO>();
                    if (!string.IsNullOrEmpty(src.HomePhone))
                    {
                        phones.Add(new PhoneDTO()
                        {
                            CustomerId = src.CustomerId,
                            PhoneNum = src.HomePhone,
                            PhoneType = PhoneType.Home
                        });                    
                    }
                    if (!string.IsNullOrEmpty(src.BusinessPhone))
                    {
                        phones.Add(new PhoneDTO()
                        {
                            CustomerId = src.CustomerId,
                            PhoneNum = src.BusinessPhone,
                            PhoneType = PhoneType.Business
                        });
                    }
                    if (!string.IsNullOrEmpty(src.CellPhone))
                    {
                        phones.Add(new PhoneDTO()
                        {
                            CustomerId = src.CustomerId,
                            PhoneNum = src.CellPhone,
                            PhoneType = PhoneType.Cell
                        });
                    }
                    return phones.Any() ? phones : null;
                }))
                .ForMember(x => x.Emails, d => d.ResolveUsing(src =>
                {
                    if (!string.IsNullOrEmpty(src.EmailAddress))
                    {
                        return new List<EmailDTO>()
                        {
                            new EmailDTO()
                            {
                                CustomerId = src.CustomerId,
                                EmailType = EmailType.Main,
                                EmailAddress = src.EmailAddress
                            }
                        };
                    }
                    return null;
                }));
        }

        private static void MapModelsToVMs(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ApplicationUserDTO, SubDealer>()
                .ForMember(x => x.DisplayName, o => o.MapFrom(src => src.UserName));
            cfg.CreateMap<DriverLicenseData, RecognizedLicense>();
            cfg.CreateMap<Tuple<DriverLicenseData, IList<Alert>>, DriverLicenseViewModel>()
                .ForMember(x => x.DriverLicense, o => o.MapFrom(src => src.Item1))
                .ForMember(x => x.RecognitionErrors, o => o.ResolveUsing(src =>
                src.Item2.Select(e => e.Header).ToList()));

            cfg.CreateMap<ContractDTO, DealItemOverviewViewModel>()
                .ForMember(d => d.TransactionId, s => s.ResolveUsing(src => src.Details?.TransactionId ?? $"Internal : {src.Id}"))
                .ForMember(d => d.CustomerName, s => s.ResolveUsing(src =>
                {
                    var customer = src.PrimaryCustomer;
                    if (customer != null)
                    {
                        return $"{customer.FirstName} {customer.LastName}";
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.Status, s => s.ResolveUsing(src => src.Details?.Status ?? src.ContractState.GetEnumDescription()))
                .ForMember(d => d.AgreementType, s => s.ResolveUsing(src => src.Equipment?.AgreementType.GetEnumDescription()))
                .ForMember(d => d.PaymentType, s => s.ResolveUsing(src => src.PaymentInfo?.PaymentType.GetEnumDescription()))
                .ForMember(d => d.Action, s => s.Ignore())
                .ForMember(d => d.Email, s => s.ResolveUsing(src => src.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress))                
                .ForMember(d => d.Phone, s => s.ResolveUsing(src => src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Home)?.PhoneNum
                    ?? src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Cell)?.PhoneNum))                
                .ForMember(d => d.Date, s => s.ResolveUsing(src =>
                    (src.LastUpdateTime?.Date ?? src.CreationTime.Date).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))
                .ForMember(d => d.Equipment, s => s.ResolveUsing(src =>
                    {
                        var equipment = src.Equipment?.NewEquipment;
                        if (equipment != null)
                        {
                            return equipment.Select(eq => eq.Type).ConcatWithComma();
                        }
                        return string.Empty;
                    }))
                  .ForMember(d => d.RemainingDescription, s => s.ResolveUsing(src =>
                    {
                        var stb = new StringBuilder();
                        src.PrimaryCustomer?.Locations?.ForEach(x =>
                        {
                            stb.AppendLine(x.Street);
                            stb.AppendLine(x.Unit);
                            stb.AppendLine(x.City);
                            stb.AppendLine(x.State);
                            stb.AppendLine(x.PostalCode);
                        });
                        src.PrimaryCustomer?.Phones?.ForEach(p => stb.AppendLine(p.PhoneNum));
                        src.PrimaryCustomer?.Emails?.ForEach(e => stb.AppendLine(e.EmailAddress));
                        src.SecondaryCustomers?.ForEach(x =>
                        {
                            stb.AppendLine(x.FirstName);
                            stb.AppendLine(x.LastName);
                            x.Phones?.ForEach(p => stb.AppendLine(p.PhoneNum));
                            x.Emails?.ForEach(e => stb.AppendLine(e.EmailAddress));
                        });
                        src.Equipment?.NewEquipment?.ForEach(x =>
                        {
                            stb.AppendLine(x.Description);
                            stb.AppendLine(x.Description);
                        });
                        stb.AppendLine(src.Equipment?.SalesRep);
                        stb.AppendLine(src.PaymentInfo?.EnbridgeGasDistributionAccount);
                        stb.AppendLine(src.PaymentInfo?.AccountNumber);
                        return stb.ToString();
                    }));

                cfg.CreateMap<CustomerDTO, ApplicantPersonalInfo>()
                    .ForMember(x => x.BirthDate, d => d.MapFrom(src => src.DateOfBirth))
                    .ForMember(x => x.CustomerId, d => d.MapFrom(src => src.Id));
                cfg.CreateMap<LocationDTO, AddressInformation>()
                    .ForMember(x => x.InstallationAddress, d => d.MapFrom(src => src.Street))
                    .ForMember(x => x.UnitNumber, d => d.MapFrom(src => src.Unit))
                    .ForMember(x => x.Province, d => d.MapFrom(src => src.State));                

                cfg.CreateMap<NewEquipmentDTO, NewEquipmentInformation>();
                cfg.CreateMap<ExistingEquipmentDTO, ExistingEquipmentInformation>();
                cfg.CreateMap<EquipmentInfoDTO, EquipmentInformationViewModel>()
                    .ForMember(x => x.ContractId, d => d.MapFrom(src => src.Id))
                    .ForMember(x => x.ProvinceTaxRate, d => d.Ignore())
                    .ForMember(x => x.CreditAmount, d => d.Ignore());

                cfg.CreateMap<CommentDTO, CommentViewModel>();
                cfg.CreateMap<ContractDocumentDTO, ExistingDocument>()
                .ForMember(x => x.DocumentId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.DocumentName, d => d.ResolveUsing(src => src.DocumentName.Substring(src.DocumentName.IndexOf("_") + 1)));

                cfg.CreateMap<PaymentInfoDTO, PaymentInfoViewModel>();
                cfg.CreateMap<CustomerDTO, ContactInfoViewModel>()
                    .ForMember(x => x.CustomerId, d => d.MapFrom(src => src.Id))
                    .ForMember(x => x.BusinessPhone, d => d.ResolveUsing(src =>
                        src.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum))
                    .ForMember(x => x.HomePhone, d => d.ResolveUsing(src =>
                        src.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum))
                    .ForMember(x => x.CellPhone, d => d.ResolveUsing(src =>
                        src.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum))
                    .ForMember(x => x.EmailAddress, d => d.ResolveUsing(src =>
                        src.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress));
        }


    }
}