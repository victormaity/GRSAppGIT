using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IMangaeRelReportProvider
    {
        #region Launch Rel Link

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testsuitId"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        //void UpdateTestSuitStartEndDateTime(int testsuitId, DateTime startDateTime, DateTime endDateTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        Int64 TotalTestSuitCount(string rel, string vm);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        List<TestSuit> GetLastTestAsListSuitBasedOnRelAndVM(string rel, string vm);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        List<TestSuit> GetTestSuitOnRelAndVM(string rel, string vm);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <param name="env"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        //public string HTMLLaunchReportForRel(string type, string rel, string vm, string env, string account);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="TestsuitId"></param>
        /// <returns></returns>
        //string HTMLLaunchReportForRel(int executionId, Int64 TestsuitId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RelRelateIDs"></param>
        /// <returns></returns>
        string HTMLForLaunchRELReportEmail(List<RelReturn> RelRelateIDs, string Message);

        /// <summary>
        /// Email report with machine last execution status
        /// </summary>
        /// <param name="RelRelateIDs"></param>
        /// <param name="MachineStatus"></param>
        /// <returns></returns>
        string HTMLForLaunchRELReportEmail(List<RelReturn> RelRelateIDs, List<RelExecutionResponseP2> MachineStatus);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="testsuitId"></param>
        /// <returns></returns>
        string HTMLForLaunchRELReportBrowser(int executionId, Int64 testsuitId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="testsuitId"></param>
        /// <returns></returns>
        string HTMLForLaunchRELTestSuitDetails(int executionId, List<Int64> testsuitId);

        #endregion Launch Rel Link

        #region Anonymous Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="MachineIP"></param>
        /// <returns></returns>
        RelExecutionStatusLastAndCount GetLastIdAndCountOfRelExecutionStatus(Int64 executionId, string MachineIP);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RelExecutionStatusId"></param>
        /// <returns></returns>
        RelExecutionStatu GetRelExecutionStatusById(Int64 RelExecutionStatusId);
        #endregion Anonymous Method


        RelExecutionResponseP2 GetLatestRelStatusByIP(string IP, DateTime serverdatetime);
        RelExecutionResponseP2 GetLatestRelStatusByID(Int64 ID, DateTime serverdatetime);
        EmailRelMapping GetRelExecutionResultById(Int64 ID);
        List<TestSuit> GetTestSuitByExecutionIdAndAfterTestSuitId(int executionId, int testsuitid, string vm);
        List<TestSuit> GetTestSuitByExecutionIdWithinStartAndEndLimit(int executionId, int testsuitstartid, int testsuitendid, string vm);
        

        //--------------In the Replacement of Load Balanced Execution Service-------------------------

        Int64 CreateRelExecutionStatus(int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment);

        void UpdateLatestRelExecutionStatusEntryLevelRecord(Int64 RelStatusID, Int64 LastRowCount, Int64 LastRowId, string ToAddress);

        string GetTestSuitIdString(int executionId, string testsuitids);

        ////////////////////////
        JenkinsReportCountStatus GetJenkinReportFinalCountByTestSuitIds(List<int> testsuitids);

    }
}
