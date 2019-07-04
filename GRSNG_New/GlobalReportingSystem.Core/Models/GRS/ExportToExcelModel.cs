using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExportToExcelModel
    {
        public string Subject { get; set; }
        public string TestName { get; set; }
        public string Description { get; set; }
        //public string Steps { get; set; }
        public string BusinessCriticality { get; set; }
        public string CurrentStatus { get; set; }
        public string GlobalStatus { get; set; }
        public string CurrentDefect { get; set; }
        public string OldDefect { get; set; }
        public string ClientIP { get; set; }
        public string EndTime { get; set; }
        //public string Attachments { get; set; }
        //public string Screenshots { get; set; }
    }
}