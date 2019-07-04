using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TestCasesAndProject
    {
        public TestCaseAndAnalysis TestCaseAndAnalysis { get; set; }
        public int ProjectID { get; set; }
        public string Bugtracker { get; set; }
    }

    public class TestCaseAndAnalysis
    {
        public TestCase TestCase { get; set; }
        public List<TestStepAndAnalysis> TestStepAndAnalysis { get; set; }
    }

    public class TestStepAndAnalysis
    {
        public TestStep TestStep { get; set; }
        public string Status { get; set; }
        public string CurrentDefects { get; set; }
        public string OldDefects { get; set; }
        public int SortOrder { get; set; }
    }
}