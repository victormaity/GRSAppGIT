using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class ExportToExcel
    {
        public string Subject { get; set; }
        public string TestName { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string BusinessCriticality { get; set; }
        public string TCState { get; set; }
        public string ClientIP { get; set; }
        public string EndTime { get; set; }
        public string Attachments { get; set; }
        public string Screenshots { get; set; }
    }
}
