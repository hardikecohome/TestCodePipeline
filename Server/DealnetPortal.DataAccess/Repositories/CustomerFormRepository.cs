﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Repositories
{
    public class CustomerFormRepository : BaseRepository, ICustomerFormRepository
    {
        public CustomerFormRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public CustomerLink GetCustomerLinkSettings(string dealerId)
        {
            var user = _dbContext.Users
                .Include(u => u.CustomerLink).AsNoTracking()
                .FirstOrDefault(u => u.Id == dealerId);
            return user?.CustomerLink;
        }

        public CustomerLink GetCustomerLinkSettingsByDealerName(string dealerName)
        {
            var user = _dbContext.Users
                .Include(u => u.CustomerLink).AsNoTracking()
                .FirstOrDefault(u => u.UserName == dealerName);
            return user?.CustomerLink;
        }

        public CustomerLink UpdateCustomerLinkLanguages(ICollection<DealerLanguage> enabledLanguages, string dealerId)
        {
            var user = _dbContext.Users
                .Include(u => u.CustomerLink)
                .FirstOrDefault(u => u.Id == dealerId);
            if (user != null)
            {
                var updatedLink = user.CustomerLink;
                if (updatedLink == null)
                {

                    updatedLink = new CustomerLink();
                    user.CustomerLink = updatedLink;
                }

                updatedLink.EnabledLanguages.Where(l => enabledLanguages.All(el => el.LanguageId != l.LanguageId)).ToList().ForEach(l => _dbContext.DealerLanguages.Remove(l));
                enabledLanguages.Where(el => updatedLink.EnabledLanguages.All(ul => ul.LanguageId != el.LanguageId)).ForEach(el =>
                {
                    var lang = _dbContext.Languages.Find(el.LanguageId);
                    if (lang != null)
                    {
                        updatedLink.EnabledLanguages.Add(new DealerLanguage()
                        {
                            CustomerLink = updatedLink,
                            CustomerLinkId = updatedLink.Id,
                            Language = lang,
                            LanguageId = lang.Id
                        });
                    }
                });
                return updatedLink;
            }
            return null;
        }

        public CustomerLink UpdateCustomerLinkServices(ICollection<DealerService> dealerServices,
            string dealerId)
        {
            var user = _dbContext.Users
               .Include(u => u.CustomerLink)
               .FirstOrDefault(u => u.Id == dealerId);
            if (user != null)
            {
                var updatedLink = user.CustomerLink;
                if (updatedLink == null)
                {

                    updatedLink = new CustomerLink();
                    user.CustomerLink = updatedLink;
                }

                updatedLink.Services.Where(
                    s => dealerServices.All(dl => dl.LanguageId != s.LanguageId || dl.Service != s.Service)).ToList()
                    .ForEach(l => _dbContext.DealerServices.Remove(l));
                var newServices =
                    dealerServices.Where(
                        ds => updatedLink.Services.All(
                            s => 
                        s.LanguageId != ds.LanguageId || s.Service != ds.Service));
                newServices.ForEach(ns => updatedLink.Services.Add(ns));
                return updatedLink;
            }
            return null;
        }

        public Contract AddCustomerContractData(int contractId, string selectedService, string customerComment,
            string dealerId)
        {
            var contract = _dbContext.Contracts.Find(contractId);
            if (contract != null)
            {
                var dealerSettings = GetCustomerLinkSettings(dealerId);
                DealerService service = null;
                if (dealerSettings != null)
                {
                    service = dealerSettings.Services.FirstOrDefault(s => s.Service == selectedService);
                }
                if (service != null || !string.IsNullOrEmpty(customerComment))
                {
                    var notes = new StringBuilder();
                    if (service != null)
                    {
                        notes.AppendLine(service.Service);
                    }
                    if (!string.IsNullOrEmpty(customerComment))
                    {
                        notes.AppendLine(customerComment);
                    }

                    if (contract.Equipment != null)
                    {
                        contract.Equipment.Notes = notes.ToString();
                    }
                    else
                    {
                        var eqInfo = new EquipmentInfo()
                        {
                            Notes = notes.ToString(),
                            ExistingEquipment = new List<ExistingEquipment>(),
                            NewEquipment = new List<NewEquipment>(),
                            Contract = contract,                            
                        };
                        _dbContext.EquipmentInfo.Add(eqInfo);
                    }                    
                }
            }
            return contract;
        }         
    }
}
