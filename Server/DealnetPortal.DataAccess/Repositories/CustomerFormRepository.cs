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

                    updatedLink = new CustomerLink()
                    {
                        User = user
                    };
                    user.CustomerLink = updatedLink;
                }

                updatedLink.EnabledLanguages.Where(l => enabledLanguages.All(el => el.Id != l.Id)).ForEach(l => updatedLink.EnabledLanguages.Remove(l));
                var dbLangs = _dbContext.Languages.Where(dbl => enabledLanguages.Any(el => el.Id == dbl.Id));
                dbLangs.ForEach(dl => updatedLink.EnabledLanguages.Add(dl));                
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

                    updatedLink = new CustomerLink()
                    {
                        User = user
                    };
                    user.CustomerLink = updatedLink;
                }

                updatedLink.Services.Where(
                    s => dealerServices.All(dl => dl.LanguageId != s.LanguageId && dl.Service != s.Service))
                    .ForEach(l => updatedLink.Services.Remove(l));
                var newServices =
                    dealerServices.Where(
                        ds => updatedLink.Services.All(s => s.LanguageId != ds.LanguageId && s.Service != ds.Service));
                newServices.ForEach(ns => updatedLink.Services.Add(ns));
                return updatedLink;
            }
            return null;
        }        
    }
}
