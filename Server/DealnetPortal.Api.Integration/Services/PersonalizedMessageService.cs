using DealnetPortal.Api.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Integration.Services
{
    public class PersonalizedMessageService : IPersonalizedMessageService
    {
        public static string _endPoint { get; set; }
        public static string _apiKey { get; set; }

        public PersonalizedMessageService(string endPoint, string apiKey)
        {
            _endPoint = endPoint;
            _apiKey = apiKey;
        }


        public async Task<HttpResponseMessage> SendMessage(string phonenumber, string messagebody)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_endPoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IMWS1", "key = " + _apiKey);
                client.DefaultRequestHeaders.Add("X-Impact-Fail-Fast", "false");
                client.DefaultRequestHeaders.Add("X-Impact-Response-Detail", "standard");
                RequestMessage request = new RequestMessage();
                request.messages.Add(new Message()
                {
                    content = new Content() { body = messagebody },
                    sendDate = DateTime.Now,
                    validUntil = DateTime.Now.AddMinutes(5),
                    to = new To() { subscriber = new Subscriber() { phone = phonenumber } },
                    tracking = new Tracking() { code = "try123" }
                });

                return await client.PostAsJsonAsync("media/ws/rest/mbox/v1/reference/ECOHOME_MRE/message", request);

            }
        }
    }
}
