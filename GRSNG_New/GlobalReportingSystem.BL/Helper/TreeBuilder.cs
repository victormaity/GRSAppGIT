using System.Collections.Generic;
using System.Linq;
using GlobalReportingSystem.Core.Abstract.BL.Helper;

namespace GlobalReportingSystem.BL.Helper
{
    public class Node : Serializer<GlobalReportingSystem.Core.Models.GRS.DB.Node>
    {
      /*  public string Value { get; set; }

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
        }*/
    }

    public class TreeBuilder
    {
        private static GlobalReportingSystem.Core.Models.GRS.DB.Node FindFirstNodeWithValue(GlobalReportingSystem.Core.Models.GRS.DB.Node root, string val)
        {
            if (root.Value == val)
                return root;
            foreach (var child in root.Children)
            {
                var res = FindFirstNodeWithValue(child, val);
                if (res != null)
                    return res;
            }
            return null;
        }
        public static GlobalReportingSystem.Core.Models.GRS.DB.Node BuildTree(IEnumerable<string> tokens, char[] sep)
        {
            var root = new GlobalReportingSystem.Core.Models.GRS.DB.Node(null, null);
            foreach (var token in tokens)
            {

        #if OLDVERSION
        var current = root;
        var splits = token.Split(sep);
        #else
                var splits = token.Split(sep);
                var current = FindFirstNodeWithValue(root, splits.First());
                if (current == null)
                    current = root;
                else
                    current = current.Parent;
        #endif

                for (int i = 0; i < splits.Length; i++)
                {
                    string split = splits[i];
                    var node = current.Children.FirstOrDefault(x => x.Value == split);
                    if (node == null)
                    {
                        node = new GlobalReportingSystem.Core.Models.GRS.DB.Node(split, current);
                        current.Children.Add(node);
                    }
                    current = node;
                }
            }
            return root;
        }
    }
}