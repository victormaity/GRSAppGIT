using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace QcClient.Entities.XmlEntities
{
    public class Field
    {
        [XmlAttribute("Name")]
        public String Name { get; set; }

        [XmlElement]
        public String Value { get; set; }

        public static String GetField(IEnumerable<Field> fields, String target)
        {
            var result = fields.FirstOrDefault(i => i.Name == target);
            return result == null ? String.Empty : result.Value;
        }
    }
}
