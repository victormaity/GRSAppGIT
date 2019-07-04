using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GlobalReportingSystem.Web.API.Models.Project
{
    [DataContract]
    public class ProjectsResponse
    {
        [DataMember]
        public IEnumerable<string> Projects { get; set; }

    }
}