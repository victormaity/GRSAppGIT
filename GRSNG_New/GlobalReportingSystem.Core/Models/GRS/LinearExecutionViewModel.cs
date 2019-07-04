using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class LinearExecutionViewModel
    {
        public List<TestsExecutions> TestsExecutions { get; set; }
        public ProjectInfo ProjectInfo { get; set; }
        //public Project ProjectInfo { get; set; }
    }

    public class ProjectInfo
    {
        public Project Project { get; set; }
        public List<string> Users { get; set; }
        public List<vw_FilesStorage> FilesStorages { get; set; }
    }

    public class TestsExecutions
    {
        public TestsExecution TestsExecution { get; set; }
        public vw_FilesStorage FilesStorage { get; set; }
    }
}