using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionEnvironmentsModel
    {
        public IEnumerable<HostsConfiguration> Environments { get; set; }
        public HostsConfiguration Environment { get; set; } 
    }
}