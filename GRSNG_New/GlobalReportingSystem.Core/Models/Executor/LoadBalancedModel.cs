using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class LoadBalancedModel
    {
        public List<LoadBalancedElement> Elements { get; set; }
    }

    public class LoadBalancedElement
    {
        public List<Machine> Machines { get; set; }
        public List<Test> Tests { get; set; }
        public string Framework { get; set; }
        public Guid Id { get; set; }
        public int ProjectId { get; set; }
    }

    public class Machine
    {
        public int ID { get; set; }
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public int Account { get; set; }
        public string Browser { get; set; }
        public int Profile { get; set; }
        public string Include { get; set; }
        public string Exclude { get; set; }
        public string Priority { get; set; }
        public int Configuration { get; set; }
        public int? TargetTestCycle { get; set; }
        public int? LoginPage { get; set; }
    }

    public class Test
    {
        public int ID { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public string Ip { get; set; }
        public string Status { get; set; }
    }
}