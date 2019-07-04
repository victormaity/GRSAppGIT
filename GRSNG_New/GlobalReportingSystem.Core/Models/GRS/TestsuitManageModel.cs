using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TestsuitManageModel
    {
        public Int64 ID { get; set; }
        public string TestsuitName { get; set; }
        public string StartTime { get; set; }
        public string TotalFailedTestcase { get; set; }
        public string TotalPassedTestcase { get; set; }
        public string TestcycleName { get; set; }
        public string clientIP { get; set; }
        public string ClientEnvironment { get; set; }
        public string clientUser { get; set; }
    }

    public class ManageTestSuits
    {
        public string ProjectDisplayName { get; set; }
        public List<TestsuitManageModel> TestsuitManageModel { get; set; }
    }

    public class AttachmentDeleteByProjectIdAndName
    {
        public int ProjectId { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
