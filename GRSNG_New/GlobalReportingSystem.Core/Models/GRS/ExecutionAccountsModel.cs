using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionAccountsModel
    {
        public IEnumerable<AccountForTestRun> AccountForTestRun { get; set; }
        public AccountForTestRun Account { get; set; } 
    }
}