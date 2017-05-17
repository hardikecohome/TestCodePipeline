using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DealnetPortal.DataAccess.Repositories
{
    public class DealerRepository : BaseRepository, IDealerRepository
    {
        public DealerRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public string GetParentDealerId(string dealerId)
        {
            return base.GetUserById(dealerId).ParentDealerId;
        }

        public string GetUserIdByName(string userName)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserName == userName)?.Id;
        }

        public IList<string> GetUserRoles(string dealerId)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_dbContext));
            return userManager.GetRoles(dealerId);
        }

        public string GetDealerNameByCustomerLinkId(int customerLinkId)
        {
            return _dbContext.Users
                .FirstOrDefault(u => u.CustomerLinkId == customerLinkId)?.UserName;
        }
    }

    
}
