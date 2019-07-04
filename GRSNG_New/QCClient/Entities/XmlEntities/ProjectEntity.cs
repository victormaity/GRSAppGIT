using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class Projects : Serializer<Projects>
    {
        [XmlElement("Project")]
        public ProjectsProject[] Project { get; set; }
    }


    public class ProjectsProject
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}