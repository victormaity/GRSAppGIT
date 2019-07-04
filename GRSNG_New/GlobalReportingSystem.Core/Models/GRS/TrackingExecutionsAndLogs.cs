using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TrackingExecutionsAndLogs
    {
        public Int64 RelExeID { get; set; }
        public string ProjectName { get; set; }
        public string CycleName { get; set; }
        public string Tests { get; set; }
        public string ExecutionStarted { get; set; }
        public string LatestStatusCheckedAt { get; set; }
        public string CurrentStatus { get; set; }
        public string LastRowCount { get; set; }
        public string LastRowId { get; set; }
        public string NewRowCount { get; set; }
        public string NewRowId { get; set; }
        public string TestSuitIds { get; set; }
    }
}
