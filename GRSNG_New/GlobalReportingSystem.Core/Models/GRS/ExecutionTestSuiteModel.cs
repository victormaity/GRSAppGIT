using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract.BL.Helper;


namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionTestSuiteModel : Serializer<ExecutionTestSuiteModel>
    {
        public ExecutionTestSuiteModel()
        {
            ExecutionTestSuites = new List<ExecutionTestSuite>();
        }
        public bool IsJava { get; set; }
        public List<ExecutionTestSuite> ExecutionTestSuites { get; set; } 
    }

    public class ExecutionTestSuite
    {
        public ExecutionTestSuite()
        {
            Categories = new List<string>();
            Tests = new List<string>();
        }

        public string Namespace { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tests { get; set; }

    }

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
