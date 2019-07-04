using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class JenkinsReportCountStatus
    {
        public string emailStatus { get; set; }
        public int totalTestCaseCount { get; set; }
        public int totalPassCount { get; set; }
        public int totalFailCount { get; set; }
        public int totalNoRunCount { get; set; }
        public int totalNotCompletedCount { get; set; }

    }
}
