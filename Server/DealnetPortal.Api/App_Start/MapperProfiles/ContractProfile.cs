using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Configuration;
using Unity.Interception.Utilities;

namespace DealnetPortal.Api.App_Start.MapperProfiles
{
    public class ContractProfile : Profile
    {
        public ContractProfile()
        {
            ConfigureMappers();
        }

        private void ConfigureMappers()
        {
            var configuration = new AppConfiguration(WebConfigSections.AdditionalSections);
            var creditReviewStates = configuration.GetSetting(WebConfigKeys.CREDIT_REVIEW_STATUS_CONFIG_KEY)?.Split(',').Select(s => s.Trim()).ToArray();
            var riskBasedStatus = configuration.GetSetting(WebConfigKeys.RISK_BASED_STATUS_KEY)?.Split(',').Select(s => s.Trim()).ToArray();
            var hidePreapprovalAmountForLeaseDealers = false;
            bool.TryParse(configuration.GetSetting(WebConfigKeys.HIDE_PREAPPROVAL_AMOUNT_FOR_LEASEDEALERS_KEY),
                out hidePreapprovalAmountForLeaseDealers);
            var leaseType = "Lease";

            CreateMap<Contract, ContractShortInfoDTO>()
                .ForMember(x => x.TransactionId, s => s.ResolveUsing(src => src.Details?.TransactionId))
                .ForMember(x => x.AgreementType, s => s.ResolveUsing(src =>
                    src.Details.AgreementType ??
                              ((src.Equipment?.AgreementType != null && src.Equipment?.ValueOfDeal != null
                                && (src.Equipment.LoanTerm != null || src.Equipment.AmortizationTerm != null ||
                                    src.Equipment.RequestedTerm != null))
                            ? src.Equipment.AgreementType
                            : (AgreementType?)null)

                ))
                .ForMember(x => x.Status, s => s.MapFrom(src => src.Details.Status))
                .ForMember(x => x.SignatureStatus, s => s.MapFrom(src => src.Details.SignatureStatus))
                .ForMember(x => x.SignatureStatusLastUpdateTime, s => s.MapFrom(src =>
                    src.Details.SignatureLastUpdateTime))
                .ForMember(d => d.LocalizedStatus, s => s.ResolveUsing(src => !string.IsNullOrEmpty(src.Details.Status) ?
                    ResourceHelper.GetGlobalStringResource("_" + src.Details.Status
                                                               .Replace('-', '_')
                                                               .Replace(" ", string.Empty)
                                                               .Replace("$", string.Empty)
                                                               .Replace("/", string.Empty)
                                                               .Replace("(", string.Empty)
                                                               .Replace(")", string.Empty)) ?? src.Details.Status : null))
                .ForMember(d => d.PrimaryCustomerFirstName, s => s.ResolveUsing(src => src.PrimaryCustomer?.FirstName))
                .ForMember(d => d.PrimaryCustomerLastName, s => s.ResolveUsing(src => src.PrimaryCustomer?.LastName))
                .ForMember(d => d.PrimaryCustomerEmail,
                    s => s.ResolveUsing(src =>
                        src.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress))
                .ForMember(d => d.PrimaryCustomerPhone, s => s.ResolveUsing(src =>
                    src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Cell)?.PhoneNum
                    ?? src.PrimaryCustomer?.Phones?.FirstOrDefault(e => e.PhoneType == PhoneType.Home)?.PhoneNum))
                .ForMember(d => d.PrimaryCustomerPostalCode, s => s.ResolveUsing(src =>
                    src.PrimaryCustomer?.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.PostalCode
                    ?? src.PrimaryCustomer?.Locations?.FirstOrDefault()?.PostalCode))
                .ForMember(d => d.PrimaryCustomerAddress, s => s.ResolveUsing(src =>
                {
                    var location =
                        src.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress)
                        ?? src.PrimaryCustomer?.Locations?.FirstOrDefault();
                    return location != null ? $"{location.Street} {location.Unit} {location.City} {location.State} {location.PostalCode}" : string.Empty;
                }
                ))
                .ForMember(d => d.CustomerComments, s => s.ResolveUsing(src =>
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
                }))
                .ForMember(d => d.CustomerComments, s => s.Ignore())
                .ForMember(d => d.LastUpdateTime, s => s.ResolveUsing(src => src.LastUpdateTime ?? src.CreationTime))
                .ForMember(d => d.Equipment, s => s.ResolveUsing((src, dest, destMember, resContext) =>
                {
                    var equipment = src.Equipment?.NewEquipment;
                    if (equipment != null)
                    {
                        var equipmentTypes = resContext.Items.ContainsKey(MappingKeys.EquipmentTypes)
                            ? resContext.Items[MappingKeys.EquipmentTypes] as IList<EquipmentType>
                            : null;
                        return equipment.Select(eq =>
                        {
                            var eqType = eq.EquipmentType ??
                                         equipmentTypes?.FirstOrDefault(
                                             et => et.Type == eq.Type);
                            return (!string.IsNullOrEmpty(eqType?.DescriptionResource)
                                       ? ResourceHelper.GetGlobalStringResource(eqType.DescriptionResource)
                                       : eqType?.Description)
                                   ?? eqType?.Description;
                        }).ConcatWithComma();
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.ValueOfDeal, s => s.ResolveUsing(src =>
                {
                    if (src.Equipment != null && src.Equipment.ValueOfDeal != null &&
                        (src.Equipment.LoanTerm != null || src.Equipment.AmortizationTerm != null ||
                         src.Equipment.RequestedTerm != null))
                    {
                        return src.Equipment.ValueOfDeal;
                    }
                    return null;
                }))
                .ForMember(d => d.PreApprovalAmount, s => s.MapFrom(src => src.Details.CreditAmount))
                .ForMember(d => d.LoanTerm, s => s.ResolveUsing(src => src.Equipment?.LoanTerm))
                .ForMember(d => d.AmortizationTerm, s => s.ResolveUsing(src => src.Equipment?.AmortizationTerm))
                .ForMember(d => d.MonthlyPayment, s => s.ResolveUsing(src => src.Equipment?.TotalMonthlyPayment))
                .ForMember(d => d.PaymentType, s => s.ResolveUsing(src => src.PaymentInfo?.PaymentType))
                .ForMember(d => d.SalesRep, s => s.ResolveUsing(src => src.Equipment?.SalesRep ?? string.Empty))
                .ForMember(d => d.IsNewlyCreated, s => s.ResolveUsing(src => src.IsNewlyCreated ?? false))
                .ForMember(d => d.IsCreatedByCustomer, s => s.ResolveUsing(src => src.IsCreatedByCustomer ?? false))
                .ForMember(d => d.CreatedBy, s => s.ResolveUsing(src => src.Dealer?.UserName ?? src.CreateOperator))
                .ForMember(d => d.ActionRequired, s => s.ResolveUsing((src, dest, destMember, resContext) =>
                {
                    if (src.ContractState != ContractState.Completed)
                    {
                        return string.Empty;
                    }
                    var cProvince = src.PrimaryCustomer?.Locations
                                        .FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State
                                    ?? src.PrimaryCustomer?.Locations.FirstOrDefault()?.State;
                    var provincesDocTypes = resContext.Items.ContainsKey(MappingKeys.DocumentTypes) ? resContext.Items[MappingKeys.DocumentTypes] as IDictionary<string, IList<DocumentType>> : null;
                    if (!string.IsNullOrEmpty(cProvince) && provincesDocTypes != null)
                    {
                        var docTypes = provincesDocTypes[cProvince];
                        var absentDocs =
                            docTypes?.Where(
                                    dt => dt.IsMandatory && src.Documents.All(d => dt.Id != d.DocumentTypeId) &&
                                          (dt.Id != (int)DocumentTemplateType.SignedContract || src.Details?.SignatureStatus != SignatureStatus.Completed))
                                .ToList();
                        if (absentDocs?.Any() ?? false)
                        {
                            var actList = new StringBuilder();
                            absentDocs.ForEach(dt => actList.AppendLine($"{dt.Description};"));
                            return actList.ToString();
                        }
                    }
                    return string.Empty;
                }))
                .ForMember(d => d.ProgramOption, s => s.Ignore())
                .ForMember(d => d.SearchDescription, s => s.ResolveUsing(src =>
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
                .AfterMap((c, d) =>
                {
                    if (d.AgreementType.HasValue)
                    {
                        if(d.AgreementType == AgreementType.RentalApplicationHwt)
                        {
                            d.ProgramOption = Resources.Resources.LeaseApplicationHwt;
                        }
                        if (d.AgreementType == AgreementType.RentalApplication)
                        {
                            d.ProgramOption = Resources.Resources.LeaseApplication;
                        }
                        else if (c.Equipment?.IsClarityProgram == true)
                        {
                            d.ProgramOption = Resources.Resources.ClarityProgram;
                        }
                        else if (d.AgreementType == AgreementType.LoanApplication && c.Equipment?.RateCard != null)
                        {
                            switch (c.Equipment.RateCard.CardType)
                            {
                                case RateCardType.Custom:
                                    d.ProgramOption = Resources.Resources.Custom;
                                    break;
                                case RateCardType.FixedRate:
                                    d.ProgramOption = c.Equipment?.RateReductionCardId != null ? Resources.Resources.RateReduction : Resources.Resources.StandardRate;
                                    break;
                                case RateCardType.NoInterest:
                                    d.ProgramOption = Resources.Resources.EqualPayments;
                                    break;
                                case RateCardType.Deferral:
                                    d.ProgramOption = $"{Convert.ToInt32(c.Equipment.RateCard.DeferralPeriod)} {Resources.Resources.Months} {Resources.Resources.Deferral}";
                                    break;
                            }
                        }
                    }

                    if ((c.Dealer?.Tier?.IsCustomerRisk == true || c.IsCreatedByBroker == true || c.IsCreatedByCustomer == true)
                        && (!hidePreapprovalAmountForLeaseDealers || c.Dealer?.DealerType != leaseType) && c.Details.CreditAmount > 0 && riskBasedStatus?.Any() == true)
                    {
                        if (riskBasedStatus.Contains(c.Details.Status))
                        {
                            if (CultureInfo.CurrentCulture.Name == "fr")
                            {
                                d.LocalizedStatus += $" {(double)(c.Details.CreditAmount ?? 0m)}";
                            }
                            else
                            {
                                d.LocalizedStatus += $" {(double)(c.Details.CreditAmount ?? 0m) / 1000} K";
                            }
                        }
                    }

                }
                );
        }
    }
}