using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace GRSExecutor.Support
{
    public class Serializer<T> where T : class
    {
        public T Deserialize(string inputXML)
        {
            using (TextReader reader = new StringReader(inputXML))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }

        }
        public string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}