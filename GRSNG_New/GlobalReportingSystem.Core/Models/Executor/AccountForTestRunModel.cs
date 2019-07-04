using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class AccountForTestRunModel
    {
        public int ID { get; set; }
        public string AccountName { get; set; }
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public string Comments { get; set; }
    }
}