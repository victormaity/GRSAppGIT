
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Data.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.DataLINQ
{
    public class HelperHomeProvider
    {
        //enum PTYPES { GUI = 1, Services = 2, API = 3 };

        GRSDataBaseEntities DBENT = new GRSDataBaseEntities();

        private string GetEnvName(string clientURL, List<int> projectIds)
        {
            var envData = (from HC in DBENT.HostsConfigurations
                           where HC.ApplicationURL.Contains(clientURL) && projectIds.Contains(HC.BelongToProject.Value)
                           select HC).FirstOrDefault();
            if (envData != null)
                return envData.EnvironmentName;
            else
                return clientURL;
        }

        private bool DateParseMMDDYYYY(string inputDateString, string timeP, out DateTime inputDateVal)
        {
            inputDateVal = DateTime.Now;

            if (inputDateString.Length == 10)
            {
                //inputdatetime format is MM DD YYYY
                string monthdata = inputDateString.Substring(0, 2);
                int monthconverted = 0;
                bool ismonthvalid = Int32.TryParse(monthdata, out monthconverted);

                string datedata = inputDateString.Substring(3, 2);
                int dateconverted = 0;
                bool isdatevalid = Int32.TryParse(datedata, out dateconverted);

                string yeardata = inputDateString.Substring(6);
                int yearconverted = 0;
                bool isyearvalid = Int32.TryParse(yeardata, out yearconverted);

                try
                {
                    if (timeP == "DS")
                    {
                        inputDateVal = new DateTime(yearconverted, monthconverted, dateconverted, 0, 0, 0);
                    }
                    else
                    {
                        inputDateVal = new DateTime(yearconverted, monthconverted, dateconverted, 23, 59, 59);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public ExecutionReportPageDataModel GetExecutionReport(int projectId, string duration = "30DAYS", string ddlvalreleasename = "ALL", string ddlvalteamname = "ALL", string startdate = "", string enddate = "", int cycleid = 0)
        {

            ExecutionReportPageDataModel returnmodel = new ExecutionReportPageDataModel();
            List<ExecutionReportModel> modelDataToReturn = new List<ExecutionReportModel>();

            List<string> listReleaseName = (from x in DBENT.ReleaseInfoes where x.ParentProjectId == projectId select x.ReleaseName).ToList();
            List<string> listTeamName = (from x in DBENT.TeamInfoes where x.ParentProject == projectId select x.TeamName).ToList();
            List<TestcycleData> tcdata = (from x in DBENT.TestCycles where x.ParentProject == projectId select new TestcycleData { cid = x.ID, cname = x.CycleName }).ToList();
            tcdata = (from x in tcdata orderby x.cname ascending select x).ToList();
            returnmodel.DDLReleaseNameList = listReleaseName;
            returnmodel.DDLTeamNameList = listTeamName;
            returnmodel.TestcycleData = tcdata;

            #region Set duration
            //duraiton will be  TODAY, 5DAYS, 10DAYS, 15DAYS, 30DAYS, 3MONTHS, 6MONTH, 1YEAR
            /*start date and end date will be in format of MM/dd/yyyy */

            if (duration.ToUpper() == "BYCYCLE" && cycleid <= 0)
            {
                duration = "5DAYS";
            }
            else if (duration.ToUpper() == "BYCYCLE" && cycleid > 0)
            {
                bool iscycleexist = (from x in DBENT.TestCycles where x.ID == cycleid select x).Any();
                if (iscycleexist == false)
                {
                    duration = "5DAYS";
                }
            }

            DateTime reportEndRange = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            DateTime reportStartTemp = DateTime.Now;

            if (duration.ToUpper() == "BYCYCLE" && cycleid > 0) { reportStartTemp = DateTime.MinValue; }
            else if (duration.ToUpper() == "TODAY") { reportStartTemp = DateTime.Now; }
            else if (duration.ToUpper() == "5DAYS") { reportStartTemp = DateTime.Now.AddDays(-5); }
            else if (duration.ToUpper() == "10DAYS") { reportStartTemp = DateTime.Now.AddDays(-10); }
            else if (duration.ToUpper() == "15DAYS") { reportStartTemp = DateTime.Now.AddDays(-15); }
            else if (duration.ToUpper() == "30DAYS") { reportStartTemp = DateTime.Now.AddDays(-30); }
            else if (duration.ToUpper() == "3MONTHS") { reportStartTemp = DateTime.Now.AddMonths(-3); }
            else if (duration.ToUpper() == "6MONTHS") { reportStartTemp = DateTime.Now.AddMonths(-6); }
            else if (duration.ToUpper() == "1YEAR") { reportStartTemp = DateTime.Now.AddYears(-1); }
            else if (duration.ToUpper() == "BWDATE")
            {
                if ((!string.IsNullOrEmpty(startdate)) && (!string.IsNullOrEmpty(enddate)))
                {
                    DateParseMMDDYYYY(startdate, "DS", out reportStartTemp);
                    DateParseMMDDYYYY(enddate, "", out reportEndRange);
                }
                else
                {
                    reportStartTemp = DateTime.Now.AddDays(-30);
                }
            }
            DateTime reportStartRange = new DateTime(reportStartTemp.Year, reportStartTemp.Month, reportStartTemp.Day, 0, 0, 0);
            #endregion Set duration

            string Component = (from x in DBENT.ProjectTypes
                                join y in DBENT.Projects on x.ID equals y.ProjectTypeID.Value
                                where y.ID == projectId
                                select x.ProjectTypeName).FirstOrDefault();

            List<ReleaseInfo> releaseInfos;


            if ((!string.IsNullOrEmpty(ddlvalreleasename)) && (ddlvalreleasename.ToUpper().Trim() != "ALL"))
            {
                ddlvalreleasename = ddlvalreleasename.Trim();
                releaseInfos = (from x in DBENT.ReleaseInfoes
                                orderby x.ReleaseName ascending
                                where x.ParentProjectId == projectId
                                && x.ReleaseName == ddlvalreleasename
                                select x).ToList();
            }
            else
            {
                releaseInfos = (from x in DBENT.ReleaseInfoes
                                orderby x.ReleaseName ascending
                                where x.ParentProjectId == projectId
                                select x).ToList();
            }

            int rowid = 0;

            foreach (var releaseitem in releaseInfos)
            {
                List<TestCycle> testCycleList;
                if (duration.ToUpper() == "BYCYCLE" && cycleid > 0)
                {
                    testCycleList = (from x in DBENT.TestCycles where x.ID == cycleid && x.ReleaseName == releaseitem.ReleaseName && x.ParentProject == projectId select x).ToList();
                }
                else
                {
                    testCycleList = (from x in DBENT.TestCycles where x.ReleaseName == releaseitem.ReleaseName && x.ParentProject == projectId select x).ToList();
                }
                
                List<string> teamNameList;

                if ((!string.IsNullOrEmpty(ddlvalteamname)) && (ddlvalteamname.ToUpper().Trim() != "ALL"))
                {
                    ddlvalteamname = ddlvalteamname.Trim();
                    teamNameList = (from x in testCycleList orderby x.TeamName where x.TeamName == ddlvalteamname select x.TeamName).Distinct().ToList();
                }
                else
                {
                    teamNameList = (from x in testCycleList orderby x.TeamName select x.TeamName).Distinct().ToList();
                }

                foreach (var teamitem in teamNameList)
                {
                    List<int> testcycleIds = (from x in testCycleList where x.ReleaseName == releaseitem.ReleaseName && x.TeamName == teamitem select x.ID).ToList();

                    var testsuitsidList = (from x in DBENT.TestSuits
                                           where testcycleIds.Contains(x.ParentTestCycle.Value)
                                           && x.ParentProject == projectId
                                           && x.TSStart >= reportStartRange
                                           && x.TSStart <= reportEndRange
                                           select x.ID).ToList();



                    if (testsuitsidList.Count > 0)
                    {
                        var forenvdetailsbysuit = (from x in DBENT.TestSuits
                                                   join y in DBENT.ClientsInformations on x.ParentClientInfo.Value equals y.ID
                                                   where x.ParentProject == projectId && testsuitsidList.Contains(x.ID)
                                                   select new
                                                   {
                                                       TSId = x.ID,
                                                       TSClientinfoId = y.ID,
                                                       TSEnvURL = y.ClientURL,
                                                       //TSEnvName = GetEnvName(y.ClientURL, listprojectids),
                                                       TCTotal = (from tct in DBENT.TestCases where tct.ParentTestSuite == x.ID select tct).Count(),
                                                       TCPass = (from tcp in DBENT.TestCases where tcp.ParentTestSuite == x.ID && tcp.TCState == "pass" select tcp).Count(),
                                                       TCFail = (from tcf in DBENT.TestCases where tcf.ParentTestSuite == x.ID && tcf.TCState == "fail" select tcf).Count(),
                                                       TCNotCompleted = (from tcnc in DBENT.TestCases where tcnc.ParentTestSuite == x.ID && tcnc.TCState == "notcompleted" select tcnc).Count(),
                                                       TCNoRun = (from tcnr in DBENT.TestCases where tcnr.ParentTestSuite == x.ID && tcnr.TCState == "norun" select tcnr).Count()
                                                   }).ToList();

                        int TotalPass = (from x in forenvdetailsbysuit select x.TCPass).Sum();
                        int TotalFail = (from x in forenvdetailsbysuit select x.TCFail).Sum();
                        int TotalNotCompleted = (from x in forenvdetailsbysuit select x.TCNotCompleted).Sum();
                        int TotalNoRun = (from x in forenvdetailsbysuit select x.TCNoRun).Sum();


                        var tsIdsPassState = (from x in DBENT.TestSuits
                                              join y in DBENT.TestCases on x.ID equals y.ParentTestSuite
                                              where y.TCState == "pass" && testsuitsidList.Contains(x.ID)
                                              select x.ID).Distinct().ToList();
                        string passts = string.Join(",", tsIdsPassState);

                        var tsIdsFailState = (from x in DBENT.TestSuits
                                              join y in DBENT.TestCases on x.ID equals y.ParentTestSuite
                                              where y.TCState == "fail" && testsuitsidList.Contains(x.ID)
                                              select x.ID).Distinct().ToList();
                        string failts = string.Join(",", tsIdsFailState);

                        var tsIdsNotCompletedState = (from x in DBENT.TestSuits
                                                      join y in DBENT.TestCases on x.ID equals y.ParentTestSuite
                                                      where y.TCState == "notcompleted" && testsuitsidList.Contains(x.ID)
                                                      select x.ID).Distinct().ToList();
                        string notcompletedts = string.Join(",", tsIdsNotCompletedState);

                        var tsIdsNoRunState = (from x in DBENT.TestSuits
                                               join y in DBENT.TestCases on x.ID equals y.ParentTestSuite
                                               where y.TCState == "norun" && testsuitsidList.Contains(x.ID)
                                               select x.ID).Distinct().ToList();
                        string norunts = string.Join(",", tsIdsNoRunState);



                        var locenvlist = (from x in forenvdetailsbysuit select x.TSEnvURL).Distinct().ToList();

                        var envfinal1 = (from xdata in locenvlist
                                         select new ListENVDetailsWithCount
                                         {
                                             ENVURL = xdata,
                                             ENVNAME = "",
                                             TSIDS = string.Join(",", (from tsenvlocal in forenvdetailsbysuit
                                                                       where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                                       select tsenvlocal.TSId).Distinct().ToList()),
                                             TOTAL = (from tsenvlocal in forenvdetailsbysuit
                                                      where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                      select tsenvlocal.TCTotal).Sum(),
                                             PASSED = (from tsenvlocal in forenvdetailsbysuit
                                                       where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                       select tsenvlocal.TCPass).Sum(),
                                             FAILED = (from tsenvlocal in forenvdetailsbysuit
                                                       where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                       select tsenvlocal.TCFail).Sum(),
                                             NOTCOMPLETED = (from tsenvlocal in forenvdetailsbysuit
                                                             where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                             select tsenvlocal.TCNotCompleted).Sum(),
                                             NORUNED = (from tsenvlocal in forenvdetailsbysuit
                                                        where tsenvlocal.TSEnvURL.ToUpper().Trim() == xdata.ToUpper().Trim()
                                                        select tsenvlocal.TCNoRun).Sum(),
                                         }).ToList();

                        rowid += 1;
                        modelDataToReturn.Add(new ExecutionReportModel()
                        {
                            Rowid = rowid,
                            ReleaseName = releaseitem.ReleaseName,
                            ReleaseDate_MM_DD_YYYY = releaseitem.ReleaseDate.ToString("MM/dd/yyyy"),
                            DateRangeSTART = reportStartRange,
                            DateRangeEND = reportEndRange,
                            TeamName = teamitem,
                            Component = Component,
                            TotalPass = TotalPass,
                            TotalFail = TotalFail,
                            TotalNotCompleted = TotalNotCompleted,
                            TotalNoRun = TotalNoRun,
                            PasstestsuitId = passts,
                            FailtestsuitId = failts,
                            NotCompletedtestsuitId = notcompletedts,
                            NoRuntestsuitId = norunts,
                            ExecutionReportByEnvCount = envfinal1
                        });
                    }
                }
            }
            returnmodel.ExecutionReportModel = modelDataToReturn;
            return returnmodel;
        }

        public CountAndVMInfo GetSuitTestCaseExecutionCountAndClientInfo(string suitIds)
        {
            if (!string.IsNullOrEmpty(suitIds.Trim()))
            {
                int[] nums = Array.ConvertAll(suitIds.Trim().Split(','), int.Parse);
                List<int> suitidlist = new List<int>(nums);
                suitidlist = (from x in suitidlist select x).Distinct().ToList();
                var vmdata = (from TS in DBENT.TestSuits
                              join CI in DBENT.ClientsInformations on TS.ParentClientInfo.Value equals CI.ID
                              where suitidlist.Contains(TS.ID)
                              select new VMInfo { MachineIP = CI.ClientIP, Browser = CI.ClientBrowser, OS = CI.ClientOS }).Distinct().ToList();
                int totalcount = (from TC in DBENT.TestCases where suitidlist.Contains(TC.ParentTestSuite.Value) select TC.ID).Distinct().Count();
                int passcount = (from TC in DBENT.TestCases where suitidlist.Contains(TC.ParentTestSuite.Value) && TC.TCState == "pass" select TC.ID).Distinct().Count();
                int failcount = (from TC in DBENT.TestCases where suitidlist.Contains(TC.ParentTestSuite.Value) && TC.TCState == "fail" select TC.ID).Distinct().Count();
                int notcompletedcount = (from TC in DBENT.TestCases where suitidlist.Contains(TC.ParentTestSuite.Value) && TC.TCState == "notcompleted" select TC.ID).Distinct().Count();
                int noruncount = (from TC in DBENT.TestCases where suitidlist.Contains(TC.ParentTestSuite.Value) && TC.TCState == "norun" select TC.ID).Distinct().Count();
                return new CountAndVMInfo()
                {
                    TestSuitIds = suitIds,
                    Total = totalcount,
                    TotalPass = passcount,
                    TotalFail = failcount,
                    TotalNotCompleted = notcompletedcount,
                    TotalNoRun = noruncount,
                    VMInfo = vmdata
                };
            }
            else
            {
                return new CountAndVMInfo() { TestSuitIds = suitIds, Total = 0, TotalPass = 0, TotalFail = 0, TotalNotCompleted = 0, TotalNoRun = 0, VMInfo = new List<VMInfo>() };
            }
        }

        public TSTCInforBase GetTestSuitWithTestCasesDetails(string suitIds)
        {
            TSTCInforBase TSTCInforBase = new TSTCInforBase();
            if (!string.IsNullOrEmpty(suitIds))
            {
                suitIds = suitIds.Trim();
                int[] nums = Array.ConvertAll(suitIds.Trim().Split(','), int.Parse);
                List<int> suitidlist = new List<int>(nums);
                suitidlist = (from x in suitidlist select x).Distinct().ToList();
                var prereturn = (from TS in DBENT.TestSuits
                                 where suitidlist.Contains(TS.ID)
                                 select new
                                 {
                                     TestsuitName = TS.TSName,
                                     TestCasesInfo = (from TC in DBENT.TestCases
                                                      where TC.ParentTestSuite == TS.ID
                                                      select new
                                                      {
                                                          TCName = TC.TCName,
                                                          TCState = TC.TCState,
                                                          TCStartTime = TC.TCStartTime,
                                                          TCEndTime = TC.TCEndTime,
                                                          TCDescription = TC.TCDescription
                                                      }).ToList()
                                 }).ToList();



                var TSTCInfoRow = (from x in prereturn
                                   select new TSTCInfo
                                   {
                                       TestsuitName = x.TestsuitName,
                                       TestcasesRowCount = x.TestCasesInfo.Count,
                                       TestCasesInfo = (from y in x.TestCasesInfo
                                                        select new TestCasesInfo
                                                        {
                                                            TCName = y.TCName,
                                                            TCState = y.TCState,
                                                            TCStartTime = y.TCStartTime.ToString("MM/dd/yyyy HH:mm:ss"),
                                                            TCEndTime = y.TCEndTime.ToString("MM/dd/yyyy HH:mm:ss"),
                                                            TCDescription = y.TCDescription
                                                        }).ToList()
                                   }).ToList();
                TSTCInforBase.TotalRowCount = TSTCInfoRow.Count;
                TSTCInforBase.TSTCInfo = TSTCInfoRow;
                return TSTCInforBase;
            }
            else
            {
                TSTCInforBase.TotalRowCount = 0;
                return TSTCInforBase;
            }
        }

    }
}
