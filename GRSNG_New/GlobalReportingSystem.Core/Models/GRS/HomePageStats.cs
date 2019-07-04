using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class HomePageStats
    {
        public CurrentRegressionStats CurrentRegressionStats { get; set; }
        public List<TestSuit> TestSuites { get; set; }
    }

    public class CurrentRegressionStats
    {
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Blocked { get; set; }
    }
}
