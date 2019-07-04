using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class LoadBalancedViewModel
    {
        public List<NewLoadBalancedItem> NewLoadBalancedList { get; set; }
        public Project ProjectInfo { get; set; }
        public List<string> Categories { get; set; }
        public bool Single { get; set; }
    }

    public class NewLoadBalancedItem
    {
        public NewLoadBalanced NewLoadBalanced { get; set; }
        public bool Single { get; set; }
    }
}