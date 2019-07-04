using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS.PublicAccess
{
    public class FullConfigurationModel
    {
        public Dictionary<string, string> BrowserList { get; set; }
        public List<HostsConfiguration> HostsConfigurations { get; set; }
        public List<AccountForTestRun> AccountForTestRuns { get; set; }
        public List<ExecutionConfiguration> ExecutionConfigurations { get; set; }
    }
}
