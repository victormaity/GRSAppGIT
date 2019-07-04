using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{

    //down 2 class is for Page bind
    public class ExecutionReportPageDataModel
    {
        public List<string> DDLReleaseNameList { get; set; }
        public List<string> DDLTeamNameList { get; set; }
        public List<TestcycleData> TestcycleData { get; set; }
        public List<ExecutionReportModel> ExecutionReportModel { get; set; }

    }

    public class TestcycleData
    {
        public int cid { get; set; }
        public string cname { get; set; }
    }

    public class ExecutionReportModel
    {
        public int Rowid { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseDate_MM_DD_YYYY { get; set; }
        public DateTime DateRangeSTART { get; set; }
        public DateTime DateRangeEND { get; set; }
        public string TeamName { get; set; }
        public string Component { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }
        public int TotalNotCompleted { get; set; }
        public int TotalNoRun { get; set; }
        public string PasstestsuitId { get; set; }
        public string FailtestsuitId { get; set; }
        public string NotCompletedtestsuitId { get; set; }
        public string NoRuntestsuitId { get; set; }
        public List<ListENVDetailsWithCount> ExecutionReportByEnvCount { get; set; }
    }

    public class ListENVDetailsWithCount
    {
        public string ENVURL { get; set; }
        public string ENVNAME { get; set; }
        public string TSIDS { get; set; }
        public Int64 TOTAL { get; set; }
        public Int64 PASSED { get; set; }
        public Int64 FAILED { get; set; }
        public Int64 NOTCOMPLETED { get; set; }
        public Int64 NORUNED { get; set; }
    }

    public class CountAndVMInfo
    {
        public String TestSuitIds { get; set; }
        public int Total { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }
        public int TotalNotCompleted { get; set; }
        public int TotalNoRun { get; set; }
        public List<VMInfo> VMInfo { get; set; }
    }

    public class VMInfo
    {
        public string MachineIP { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
    }

    public class TSTCInforBase
    {
        public int TotalRowCount { get; set; }
        public List<TSTCInfo> TSTCInfo { get; set; }
    }

    public class TSTCInfo
    {
        public string TestsuitName { get; set; }
        public int TestcasesRowCount { get; set; }
        public List<TestCasesInfo> TestCasesInfo { get; set; }
    }

    public class TestCasesInfo
    {
        public string TCName { get; set; }
        public string TCState { get; set; }
        public string TCStartTime { get; set; }
        public string TCEndTime { get; set; }
        public string TCDescription { get; set; }
    }

}
