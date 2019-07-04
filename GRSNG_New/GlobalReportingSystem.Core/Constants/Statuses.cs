using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Constants
{
    public static class QcStatuses
    {
        public const string Blocked = "Blocked";
        public const string BlockingFail = "Blocking Fail";
        public const string Failed = "Failed";
        public const string KnownFail = "Known Fail";
        public const string NA = "N/A";
        public const string NoRun = "No Run";
        public const string NotComplited = "Not Complited";
        public const string OldFail = "Old Fail";
        public const string Passed = "Passed";
    }

    public static class DbStatuses
    {
        public const string Fail = "fail";
        public const string NoRun = "norun";
        public const string Pass = "pass";
    }
}
