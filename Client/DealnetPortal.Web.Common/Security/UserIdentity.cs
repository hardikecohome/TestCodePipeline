using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Security
{
    public class UserIdentity : ClaimsIdentity
    {
        public UserIdentity(IEnumerable<Claim> claims) : base(claims, "OWIN")
        { }

        public UserIdentity() : base("OWIN")
        { }

        /// <summary>
        /// Bearer token
        /// </summary>
        public string Token { set; get; }
        
        public override string Name
        {
            get
            {

                var nameclaim = Claims.FirstOrDefault(t => t.Type == ClaimTypes.Name);

                return nameclaim == null ? null : nameclaim.Value;
            }
        }
    }
}
