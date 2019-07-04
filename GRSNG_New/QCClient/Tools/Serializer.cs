using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace QcClient.Tools
{
    public class Serializer<T> where T : class
    {
        public T Deserialize(string inputXml)
        {
            using (TextReader reader = new StringReader(inputXml))
            {
                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }

        }
        public string Serialize()
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this, null);
                return writer.ToString();
            }
        }
    }
}