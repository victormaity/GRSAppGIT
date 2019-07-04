using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ItemId
    {
        public string Item { get; set; }
        public int Id { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
    }
}