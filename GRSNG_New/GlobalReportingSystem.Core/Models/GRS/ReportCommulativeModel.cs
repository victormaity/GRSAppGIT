using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ReportCommulativeModel
    {
        public int TestSiuteId { get; set; }
        public string TestCycleName { get; set; }
        public string TestSiuteName { get; set; }
        public Browser Browser { get; set; }
        public List<string> Bugs { get; set; }
        public string Bugtracker { get; set; }
        public TimeSpan Time { get; set; }
        public string Tester { get; set; }
        public string Comments { get; set; }
    }

    public class Browser
    {
        public string Name { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int NotCompleted { get; set; }
        public int Analyzed { get; set; }
        public int NotAnalyzed { get; set; }
        public List<CustomStatus> CustomStatuses { get; set; }
        public List<string> AvailableStatuses { get; set; }
        public List<string> AvailableStatusesIds { get; set; }
    }

    public class CustomStatus
    {
        public string Name { get; set; }
        public int Status { get; set; }
    }
}