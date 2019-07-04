using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Security.Policy;
using System.Xml.Serialization;

namespace GlobalReportingSystem
{
    [Serializable]
    // [XmlRoot("NunitTests"), XmlType("NunitTests")]
    public class NunitTests
    {
        public string FullName { get; set; }
        public List<TestFixture> TestFixtures { get; set; }
        public NunitTests Deserialize(string inputXML)
        {
            //XmlRootAttribute xRoot = new XmlRootAttribute();
            //xRoot.ElementName = "NunitTests";           
            //xRoot.IsNullable = true;
            using (TextReader reader = new StringReader(inputXML))
            {
                XmlSerializer xs = new XmlSerializer(typeof(NunitTests));
                return (NunitTests)xs.Deserialize(reader);
            }

        }
        public string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(NunitTests));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }


    }
    [Serializable]
    public class TestFixture
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> Tests { get; set; }
        public List<string> Categories { get; set; }
    }
}
