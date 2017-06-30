using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.UI;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Helpers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Aspire.Integration.Models.AspireDb;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;
using Contract = DealnetPortal.Domain.Contract;

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
                MapAspireDomainsToModels(cfg);
                MapModelsToDomains(cfg);
            });
        }

        private static void MapDomainsToModels(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<ApplicationUser, ApplicationUserDTO>()
                .ForMember(x => x.SubDealers, o => o.Ignore())
                .ForMember(x => x.UdfSubDealers, d => d.Ignore());
            mapperConfig.CreateMap<GenericSubDealer, SubDealerDTO>();
            mapperConfig.CreateMap<Location, LocationDTO>();
            mapperConfig.CreateMap<Phone, PhoneDTO>()
                .ForMember(x => x.CustomerId, o => o.MapFrom(src => src.Customer != null ? src.Customer.Id : 0));
            mapperConfig.CreateMap<Email, EmailDTO>()
                .ForMember(x => x.CustomerId, o => o.MapFrom(src => src.Customer != null ? src.Customer.Id : 0));
            mapperConfig.CreateMap<EquipmentInfo, EquipmentInfoDTO>();
            mapperConfig.CreateMap<ExistingEquipment, ExistingEquipmentDTO>();
            mapperConfig.CreateMap<NewEquipment, NewEquipmentDTO>()
                .ForMember(x => x.TypeDescription, d => d.Ignore());
            mapperConfig.CreateMap<Comment, CommentDTO>()
                .ForMember(x => x.IsOwn, s => s.Ignore())
                .ForMember(d => d.AuthorName, s => s.ResolveUsing(src => src.Dealer.UserName));
            mapperConfig.CreateMap<Customer, CustomerDTO>()
                .ForMember(x => x.IsHomeOwner, d => d.Ignore())
                .ForMember(x => x.IsInitialCustomer, d => d.Ignore());
            mapperConfig.CreateMap<PaymentInfo, PaymentInfoDTO>();
            mapperConfig.CreateMap<ContractDetails, ContractDetailsDTO>();
            mapperConfig.CreateMap<Contract, ContractDTO>()
                .ForMember(x => x.PrimaryCustomer, o => o.MapFrom(src => src.PrimaryCustomer))
                .ForMember(x => x.SecondaryCustomers, o => o.MapFrom(src => src.SecondaryCustomers))
                .ForMember(x => x.PaymentInfo, o => o.MapFrom(src => src.PaymentInfo))
                .ForMember(x => x.Comments, o => o.MapFrom(src => src.Comments))
                .ForMember(x => x.OnCreditReview, o=> o.Ignore())
                .AfterMap((c, d) =>
                {
                    if (d?.PrimaryCustomer != null)
                    {
                        d.PrimaryCustomer.IsHomeOwner = c.HomeOwners?.Any(ho => ho.Id == d.PrimaryCustomer.Id) ?? false;
                        d.PrimaryCustomer.IsInitialCustomer = c.InitialCustomers?.Any(ho => ho.Id == d.PrimaryCustomer.Id) ?? false;
                    }
                    d?.SecondaryCustomers?.ForEach(sc =>
                    {
                        sc.IsHomeOwner = c.HomeOwners?.Any(ho => ho.Id == sc.Id) ?? false;
                        sc.IsInitialCustomer = c.InitialCustomers?.Any(ho => ho.Id == sc.Id) ?? false;
                    });
                    if (!string.IsNullOrEmpty(c.Equipment?.Notes) && d.Details != null)
                    {
                        d.Details.Notes = c.Equipment?.Notes;
                    }
                });                
                //.ForMember(x => x.Documents, d => d.Ignore());
            mapperConfig.CreateMap<EquipmentType, EquipmentTypeDTO>().
                ForMember(x => x.Description, s => s.ResolveUsing(src => ResourceHelper.GetGlobalStringResource(src.DescriptionResource)));
            mapperConfig.CreateMap<ProvinceTaxRate, ProvinceTaxRateDTO>().
                ForMember(x => x.Description, s => s.ResolveUsing(src => ResourceHelper.GetGlobalStringResource(src.Description)));

            mapperConfig.CreateMap<AgreementTemplate, AgreementTemplateDTO>()
                .ForMember(d => d.AgreementFormRaw, s => s.MapFrom(src => src.AgreementForm))
                .ForMember(d => d.DealerName, s => s.ResolveUsing(src => src.Dealer?.UserName ?? string.Empty));
                //.ForMember(d => d.EquipmentTypes, s => s.ResolveUsing(src => src.EquipmentTypes?.Select(e => e.Type)));

            mapperConfig.CreateMap<DocumentType, DocumentTypeDTO>().
                ForMember(x => x.Description, s => s.ResolveUsing(src => ResourceHelper.GetGlobalStringResource(src.DescriptionResource)));
            mapperConfig.CreateMap<ContractDocument, ContractDocumentDTO>()
                .ForMember(x => x.DocumentBytes, d => d.Ignore());

            mapperConfig.CreateMap<SettingValue, StringSettingDTO>()
                .ForMember(x => x.Name, d => d.ResolveUsing(src => src.Item?.Name))
                .ForMember(x => x.Value, d => d.MapFrom(s => s.StringValue));

            mapperConfig.CreateMap<CustomerLink, CustomerLinkDTO>()
                .ForMember(x => x.EnabledLanguages,
                    d => d.ResolveUsing(src => src.EnabledLanguages?.Select(l => l.LanguageId).Cast<LanguageCode>().ToList()))
                .ForMember(x => x.HashLink, d => d.MapFrom(s=>s.HashLink))
                .ForMember(x => x.Services, d => d.ResolveUsing(src => src.Services?.GroupBy(k => k.LanguageId).ToDictionary(ds => (LanguageCode)ds.Key, ds => ds.Select(s => s.Service).ToList())));
            mapperConfig.CreateMap<DealerEquipment, DealerEquipmentDTO>()
                .ForMember(x => x.Equipment, d => d.MapFrom( src => src.Equipment))
                .ForMember(x => x.ProfileId, d => d.MapFrom( src => src.ProfileId));
            mapperConfig.CreateMap<DealerArea, DealerAreaDTO>();
            mapperConfig.CreateMap<DealerProfile, DealerProfileDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.DealerId, d => d.MapFrom(src => src.DealerId))
                .ForMember(x => x.EquipmentList, d => d.ResolveUsing(src => src.Equipments.Any() ? src.Equipments : null))
                .ForMember(x => x.PostalCodesList, d => d.ResolveUsing(src => src.Areas.Any() ? src.Areas : null));

           
        }

        private static void MapAspireDomainsToModels(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<Aspire.Integration.Models.AspireDb.Contract, ContractDTO>()
                .ForMember(d => d.Id, s => s.UseValue(0))
                .ForMember(d => d.LastUpdateTime, s => s.MapFrom(src => src.LastUpdateDateTime))
                .ForMember(d => d.CreationTime, s => s.MapFrom(src => src.LastUpdateDateTime))
                .ForMember(d => d.Details, s => s.ResolveUsing(src =>
                {
                    var details = new ContractDetailsDTO()
                    {
                        TransactionId = src.TransactionId.ToString(),
                        Status = src.DealStatus,
                        AgreementType =
                            src.AgreementType == "RENTAL"
                                ? AgreementType.RentalApplication
                                : AgreementType.LoanApplication
                    };
                    return details;
                }))
                .ForMember(d => d.Equipment, s => s.ResolveUsing(src =>
                {
                    var equipment = new EquipmentInfoDTO()
                    {
                        Id = 0,
                        LoanTerm = src.Term,
                        RequestedTerm = src.Term,
                        ValueOfDeal = (double) src.AmountFinanced,
                        AgreementType =
                            src.AgreementType == "RENTAL"
                                ? AgreementType.RentalApplication
                                : AgreementType.LoanApplication,
                        NewEquipment = new List<NewEquipmentDTO>()
                        {
                            new NewEquipmentDTO()
                            {
                                Id = 0,
                                Description = src.EquipmentDescription,
                                Type = src.EquipmentType,
                            }
                        }
                    };
                    return equipment;
                }))
                .ForMember(d => d.PrimaryCustomer, s => s.ResolveUsing(src =>
                {
                    var primaryCustomer = new CustomerDTO()
                    {
                        Id = 0,
                        AccountId = src.CustomerAccountId,
                        LastName = src.CustomerLastName,
                        FirstName = src.CustomerFirstName,
                    };
                    return primaryCustomer;
                }))
                .ForMember(d => d.DealerId, s => s.Ignore())
                .ForMember(d => d.ContractState, s => s.Ignore())
                .ForMember(d => d.ExternalSubDealerName, s => s.Ignore())
                .ForMember(d => d.ExternalSubDealerId, s => s.Ignore())
                .ForMember(d => d.SecondaryCustomers, s => s.Ignore())
                .ForMember(d => d.PaymentInfo, s => s.Ignore())
                .ForMember(d => d.Comments, s => s.Ignore())
                .ForMember(d => d.Documents, s => s.Ignore())
                .ForMember(d => d.WasDeclined, s => s.Ignore())
                .ForMember(d => d.IsCreatedByCustomer, s => s.Ignore())
                .ForMember(d => d.OnCreditReview, s => s.Ignore())
                .ForMember(d => d.IsNewlyCreated, s => s.Ignore());

            mapperConfig.CreateMap<Aspire.Integration.Models.AspireDb.Entity, CustomerDTO>()
                .ForMember(d => d.Id, s => s.UseValue(0))
                .ForMember(d => d.AccountId, s => s.MapFrom(src => src.EntityId))
                .ForMember(d => d.Emails, s => s.ResolveUsing(src =>
                {
                    List<EmailDTO> emails = null;
                    if (!string.IsNullOrEmpty(src.EmailAddress))
                    {
                        emails = new List<EmailDTO>()
                        {
                            new EmailDTO()
                            {
                                EmailType = EmailType.Main,
                                EmailAddress = src.EmailAddress
                            }
                        };
                    }
                    return emails;
                }))
                .ForMember(d => d.Locations, s => s.ResolveUsing(src =>
                {
                    if (!string.IsNullOrEmpty(src.PostalCode))
                    {
                        var locations = new List<LocationDTO>()
                        {
                            new LocationDTO()
                            {
                                AddressType = AddressType.MainAddress,
                                City = src.City,
                                State = src.State,
                                PostalCode = src.PostalCode,
                                ResidenceType = ResidenceType.Own,
                                Street = src.Street
                            }
                        };
                        return locations;
                    }
                    return null;
                }))
                .ForMember(d => d.Phones, s => s.ResolveUsing(src =>
                {
                    if (!string.IsNullOrEmpty(src.PhoneNum))
                    {
                        var phones = new List<PhoneDTO>()
                        {
                            new PhoneDTO()
                            {
                                PhoneNum = src.PhoneNum,
                                PhoneType = PhoneType.Home
                            }
                        };
                        return phones;
                    }
                    return null;
                }))
                .ForMember(d => d.Sin, s => s.Ignore())
                .ForMember(d => d.DriverLicenseNumber, s => s.Ignore())
                .ForMember(d => d.AllowCommunicate, s => s.Ignore())
                .ForMember(d => d.IsHomeOwner, s => s.Ignore())
                .ForMember(d => d.IsInitialCustomer, s => s.Ignore())
                .ForMember(d => d.PreferredContactMethod, s => s.Ignore());
                
            mapperConfig.CreateMap<Aspire.Integration.Models.AspireDb.Entity, DealerDTO>()
                .IncludeBase<Entity, CustomerDTO>()
                .ForMember(d => d.ParentDealerUserName, s => s.MapFrom(src => src.ParentUserName))
                .ForMember(d => d.FirstName, s => s.MapFrom(src => src.FirstName ?? src.Name))
                .ForMember(d => d.PreferredContactMethod, s => s.Ignore())
                .ForMember(d => d.ProductType, s => s.Ignore())
                .ForMember(d => d.ChannelType, s => s.Ignore())
                .ForMember(d => d.Role, s => s.Ignore())
                .ForMember(d => d.Ratecard, s => s.Ignore());

            mapperConfig.CreateMap<Aspire.Integration.Models.AspireDb.DealerRoleEntity, DealerDTO>()
                .IncludeBase<Entity, DealerDTO>()
                .ForMember(d => d.ParentDealerUserName, s => s.MapFrom(src => src.ParentUserName))
                .ForMember(d => d.FirstName, s => s.MapFrom(src => src.FirstName ?? src.Name))
                .ForMember(d => d.PreferredContactMethod, s => s.Ignore())
                .ForMember(d => d.ProductType, s => s.MapFrom(src => src.ProductType))
                .ForMember(d => d.ChannelType, s => s.MapFrom(src => src.ChannelType))
                .ForMember(d => d.Role, s => s.MapFrom(src => src.Role))
                .ForMember(d => d.Ratecard, s => s.MapFrom(src => src.Ratecard));
        }

        private static void MapModelsToDomains(IMapperConfigurationExpression mapperConfig)
        {
            mapperConfig.CreateMap<LocationDTO, Location>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<PhoneDTO, Phone>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<EmailDTO, Email>()
                .ForMember(x => x.Customer, s => s.Ignore());
            mapperConfig.CreateMap<ContractDetailsDTO, ContractDetails>();
            mapperConfig.CreateMap<EquipmentTypeDTO, EquipmentType>()
                .ForMember(x => x.DescriptionResource, s => s.Ignore());
            mapperConfig.CreateMap<EquipmentInfoDTO, EquipmentInfo>()
                .ForMember(d => d.Contract, x => x.Ignore())
                .ForMember(d => d.ValueOfDeal, x => x.Ignore())
                .ForMember(d => d.Notes, x => x.Ignore());
            mapperConfig.CreateMap<NewEquipmentDTO, NewEquipment>()
                .ForMember(x => x.EquipmentInfo, d => d.Ignore())
                .ForMember(x => x.EquipmentInfoId, d => d.Ignore());
            mapperConfig.CreateMap<ExistingEquipmentDTO, ExistingEquipment>()
                .ForMember(x => x.EquipmentInfo, d => d.Ignore())
                .ForMember(x => x.EquipmentInfoId, d => d.Ignore());
            mapperConfig.CreateMap<CommentDTO, Comment>()
               .ForMember(x => x.ParentComment, d => d.Ignore())
               .ForMember(x => x.Contract, d => d.Ignore())
               .ForMember(x => x.Replies, d => d.Ignore())
               .ForMember(d => d.Dealer, s => s.Ignore());
            mapperConfig.CreateMap<CustomerDTO, Customer>()
                .ForMember(x => x.Sin, s => s.ResolveUsing(src => string.IsNullOrWhiteSpace(src.Sin) ? null : src.Sin))
                .ForMember(x => x.DriverLicenseNumber, s => s.ResolveUsing(src => string.IsNullOrWhiteSpace(src.DriverLicenseNumber) ? null : src.DriverLicenseNumber))
                .ForMember(x => x.PreferredContactMethod, s => s.MapFrom(m => m.PreferredContactMethod))
                .ForMember(x => x.AccountId, d => d.Ignore());

            mapperConfig.CreateMap<CustomerInfoDTO, Customer>()
                .ForMember(x => x.AccountId, d => d.Ignore())
                .ForMember(x => x.Locations, d => d.Ignore())
                .ForMember(x => x.Emails, d => d.Ignore())
                .ForMember(x => x.Phones, d => d.Ignore());

            mapperConfig.CreateMap<ContractDataDTO, ContractData>()
                .ForMember(x => x.HomeOwners, d => d.Ignore())
                .AfterMap((d, c, rc) =>
                {                    
                    if (d?.PrimaryCustomer?.IsHomeOwner == true || (d?.SecondaryCustomers?.Any(sc => sc.IsHomeOwner == true) ?? false))
                    {
                        c.HomeOwners = new List<Customer>();
                        if (d?.PrimaryCustomer?.IsHomeOwner == true)
                        {
                            c.HomeOwners.Add(c.PrimaryCustomer);
                        }
                        d?.SecondaryCustomers?.Where(sc => sc.IsHomeOwner == true).ForEach(sc =>
                        {
                            c.HomeOwners.Add(rc.Mapper.Map<Customer>(sc));
                        });
                    }
                });
            mapperConfig.CreateMap<PaymentInfoDTO, PaymentInfo>()
                .ForMember(d => d.Contract, s => s.Ignore());            
            mapperConfig.CreateMap<ContractDTO, Contract>()
                .ForMember(d => d.PaymentInfo, s => s.Ignore())
                .ForMember(d => d.Comments, s => s.Ignore())
                .ForMember(x => x.Documents, d => d.Ignore())
                .ForMember(x => x.Dealer, d => d.Ignore())
                .ForMember(x => x.HomeOwners, d => d.Ignore())
                .ForMember(x => x.InitialCustomers, d => d.Ignore())
                .ForMember(x => x.CreateOperator, d => d.Ignore())
                .ForMember(x => x.LastUpdateOperator, d => d.Ignore())
                .ForMember(x => x.IsCreatedByBroker, d => d.Ignore());

            mapperConfig.CreateMap<AgreementTemplateDTO, AgreementTemplate>()
                .ForMember(d => d.AgreementForm, s => s.MapFrom(src => src.AgreementFormRaw))
                .ForMember(d => d.Dealer, s => s.Ignore())
                .ForMember(d => d.DocumentTypeId, s => s.Ignore())
                .ForMember(d => d.DocumentType, s => s.Ignore())
                .ForMember(d => d.ApplicationId, s => s.Ignore())
                .ForMember(d => d.Application, s => s.Ignore());
                //.ForMember(d => d.EquipmentTypes, s => s.Ignore());

            mapperConfig.CreateMap<DocumentTypeDTO, DocumentType>()
                .ForMember(x => x.DescriptionResource, d => d.Ignore());
            mapperConfig.CreateMap<ContractDocumentDTO, ContractDocument>()
                .ForMember(x => x.Contract, d => d.Ignore())
                .ForMember(x => x.DocumentType, d => d.Ignore());

            mapperConfig.CreateMap<CustomerLinkDTO, CustomerLink>()                
                .ForMember(x => x.EnabledLanguages, d => d.ResolveUsing(src =>
                    src.EnabledLanguages?.Select(l => new DealerLanguage() {LanguageId = (int)l, Language = new Language() { Id = (int)l } }).ToList()))
                .ForMember(x => x.Services, d => d.ResolveUsing(src =>
                    src.Services?.SelectMany(ds => ds.Value.Select(dsv => new DealerService() {LanguageId = (int)ds.Key, Service = dsv}))))
                .ForMember(x => x.Id, d => d.Ignore())
                .ForMember(x => x.HashLink, d => d.MapFrom(s=>s.HashLink));
            mapperConfig.CreateMap<DealerEquipmentDTO, DealerEquipment>()
                .ForMember(x => x.EquipmentId, d => d.ResolveUsing(src => src.Equipment.Id))
                .ForMember(x => x.DealerProfile, d => d.Ignore())
                .ForMember(x => x.Id, d => d.Ignore());
            mapperConfig.CreateMap<DealerAreaDTO, DealerArea>()
                .ForMember(x => x.DealerProfile, d => d.Ignore());
            mapperConfig.CreateMap<DealerProfileDTO, DealerProfile>()
                .ForMember(x => x.Id, d => d.MapFrom( src => src.Id))
                .ForMember(x => x.DealerId, d => d.MapFrom( src => src.DealerId))
                .ForMember(x => x.Equipments, d => d.MapFrom( src => src.EquipmentList.Select( s=> new DealerEquipment() {EquipmentId = s.Equipment.Id, ProfileId = src.Id})))
                .ForMember(x => x.Areas, d => d.MapFrom( src => src.PostalCodesList.Select(s => new DealerArea() {ProfileId = src.Id, PostalCode = s.PostalCode})))
                .ForMember(x => x.Dealer, d => d.Ignore());
           
        }
    }
}