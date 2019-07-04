using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GlobalReportingSystem.Core.Models.Entities;


namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ProjectAndUsersModel
    {
        public Project Project { get; set; }
        public List<User> User { get; set; }
        public List<string> TsNames { get; set; }
    }
} 