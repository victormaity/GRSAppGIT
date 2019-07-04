using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionConfigurationsModel
    {
        public IEnumerable<ExecutionConfiguration> Configurations { get; set; }
        public ExecutionConfiguration Configuration { get; set; } 
    }
}