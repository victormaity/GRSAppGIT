using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class LinearExecutionLaunchModel
    {
        public List<TestsExecutions> TestsExecutions { get; set; }
        public ProjectDetails ProjectDetails { get; set; }
    }

    public class ProjectDetails
    {
        public bool IsFrameworkSynced { get; set; }
        public bool IsJava { get; set; }
        public List<string> Users { get; set; }
        public Project Project { get; set; }
    }
}
