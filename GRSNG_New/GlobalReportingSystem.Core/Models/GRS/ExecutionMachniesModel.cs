using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionMachniesModel
    {
        public IEnumerable<Client> Machines{ get; set; }
        public Client Client { get; set; } 
    }
}