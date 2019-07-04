using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GlobalReportingSystem.Web.API.Models.Project
{
    [DataContract]
    public class ExecutionResponse
    {
        [DataMember]
        public IEnumerable<string> Urls { get; set; }

        [DataMember]
        public IEnumerable<string> TestCycles { get; set; }

        [DataMember]
        public IEnumerable<AccountModel> Accounts { get; set; }
    }
}