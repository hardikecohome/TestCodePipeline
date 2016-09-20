using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class XmlSerializerHelper
    {
        public static T DeserializeFromString<T>(string text)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(text))
            {
                return (T)ser.Deserialize(reader);
            }
        }

        public static async Task<T> DeserializeFromStringAsync<T>(this HttpContent content)
        {
            var contentStr = await content.ReadAsStringAsync();
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(contentStr))
            {
                return (T)ser.Deserialize(reader);
            }
        }
    }
}
