using System.Collections.Generic;
using Newtonsoft.Json;

namespace DealnetPortal.Web.Models
{
    public class RecaptchaResponseModel
    {
        [JsonProperty(PropertyName = "success")]
        public string Success { get; set; }
        [JsonProperty(PropertyName = "challenge_ts")]
        public string ChallengeTimeStamp { get; set; }
        [JsonProperty(PropertyName = "hostname")]
        public string HostName { get; set; }
        [JsonProperty(PropertyName = "error-code")]
        public IEnumerable<string> ErrorCodes { get; set; }
    }
}
