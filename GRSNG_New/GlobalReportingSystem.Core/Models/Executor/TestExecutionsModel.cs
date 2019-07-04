using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class TestExecutionsModel
    {
        public int ID { get; set; }
        public int? BelongToProject { get; set; }
        public int? Client { get; set; }
        public string Tests { get; set; }
        public int? Account { get; set; }
        public int? ProfileHost { get; set; }
        public string Browser { get; set; }
        public Guid? RemoteExecutionLink { get; set; }
        public string Comments { get; set; }
        public int? FrameworkVersion { get; set; }
        public int? Configuration { get; set; }
        public string AccaptanceCriteria { get; set; }
        public string CategoriesInclude { get; set; }
        public string CategoriesExclude { get; set; }
        public int? FailsToAlert { get; set; }
        public string Priroty { get; set; }
        public string ExecutionType { get; set; }
        public string Scheduling { get; set; }
        public int? TargetTestCycle { get; set; }
        public int? LoginPageURL { get; set; }
    }
}
