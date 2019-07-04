using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS.DB
{
    public class Node
    {
        public Node()
        {
        }
        public string Value { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public Node Parent { get; set; }

        public List<Node> Children { get; set; }
        public Node(string c, Node parent)
        {
            this.Value = c;
            this.Parent = parent;
            this.Children = new List<Node>();
        }
        public override string ToString()
        {
            return "Value: " + this.Value;
        }
    }
}
