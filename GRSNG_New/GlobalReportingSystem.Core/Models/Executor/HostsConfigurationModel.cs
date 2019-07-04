using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class HostsConfigurationModel
    {
        public int ID { get; set; }
        public string EnvironmentName { get; set; }
        public string HostFileContent { get; set; }
        public string ApplicationURL { get; set; }
        public bool isBackDoor { get; set; }
    }
}
