using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class Report
    {
        public List<TestSuite> TestSuiteField { get; set; }
    }

    public class TestSuite
    {
        public string QuickView { get; set; }
        public string TestSuiteName { get; set; }
        public string RunTime { get; set; }
        public string Total { get; set; }
        public string Pass { get; set; }
        public string Fail { get; set; }
        public string Blocker { get; set; }
        public string NoRun { get; set; }
    }
}
