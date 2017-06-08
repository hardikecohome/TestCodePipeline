using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DealnetPortal.Domain
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        public string AspireLogin { get; set; }
        public string AspirePassword { get; set; }
        public string AspireAccountId { get; set; }

        public string Company { get; set; }

        public string DisplayName { get; set; }

        public bool EsignatureEnabled { get; set; }

        public string Culture { get; set; }

        public virtual ICollection<ApplicationUser> SubDealers { get; set; }

        public string ParentDealerId { get; set; }
        [ForeignKey("ParentDealerId")]
        public virtual ApplicationUser ParentDealer { get; set; }

        public int? UserSettingsId { get; set; }
        [ForeignKey("UserSettingsId")]
        public UserSettings Settings { get; set; }

        public int? CustomerLinkId { get; set; }
        [ForeignKey("CustomerLinkId")]
        public virtual CustomerLink CustomerLink { get; set; }

        public int? DealerProfileId { get; set; }

        public int? TierId { get; set; }
        [ForeignKey("TierId")]
        public virtual Tier Tier { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here            

            return userIdentity;
        }
    }
}