using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class BugListModel
    {
        public string BugID { get; set; }
        public string BugUrl { get; set; }
        public int BugCount { get; set; }
        public int TCycleId { get; set; }
    }

    public class TestCaseDetails
    {
        public string TCName { get; set; }
        public string TCDescription { get; set; }
        public string TestSuiteName { get; set; }
        public string qc_id { get; set; }
        public string bugId { get; set; }
    }
}
