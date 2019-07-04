using System.IO;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract.BL.Helper;

namespace GlobalReportingSystem.BL.Helper
{
    public class Serializer<T> : ISerializer<T>
        where T : class
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
            var serializer = new XmlSerializer(typeof (T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }

        public string Serialize(T item)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, item);
                return writer.ToString();
            }
        }
    }
}