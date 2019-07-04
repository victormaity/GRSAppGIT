using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TestCasesAndStepsModel
    {
        public int ProjectID { get; set; }
        public int TestSuiteID { get; set; }
        public string TSuitName { get; set; }
        public DateTime TSuitStart { get; set; }
        public List<TestCasesModel> TestCases { get; set; }
        public int TotalCount { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public int NotCompetedCount { get; set; }
        public int NoRunCount { get; set; }
        public int AnalyzedCount { get; set; }
        public int NotAnalyzedCount { get; set; }
        public List<TestSuit> DedicatedSuits { get; set; }
        public int? DedicatedLinearExecution { get; set; }
        public int? DedicatedLoadBalancedExecution { get; set; }
    }

    public class TestCasesModel
    {
        public string TCName { get; set; }
        public DateTime TCEndTime { get; set; }
        public string TCDescription { get; set; }
        public DateTime TCStartTime { get; set; }
        public int ID { get; set; }
        public bool isAnalyzed { get; set; }
        public string TCState { get; set; }
        public string qc_id { get; set; }

        public List<TestStep> TestSteps { get; set; }
    }
}
