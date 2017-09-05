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
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.Models.MyProfile;
using DealnetPortal.Web.Models.Enumeration;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;
using ContractState = DealnetPortal.Web.Models.Enumeration.ContractState;

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
                    .ForMember(x => x.DateOfBirth, d => d.MapFrom(src => src.BirthDate))
                    .ForMember(x => x.Sin, d => d.ResolveUsing(src =>
                    {
                        if (src.Sin == null)
                        {
                            return src.Sin;
                        }

                        return src.Sin.Trim().Length > 0 ? src.Sin.Trim() : null;
                    }))
                    .ForMember(x => x.DriverLicenseNumber, d => d.ResolveUsing(src =>
                    {
                        if (src.DriverLicenseNumber == null)
                        {
                            return src.DriverLicenseNumber;
                        }

                        return src.DriverLicenseNumber.Trim().Length > 0 ? src.DriverLicenseNumber.Trim() : null;
                    }))
                    .ForMember(x => x.AllowCommunicate, d => d.Ignore())
                    .ForMember(x => x.IsInitialCustomer, d => d.Ignore())
                    .ForMember(x => x.IsHomeOwner, d => d.ResolveUsing(src => src?.IsHomeOwner == true))
                    .ForMember(x => x.AccountId, d => d.Ignore());

            cfg.CreateMap<AddressInformation, LocationDTO>()
                .ForMember(x => x.Unit, d => d.MapFrom(src => src.UnitNumber))
                .ForMember(x => x.State, d => d.MapFrom(src => src.Province))
                .ForMember(x => x.AddressType, d => d.Ignore())
                .ForMember(x => x.Id, d => d.Ignore())
                .ForMember(x => x.CustomerId, d => d.Ignore());

            cfg.CreateMap<EquipmentInformationViewModel, EquipmentInfoDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.ContractId ?? 0))
                .ForMember(x => x.ValueOfDeal, d => d.Ignore())
                .ForMember(x => x.InstallationDate, d => d.Ignore())
                .ForMember(x => x.InstallerFirstName, d => d.Ignore())
                .ForMember(x => x.InstallerLastName, d => d.Ignore())
                .ForMember(x => x.DeferralType, d => d.ResolveUsing(src => src.AgreementType == AgreementType.LoanApplication ? src.LoanDeferralType.ConvertTo<DeferralType>() : src.RentalDeferralType.ConvertTo<DeferralType>()));
            cfg.CreateMap<NewEquipmentInformation, NewEquipmentDTO>()
                .ForMember(x => x.TypeDescription, d => d.Ignore())
                .ForMember(x => x.AssetNumber, d => d.Ignore())
                .ForMember(x => x.InstalledSerialNumber, d => d.Ignore())
                .ForMember(x => x.InstalledModel, d => d.Ignore());
            cfg.CreateMap<ExistingEquipmentInformation, ExistingEquipmentDTO>();

            cfg.CreateMap<PaymentInfoViewModel, PaymentInfoDTO>().ForMember(x => x.Id, d => d.Ignore());

            cfg.CreateMap<CommentViewModel, CommentDTO>()
                .ForMember(x => x.DealerId, d => d.Ignore());

            cfg.CreateMap<ApplicantPersonalInfo, CustomerDataDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.CustomerId))
                .ForMember(x => x.CustomerInfo, d => d.Ignore())
                .ForMember(x => x.Phones, d => d.Ignore())
                .ForMember(x => x.Emails, d => d.Ignore())
                .ForMember(x => x.Locations, d => d.Ignore());

            cfg.CreateMap<ContactInfoViewModel, CustomerDataDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.CustomerId))
                .ForMember(x => x.ContractId, d => d.Ignore())
                .ForMember(x => x.Locations, d => d.Ignore())
                .ForMember(x => x.Phones, d => d.ResolveUsing(src =>
                {
                    List<PhoneDTO> phones = new List<PhoneDTO>();
                    
                    if (!string.IsNullOrEmpty(src.HomePhone))
                    {
                        if (src.HomePhone.Trim().Any())
                        {
                            phones.Add(new PhoneDTO()
                            {
                                CustomerId = src.CustomerId,
                                PhoneNum = src.HomePhone,
                                PhoneType = PhoneType.Home
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(src.BusinessPhone))
                    {
                        if (src.BusinessPhone.Trim().Any())
                        {
                            phones.Add(new PhoneDTO()
                            {
                                CustomerId = src.CustomerId,
                                PhoneNum = src.BusinessPhone,
                                PhoneType = PhoneType.Business
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(src.CellPhone))
                    {
                        if (src.CellPhone.Trim().Any())
                        {
                            phones.Add(new PhoneDTO()
                            {
                                CustomerId = src.CustomerId,
                                PhoneNum = src.CellPhone,
                                PhoneType = PhoneType.Cell
                            });
                        }
                    }
                    return phones.Any() ? phones : null;
                }))
                .ForMember(x => x.Emails, d => d.ResolveUsing(src => new List<EmailDTO>()
                {
                    new EmailDTO()
                    {
                        CustomerId = src.CustomerId,
                        EmailType = EmailType.Main,
                        EmailAddress = src.EmailAddress
                    }
                }))
                .ForMember(x => x.CustomerInfo, d => d.ResolveUsing(src =>
                new CustomerInfoDTO()
                {
                    Id = src.CustomerId,
                    AllowCommunicate = src.AllowCommunicate                    
                }));

            cfg.CreateMap<RegisterViewModel, RegisterBindingModel>();
            cfg.CreateMap<ChangePasswordAnonymouslyViewModel, ChangePasswordAnonymouslyBindingModel>();
            cfg.CreateMap<ForgotPasswordViewModel, ForgotPasswordBindingModel>();
            cfg.CreateMap<CertificateEquipmentInfoViewModel, InstalledEquipmentDTO>();
            cfg.CreateMap<CertificateInformationViewModel, InstallationCertificateDataDTO>()
                .ForMember(x => x.InstalledEquipments, d => d.MapFrom(src => src.Equipments));

            cfg.CreateMap<ShareableLinkViewModel, CustomerLinkDTO>()
                .ForMember(x => x.EnabledLanguages, d => d.ResolveUsing(src =>
                {
                    if (!src.EnglishLinkEnabled && !src.FrenchLinkEnabled)
                    {
                        return null;
                    }
                    var enabledLanguages = new List<LanguageCode>();
                    if (src.EnglishLinkEnabled)
                    {
                        enabledLanguages.Add(LanguageCode.English);
                    }
                    if (src.FrenchLinkEnabled)
                    {
                        enabledLanguages.Add(LanguageCode.French);
                    }
                    return enabledLanguages;
                }))
                .ForMember(x => x.Services, d => d.ResolveUsing(src =>
                {
                    if (src.EnglishServices == null && src.FrenchServices == null)
                    {
                        return null;
                    }
                    var services = new Dictionary<LanguageCode, List<string>>();
                    if (src.EnglishServices != null)
                    {
                        services.Add(LanguageCode.English, src.EnglishServices);
                    }
                    if (src.FrenchServices != null)
                    {
                        services.Add(LanguageCode.French, src.FrenchServices);
                    }
                    return services;
                }))
                .ForMember(x=>x.HashLink, d=> d.MapFrom(s=>s.HashDealerName));
            cfg.CreateMap<ProfileViewModel, DealerProfileDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.ProfileId))
                .ForMember(x => x.EquipmentList, d => d.ResolveUsing(src => src.DealerEquipments!=null ? src.DealerEquipments.Select(s=> new DealerEquipmentDTO() {Equipment = s}).ToList() : null))
                .ForMember(d => d.PostalCodesList, d => d.MapFrom(src => src.PostalCodes));
            cfg.CreateMap<DealerAreaViewModel, DealerAreaDTO>();

            cfg.CreateMap<CustomerContactInfoViewModel, CustomerDTO>()
                .ForMember(x => x.PreferredContactMethod, d => d.MapFrom(src => src.PreferredContactMethod));

            cfg.CreateMap<NewCustomerViewModel, NewCustomerDTO>()
                .ForMember(x => x.CustomerComment, d => d.MapFrom(src => src.CustomerComment))
                .ForMember(x => x.HomeImprovementTypes, d => d.MapFrom(src => src.HomeImprovementTypes))
                .ForMember(x => x.PrimaryCustomer, d => d.MapFrom(src => src.HomeOwnerContactInfo));
            
            cfg.CreateMap<EquipmentInformationViewModelNew, EquipmentInfoDTO>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.ContractId ?? 0))
                .ForMember(x => x.ValueOfDeal, d => d.Ignore())
                .ForMember(x => x.InstallationDate, d => d.Ignore())
                .ForMember(x => x.InstallerFirstName, d => d.Ignore())
                .ForMember(x => x.InstallerLastName, d => d.Ignore())
                .ForMember(x => x.DownPayment, d => d.MapFrom(s => s.DownPayment ?? 0))
                .ForMember(x => x.RateCardId, s=>s.MapFrom( d=>d.SelectedRateCardId))
                .ForMember(x => x.DeferralType, d => d.ResolveUsing(src => src.AgreementType == AgreementType.LoanApplication ? src.LoanDeferralType.ConvertTo<DeferralType>() : src.RentalDeferralType.ConvertTo<DeferralType>()));

            cfg.CreateMap<AddressInformation, AddressDTO>()
                .ForMember(x => x.State, d => d.MapFrom(src => src.Province));

            cfg.CreateMap<ProductInfoViewModel, ProductInfoDTO>()
                .ForMember(x => x.ServiceTypes, d => d.MapFrom(src => src.EquipmentTypes));

            cfg.CreateMap<CompanyInfoViewModel, CompanyInfoDTO>();

            cfg.CreateMap<OwnerViewModel, OwnerInfoDTO>()
                .ForMember(x => x.DateOfBirth, d => d.MapFrom(src => src.BirthDate))
                .ForMember(x => x.MobilePhone, d => d.MapFrom(src => src.CellPhone));

            cfg.CreateMap<DealerOnboardingViewModel, DealerInfoDTO>()
                .ForMember(x => x.SalesRepLink, d => d.MapFrom(src => src.OnBoardingLink))
                .ForMember(x => x.MarketingConsent, d => d.MapFrom(src => src.AllowCommunicate))
                .ForMember(x => x.CreditCheckConsent, d => d.MapFrom(src => src.AllowCreditCheck))
                ;
        }

        private static void MapModelsToVMs(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ApplicationUserDTO, SubDealer>()
                .ForMember(x => x.DisplayName, o => o.MapFrom(src => src.UserName));
            cfg.CreateMap<SubDealerDTO, SubDealer>()
                .ForMember(x => x.Id, o => o.MapFrom(src => src.SubmissionValue))
                .ForMember(x => x.DisplayName, o => o.MapFrom(src => src.SubDealerName));
            cfg.CreateMap<DriverLicenseData, RecognizedLicense>();
            cfg.CreateMap<Tuple<DriverLicenseData, IList<Alert>>, DriverLicenseViewModel>()
                .ForMember(x => x.DriverLicense, o => o.MapFrom(src => src.Item1))
                .ForMember(x => x.RecognitionErrors, o => o.ResolveUsing(src =>
                src.Item2.Select(e => e.Header).ToList()));

            cfg.CreateMap<ContractDTO, ClientsInformationViewModel>()
                .ForMember(d => d.TransactionId,
                    s => s.ResolveUsing(src => src.Details?.TransactionId ??
                                               string.Concat(Resources.Resources.Internal, $" : {src.Id}")))
                .ForMember(d => d.IsInternal, s => s.ResolveUsing(src => src.Details?.TransactionId == null))
                .ForMember(d => d.Improvement, s => s.ResolveUsing(src =>
                {
                    var equipment = src.Equipment?.NewEquipment;
                    if (equipment != null)
                    {
                        return equipment.Select(eq => eq.TypeDescription).ConcatWithComma();
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.Client, s => s.ResolveUsing(src =>
                {
                    var customer = src.PrimaryCustomer;
                    if (customer != null)
                    {
                        return $"{customer.FirstName} {customer.LastName}";
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.Status,
                    s => s.ResolveUsing(src => src.Details?.Status ??
                                               (src.ContractState.ConvertTo<ContractState>()).GetEnumDescription()))
                .ForMember(d => d.PaymentType,
                    s => s.ResolveUsing(src => src.PaymentInfo?.PaymentType.ConvertTo<Models.Enumeration.PaymentType>()
                        .GetEnumDescription()))
                .ForMember(d => d.Action, s => s.Ignore())
                .ForMember(d => d.Email,
                    s => s.ResolveUsing(src => src.PrimaryCustomer?.Emails
                        ?.FirstOrDefault(e => e.EmailType == EmailType.Main)
                        ?.EmailAddress))
                .ForMember(d => d.Phone, s => s.ResolveUsing(
                    src => src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Cell)?.PhoneNum
                           ?? src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Home)
                               ?.PhoneNum))
                .ForMember(d => d.Date, s => s.ResolveUsing(src =>
                    (src.LastUpdateTime?.Date ?? src.CreationTime.Date).ToString("MM/dd/yyyy",
                        CultureInfo.InvariantCulture)))
                .ForMember(d => d.SalesRep, s => s.ResolveUsing(src => src.Equipment?.SalesRep ?? string.Empty))
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
                    });
                    stb.AppendLine(src.Equipment?.SalesRep);
                    stb.AppendLine(src.PaymentInfo?.EnbridgeGasDistributionAccount);
                    stb.AppendLine(src.PaymentInfo?.AccountNumber);
                    return stb.ToString();
                }))
                .ForMember(d => d.Value, s => s.ResolveUsing(src =>
                {
                    if (src.Equipment != null)
                    {
                        return FormattableString.Invariant($"$ {src.Equipment.ValueOfDeal:0.00}");
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.PreApprovalAmount, s => s.ResolveUsing(src =>
                {
                    if (src.Details?.CreditAmount != null)
                    {
                        return FormattableString.Invariant($"$ {src.Details.CreditAmount:0.00}");
                    }
                    return string.Empty;
                }));
                

            cfg.CreateMap<ContractDTO, DealItemOverviewViewModel>()
                .ForMember(d => d.TransactionId, s => s.ResolveUsing(src => src.Details?.TransactionId ?? string.Concat(Resources.Resources.Internal, $" : {src.Id}")))
                .ForMember(d => d.IsInternal, s => s.ResolveUsing(src => src.Details?.TransactionId == null ))
                .ForMember(d => d.CustomerName, s => s.ResolveUsing(src =>
                {
                    var customer = src.PrimaryCustomer;
                    if (customer != null)
                    {
                        return $"{customer.FirstName} {customer.LastName}";
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.Status, s => s.ResolveUsing(src => src.Details?.Status ?? (src.ContractState.ConvertTo<ContractState>()).GetEnumDescription()))
                .ForMember(d => d.AgreementType, s => s.ResolveUsing(src =>
                {
                    if (src.Equipment?.AgreementType != null && src.Equipment?.ValueOfDeal != null)
                    {
                        return src.Equipment?.AgreementType.ConvertTo<AgreementType>()
                            .GetEnumDescription();
                    }

                    return string.Empty;
                }))
                .ForMember(d => d.PaymentType, s => s.ResolveUsing(src => src.PaymentInfo?.PaymentType.ConvertTo<Models.Enumeration.PaymentType>().GetEnumDescription() ))
                .ForMember(d => d.Action, s => s.Ignore())
                .ForMember(d => d.Email, s => s.ResolveUsing(src => src.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress))                
                .ForMember(d => d.Phone, s => s.ResolveUsing(src => src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Cell)?.PhoneNum
                    ?? src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Home)?.PhoneNum))                
                .ForMember(d => d.Date, s => s.ResolveUsing(src =>
                    (src.LastUpdateTime?.Date ?? src.CreationTime.Date).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))
                .ForMember(d => d.SalesRep, s => s.ResolveUsing(src => src.Equipment?.SalesRep ?? string.Empty))
                .ForMember(d => d.Equipment, s => s.ResolveUsing(src =>
                    {
                        var equipment = src.Equipment?.NewEquipment;
                        if (equipment != null)
                        {
                            return equipment.Select(eq => eq.TypeDescription).ConcatWithComma();
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
                        });
                        stb.AppendLine(src.Equipment?.SalesRep);
                        stb.AppendLine(src.PaymentInfo?.EnbridgeGasDistributionAccount);
                        stb.AppendLine(src.PaymentInfo?.AccountNumber);
                        return stb.ToString();
                    }))
                    .ForMember(d => d.Value, s => s.ResolveUsing(src =>
                    {
                        if (src.Equipment != null && src.Equipment.ValueOfDeal != null)
                        {
                            return FormattableString.Invariant($"$ {src.Equipment.ValueOfDeal:0.00}");
                        }
                        return string.Empty;
                    }))
                    .ForMember(d => d.PostalCode, s => s.ResolveUsing(src =>
                    {
                        var location =
                            src.PrimaryCustomer?.Locations?.FirstOrDefault(x => x.AddressType == AddressType.MainAddress);
                        if (location?.PostalCode != null)
                        {
                            var substring = location.PostalCode.Substring(0, 3);
                            return $"{substring.ToUpperInvariant()}***";
                        }             
                        return string.Empty;
                    }))
                    .ForMember(d => d.PreApprovalAmount, s => s.ResolveUsing(src =>
                    {
                        if (src.Details?.CreditAmount != null)
                        {
                            return FormattableString.Invariant($"$ {src.Details.CreditAmount:0.00}");
                        }
                        return string.Empty;
                    }))
                    .ForMember(d => d.CustomerComment, s => s.ResolveUsing(src => 
                    {
                        if (src.Comments?.Any(x => x.IsCustomerComment == true) == true)
                        {
                            var comments = src.Comments
                                .Where(x => x.IsCustomerComment == true)
                                .Select(q => q.Text)
                                .ToList();

                            return string.Join(Environment.NewLine, comments);
                        }

                        return string.Empty;
                    }));

            cfg.CreateMap<CustomerDTO, ApplicantPersonalInfo>()
                .ForMember(x => x.BirthDate, d => d.MapFrom(src => src.DateOfBirth))
                .ForMember(x => x.CustomerId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.AddressInformation, d => d.MapFrom(src => src.Locations.FirstOrDefault(x => x.AddressType == AddressType.MainAddress)))
                .ForMember(x => x.MailingAddressInformation, d => d.MapFrom(src => src.Locations.FirstOrDefault(x => x.AddressType == AddressType.MailAddress)))
                .ForMember(x => x.PreviousAddressInformation, d => d.MapFrom(src => src.Locations.FirstOrDefault(x => x.AddressType == AddressType.PreviousAddress)))
                .ForMember(x => x.IsHomeOwner, d => d.ResolveUsing(src => src?.IsHomeOwner == true))
                .ForMember(x => x.ContractId, d => d.Ignore());
            cfg.CreateMap<LocationDTO, AddressInformation>()
                .ForMember(x => x.UnitNumber, d => d.MapFrom(src => src.Unit))
                .ForMember(x => x.Province, d => d.MapFrom(src => src.State));

            cfg.CreateMap<NewEquipmentDTO, NewEquipmentInformation>();
            cfg.CreateMap<ExistingEquipmentDTO, ExistingEquipmentInformation>();

            cfg.CreateMap<EquipmentInfoDTO, EquipmentInformationViewModel>()
                .ForMember(x => x.ContractId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.ProvinceTaxRate, d => d.Ignore())
                .ForMember(x => x.CreditAmount, d => d.Ignore())
                .ForMember(x => x.LoanDeferralType, d => d.ResolveUsing(src => src.AgreementType == Api.Common.Enumeration.AgreementType.LoanApplication ? src.DeferralType : 0))
                .ForMember(x => x.RentalDeferralType, d => d.ResolveUsing(src => src.AgreementType != Api.Common.Enumeration.AgreementType.LoanApplication ? src.DeferralType : 0))
                .ForMember(x => x.EstimatedInstallationDate, d => d.ResolveUsing(src => src.EstimatedInstallationDate ?? ((src.NewEquipment?.Any() ?? false) ? src.NewEquipment.First().EstimatedInstallationDate : DateTime.Today) ))
                .ForMember(x => x.FullUpdate, d => d.Ignore())
                .ForMember(x => x.IsAllInfoCompleted, d => d.Ignore())
                .ForMember(x => x.IsApplicantsInfoEditAvailable, d => d.Ignore())
                .ForMember(x => x.Notes, d => d.Ignore());

            cfg.CreateMap<CommentDTO, CommentViewModel>();
            cfg.CreateMap<ContractDocumentDTO, ExistingDocument>()
            .ForMember(x => x.DocumentId, d => d.MapFrom(src => src.Id))
            .ForMember(x => x.DocumentName, d => d.ResolveUsing(src => src.DocumentName.Substring(src.DocumentName.IndexOf("_") + 1)))
            .ForMember(x => x.LastUpdateTime, d => d.MapFrom(src => src.CreationDate));

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
                    src.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress))
                .ForMember(x => x.AllowCommunicate, d => d.ResolveUsing(src => src.AllowCommunicate ?? false));

            cfg.CreateMap<CustomerLinkDTO, ShareableLinkViewModel>()
                .ForMember(x => x.EnglishLinkEnabled, d => d.ResolveUsing(src => src.EnabledLanguages?.Contains(LanguageCode.English) ?? false))
                .ForMember(x => x.FrenchLinkEnabled, d => d.ResolveUsing(src => src.EnabledLanguages?.Contains(LanguageCode.French) ?? false))
                .ForMember(x => x.EnglishServices, d => d.ResolveUsing(src =>
                {
                    List<string> services = null;
                    src.Services?.TryGetValue(LanguageCode.English, out services);
                    return services;
                }))
                .ForMember(x => x.FrenchServices, d => d.ResolveUsing(src =>
                {
                    List<string> services = null;
                    src.Services?.TryGetValue(LanguageCode.French, out services);
                    return services;
                }))
                .ForMember(x=>x.HashDealerName, d=>d.MapFrom(s=>s.HashLink));
            cfg.CreateMap<DealerProfileDTO, ProfileViewModel>()
                .ForMember(x => x.ProfileId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.DealerEquipments, d => d.ResolveUsing(src => src.EquipmentList?.Select(x=> x.Equipment)))
                .ForMember(d => d.PostalCodes, d => d.MapFrom(src => src.PostalCodesList));
            cfg.CreateMap<DealerAreaDTO, DealerAreaViewModel>();

            //New Version
            cfg.CreateMap<EquipmentInfoDTO, EquipmentInformationViewModelNew>()
                .ForMember(x => x.ContractId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.DownPayment, d => d.MapFrom(src => src.DownPayment == 0 ? null : src.DownPayment))
                .ForMember(x => x.SelectedRateCardId, d => d.MapFrom(o => o.RateCardId))
                .ForMember(x => x.ProvinceTaxRate, d => d.Ignore())
                .ForMember(x => x.CreditAmount, d => d.Ignore())
                .ForMember(x => x.LoanDeferralType, d => d.ResolveUsing(src => src.AgreementType == Api.Common.Enumeration.AgreementType.LoanApplication ? src.DeferralType.ConvertTo<LoanDeferralType>() : 0))
                .ForMember(x => x.RentalDeferralType, d => d.ResolveUsing(src => src.AgreementType != Api.Common.Enumeration.AgreementType.LoanApplication ? src.DeferralType : 0))
                .ForMember(x => x.FullUpdate, d => d.Ignore())
                .ForMember(x => x.IsAllInfoCompleted, d => d.Ignore())
                .ForMember(x => x.IsApplicantsInfoEditAvailable, d => d.Ignore());

            cfg.CreateMap<AddressDTO, AddressInformation>()
                .ForMember(x => x.Province, d => d.MapFrom(src => src.State));

            cfg.CreateMap<ProductInfoDTO, ProductInfoViewModel>()
                .ForMember(x => x.EquipmentTypes, d => d.MapFrom(src => src.ServiceTypes));

            cfg.CreateMap<CompanyInfoDTO, CompanyInfoViewModel>();

            cfg.CreateMap<OwnerInfoDTO, OwnerViewModel>()
                .ForMember(x => x.CellPhone, d => d.MapFrom(src => src.MobilePhone))
                .ForMember(x => x.BirthDate, d => d.MapFrom(src => src.DateOfBirth));

            cfg.CreateMap<DealerInfoDTO, DealerOnboardingViewModel>()
                .ForMember(x => x.OnBoardingLink, d => d.MapFrom(src => src.SalesRepLink))
                .ForMember(x => x.AllowCommunicate, d => d.MapFrom(src => src.MarketingConsent))
                .ForMember(x => x.AllowCreditCheck, d => d.MapFrom(src => src.CreditCheckConsent))
                ;
        }
    }
}