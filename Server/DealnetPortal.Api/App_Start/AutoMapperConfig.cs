using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Helpers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

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
            mapperConfig.CreateMap<ApplicationUser, ApplicationUserDTO>()
                .ForMember(x => x.SubDealers, o => o.Ignore())
                .ForMember(x => x.UdfSubDealers, d => d.Ignore());
            mapperConfig.CreateMap<Location, LocationDTO>();
            mapperConfig.CreateMap<Phone, PhoneDTO>()
                .ForMember(x => x.CustomerId, o => o.MapFrom(src => src.Customer != null ? src.Customer.Id : 0));
            mapperConfig.CreateMap<Email, EmailDTO>()
                .ForMember(x => x.CustomerId, o => o.MapFrom(src => src.Customer != null ? src.Customer.Id : 0));
            mapperConfig.CreateMap<EquipmentInfo, EquipmentInfoDTO>()
                .ForMember(x=>x.RateCardId, o => o.MapFrom(d=>d.RateCard.Id));
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
                    d =>
                        d.ResolveUsing(
                            src => src.EnabledLanguages?.Select(l => l.LanguageId).Cast<LanguageCode>().ToList()))
                .ForMember(x => x.HashLink, d => d.MapFrom(s => s.HashLink))
                .ForMember(x => x.Services,
                    d =>
                        d.ResolveUsing(
                            src =>
                                src.Services?.GroupBy(k => k.LanguageId)
                                    .ToDictionary(ds => (LanguageCode) ds.Key, ds => ds.Select(s => s.Service).ToList())));
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
                .ForMember(x => x.LastUpdateOperator, d => d.Ignore());

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
        }
    }
}