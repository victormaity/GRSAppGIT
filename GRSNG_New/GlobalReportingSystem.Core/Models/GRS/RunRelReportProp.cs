using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class RunRelReportProp
    {

    }

    public class RunRelClient_Info
    {
        public string project { get; set; }
        public string test_cycle { get; set; }
        public string url { get; set; }
        public string environment { get; set; }
        public string user { get; set; }
        public string ip { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public string browser { get; set; }
        public string os { get; set; }
        public string free_space { get; set; }
    }

    public class RunRelTestSet
    {
        public string start_time { get; set; }
        public string name { get; set; }
        public List<RunRelTestCase> TestCase { get; set; }
    }

    public class RunRelTestCase
    {
        public string start_time { get; set; }
        public string name { get; set; }
        public string end_time { get; set; }
        public string state { get; set; }
        public string criticality { get; set; }
        public string description { get; set; }
        public List<RunRelSteps> Steps { get; set; }
    }

    public class RunRelSteps
    {
        public string description { get; set; }
        public string attachment { get; set; }
        public string inputdata { get; set; }
        public string time { get; set; }
        public string expected { get; set; }
        public string type { get; set; }
        public List<RunRelSubSteps> SubSteps { get; set; }
    }

    public class RunRelSubSteps
    {
        public string time { get; set; }
        public string valid { get; set; }
        public string InternalMessage { get; set; }
        public string ScreenShot1 { get; set; }
        public string ScreenShot2 { get; set; }
    }

    public class RunRelResultSet
    {
        public RunRelClient_Info ResclientInfo { get; set; }
        public List<RunRelTestSet> ResTestSet { get; set; }
    }

    public class RelReturn
    {
        public int ExecutionId { get; set; }
        public int TestSuitId { get; set; }
        public string Message { get; set; }
    }

    public class RelRepotTotalTestsetWithExecution
    {
        public int ExecutionID { get; set; }

        public int ProjectID { get; set; }
        public string ProjectName { get; set; }

        public int TestCycleID { get; set; }
        public string TestCycleName { get; set; }

        //public Int64 TestSuitID { get; set; }
        public string TestSuitName { get; set; }

        public string RemoteMachineIP { get; set; }
        public string OS { get; set; }
        public string Browser { get; set; }
        public string Environment { get; set; }
        public string TestURL { get; set; }

        public TimeSpan ExecutionTime { get; set; }
        public int TestCases { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int NotCompleted { get; set; }
        public int NoRun { get; set; }
        public string ErrorDetails { get; set; }
        public string GrsLink { get; set; }
    }

    public class RELReportErrorDetailsBrowser
    {
        public string TestSetName { get; set; }
        public string TestCaseName { get; set; }
        public string TestCaseStatus { get; set; }
        public List<RELReportTestStepBrowser> TestStepsDetails { get; set; }
    }

    public class RELReportTestStepBrowser
    {
        public string ProjectID { get; set; }
        public string TestStepName { get; set; }
        public string TestStepStatus { get; set; }
        public string TestStepExpected { get; set; }
        public string InputData { get; set; }
        public string TestCaseAttachment { get; set; }
        public string TestSubStepScreenShots { get; set; }
    }

    public class ReturnRelLinkNEW
    {
        public string REL { get; set; }
        public string VM { get; set; }
        public string Environment { get; set; }
        public string Account { get; set; }
        public int TotalCount { get; set; }
        public Int64 LastTestSuitId { get; set; }
        public int ExecutionID { get; set; }
        public string Message { get; set; }
    }

    public class RELREPEmail
    {
        public int EXECUTIONID { get; set; }
        public List<int> TESTSUITID { get; set; }
    }

    public class RelExecutionStatusLastAndCount
    {
        public Int64 LastRelExecutionId { get; set; }
        public Int64 TotalRelExecutionCount { get; set; }
    }

    public class RelExecutionResponseP2
    {
        public Int64 RelExecutionStatusId { get; set; }
        public string MachineIP { get; set; }
        public string Status { get; set; }
    }

    public class RelExeStatusReturn
    {
        public Int64 RelStatusId { get; set; }
        public string MachineIP { get; set; }
        public string CurrentStatus { get; set; }
        //public bool IsCompleted { get; set; }
    }

    public class EmailRelMapping
    {
        public Int64 RelStatusId { get; set; }
        public bool IsCompleted { get; set; }
        public int ExecutionId { get; set; }
        public string TestSuitId { get; set; }
    }

    public class BreakedDateTime
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }

    public class ResponseDelete
    {
        public bool IsDeleted { get; set; }
        public bool IsException { get; set; }
        public string Message { get; set; }
    }

}
