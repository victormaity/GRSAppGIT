using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class TestCycleModel
    {
        public int ID { get; set; }
        public string CycleName { get; set; }
        public DateTime CycleStart { get; set; }
        public DateTime CycleEnd { get; set; }
        public string CycleComments { get; set; }
        public bool CycleIsCurrent { get; set; }
        public bool isInnactive { get; set; }
    }
}
