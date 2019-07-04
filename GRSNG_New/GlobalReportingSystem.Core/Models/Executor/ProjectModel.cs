using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.Executor
{
    public class ProjectModel
    {
        public string ProjectName { get; set; }        
        public string DisplayName { get; set; }
        public bool IsGITDefault { get; set; }
        public string SvnPath { get; set; }
        public string SvnLogin { get; set; }
        public string SvnPassword { get; set; }
        public string GitPath { get; set; }
        public string GitLogin { get; set; }
        public string GitPassword { get; set; }
    }
}
