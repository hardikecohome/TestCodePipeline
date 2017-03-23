using System;
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

        public CustomerLink UpdateCustomerLinkLanguages(ICollection<Language> enabledLanguages, string dealerId)
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

                updatedLink.EnabledLanguages.Where(l => enabledLanguages.All(el => el.Id != l.Id)).ToList().ForEach(l => updatedLink.EnabledLanguages.Remove(l));
                enabledLanguages.Where(el => updatedLink.EnabledLanguages.All(ul => ul.Id != el.Id)).ForEach(el =>
                {
                    var lang = _dbContext.Languages.Find(el.Id);
                    if (lang != null)
                    {
                        updatedLink.EnabledLanguages.Add(lang);
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

        public CustomerContractInfo AddCustomerContractData(CustomerContractInfo customerContractInfo)
        {
            throw new NotImplementedException();
        }

        public Contract CreateContractForDealer(string dealerName)
        {
            throw new NotImplementedException();
        }

    }
}
