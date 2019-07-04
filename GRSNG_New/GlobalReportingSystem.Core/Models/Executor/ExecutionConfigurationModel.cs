using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class ExecutionConfigurationModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
