using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TestStepAndProject
    {
        public List<SubStepAndCustomStatus> TestStepAndCustomStatus { get; set; }
        public TestStep TestStep { get; set; }
        public int ProjectId { get; set; }
        public string Bugtracker { get; set; }
        public List<string> CustomStatusesList { get; set; }
    }

    public class SubStepAndCustomStatus
    {
        public SubStep SubStep { get; set; }
        public string CustomStatus { get; set; }
    }
}