using System;
using DealnetPortal.Api.Common.Constants;

namespace DealnetPortal.Api.Helpers
{
    using System.Text;
    using System.Threading.Tasks;

    public class SecurityHelper
    {
        public static async Task<string> GeneratePasswordAsync()
        {
            var length = Convert.ToInt32(new Utilities.Configuration.AppConfiguration().GetSetting(WebConfigKeys.ES_SMTPPASSLENGTH_CONFIG_KEY));
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+\\/.,?|{}[]=";
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return await Task.FromResult(res.ToString());
        }
    }
}