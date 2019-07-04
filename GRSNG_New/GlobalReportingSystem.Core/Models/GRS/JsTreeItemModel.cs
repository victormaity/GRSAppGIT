using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class JsTreeItemModel
    {
        //public string title { get; set; }
        //public string isFolder { get; set; }
        //public string key { get; set; }
        //public string expand { get; set; }
        //public string isLazy { get; set; }
        //public List<DynatreeItemModel> children { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }
        public HtmlAttribute li_attr { get; set; }
        public string a_attr { get; set; }
        public bool children { get; set; }
    }

    public class State
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
    }
    public class HtmlAttribute
    {
        public string title { get; set; }
    }
}
