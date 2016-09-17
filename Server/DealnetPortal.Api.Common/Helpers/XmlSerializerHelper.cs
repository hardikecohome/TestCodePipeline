using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DealnetPortal.Api.Common.Helpers
{
    public class XmlSerializerHelper
    {
        public T DeserializeFromString<T>(string text)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(text))
            {
                return (T)ser.Deserialize(reader);
            }
        }
    }
}
