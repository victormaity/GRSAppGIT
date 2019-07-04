using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Data.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GlobalReportingSystem.DataLINQ
{
    public class HelperRelReportProvider
    {
        GRSDataBaseEntities DBENT = new GRSDataBaseEntities();

        public string GetTestSuitIds(int executionId, string partialTestsuitIds)
        {
            var tids = (from x in DBENT.RelExecutionStatus
                        where x.ExecutionId == executionId
                        && x.TestSuitIds.Contains(partialTestsuitIds)
                        select x).FirstOrDefault();
            if (tids != null)
                return tids.TestSuitIds;
            else
                return "";
        }

        public DateTime GetMinStartDateTime_Testcase_Testsuit(Int64 testsuitid)
        {
            return (from x in DBENT.TestCases where x.ParentTestSuite == testsuitid select x.TCStartTime).Min();
        }

        public DateTime GetMaxEndDateTime_Testcase_Testsuit(Int64 testsuitid)
        {
            return (from x in DBENT.TestCases where x.ParentTestSuite == testsuitid select x.TCEndTime).Max();
        }

        public JenkinsReportCountStatus GetJenkinReportFinalCountByTestSuitIds(List<int> testsuitids)
        {
            int totalTestCaseCount = (from x in DBENT.TestCases
                                      where testsuitids.Contains(x.ParentTestSuite.Value)
                                      select x).Count();
            int totalPassCount = (from x in DBENT.TestCases
                                  where testsuitids.Contains(x.ParentTestSuite.Value)
                                  && x.TCState == "pass"
                                  select x).Count();
            int totalFailCount = (from x in DBENT.TestCases
                                  where testsuitids.Contains(x.ParentTestSuite.Value)
                                  && x.TCState == "fail"
                                  select x).Count();
            int totalNoRunCount = (from x in DBENT.TestCases
                                   where testsuitids.Contains(x.ParentTestSuite.Value)
                                   && x.TCState == "norun"
                                   select x).Count();
            int totalNotCompletedCount = (from x in DBENT.TestCases
                                          where testsuitids.Contains(x.ParentTestSuite.Value)
                                          && x.TCState == "notcompleted"
                                          select x).Count();

            return new JenkinsReportCountStatus()
            {
                totalTestCaseCount = totalTestCaseCount,
                totalPassCount = totalPassCount,
                totalFailCount = totalFailCount,
                totalNoRunCount = totalNoRunCount,
                totalNotCompletedCount = totalNotCompletedCount,
            };

        }

        public List<int> GetDistinctListOfProjectId(List<Int64> testsuitids)
        {
            return (from x in DBENT.TestSuits where testsuitids.Contains(x.ID) select x.ParentProject.Value).Distinct().ToList();
        }

        public List<int> GetDistinctListOfTestCycleId(List<Int64> testsuitids)
        {
            return (from x in DBENT.TestSuits where testsuitids.Contains(x.ID) select x.ParentTestCycle.Value).Distinct().ToList();
        }

        public List<int> GetDistinctListOfClientInfoId(List<Int64> testsuitids)
        {
            return (from x in DBENT.TestSuits where testsuitids.Contains(x.ID) select x.ParentClientInfo.Value).Distinct().ToList();
        }

        public string ListOfEnv(int projectid, List<int> lstClientinfoIds)
        {
            var cinfo = (from x in DBENT.ClientsInformations where lstClientinfoIds.Contains(x.ID) select x.ClientURL).Distinct().ToList();
            //var hostConfigEnvNameList = (from x in DBENT.HostsConfigurations where x.BelongToProject == projectid && cinfo.Contains(x.ApplicationURL) select x.EnvironmentName).Distinct().ToList();
            //return string.Join(", ", hostConfigEnvNameList);


            List<string> envnamelist = new List<string>();
            var hostConfigList = (from x in DBENT.HostsConfigurations where x.BelongToProject == projectid select x).ToList();

            foreach (var cinfoclienturl in cinfo)
            {
                foreach (var item in hostConfigList)
                {
                    if (cinfoclienturl.Trim().IndexOf(item.ApplicationURL.Trim()) > -1)
                    {
                        envnamelist.Add(item.EnvironmentName);
                    }
                }
            }

            var distinctenamelist = (from x in envnamelist select x).Distinct().ToList();

            return string.Join(", ", distinctenamelist);
        }

    }
}
