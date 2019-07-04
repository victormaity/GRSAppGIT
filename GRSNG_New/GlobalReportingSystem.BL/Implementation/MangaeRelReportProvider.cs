using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.DataLINQ;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.BL.Implementation
{
    public class MangaeRelReportProvider : IMangaeRelReportProvider
    {
        #region Initialize Constroctor

        private readonly ISessionHelper _sessionHelper;
        private readonly IRepository<TestsExecution> _testExecutionRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TestCycle> _testCycleRepository;
        private readonly IRepository<AccountForTestRun> _accountForTestRunRepository;
        private readonly IRepository<Client> _clientsRepository;
        private readonly IRepository<ClientsInformation> _clientsInformationRepository;
        private readonly IRepository<TestSuit> _testSuitsRepository;
        private readonly IRepository<TestStep> _testStepRepository;
        private readonly IRepository<TestCase> _testCaseRepository;
        private readonly IRepository<SubStep> _substepRepository;
        private readonly IRepository<RelExecutionStatu> _relExecutionStatus;
        private readonly IRepository<HostsConfiguration> _hostsConfiguration;

        /// <summary>
        /// ManageRelReprotProvider Constroctor - Used to initialize the database table repository
        /// </summary>
        /// <param name="sessionHelper"></param>
        /// <param name="testExecutionRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="testCycleRepository"></param>
        /// <param name="accountForTestRunRepository"></param>
        /// <param name="clientsRepository"></param>
        /// <param name="clientsInformationRepository"></param>
        /// <param name="testSuitsRepository"></param>
        /// <param name="testStepRepository"></param>
        /// <param name="testCaseRepository"></param>
        /// <param name="substepRepository"></param>
        /// <param name="relExecutionStatus"></param>
        public MangaeRelReportProvider(
            ISessionHelper sessionHelper,
            IRepository<TestsExecution> testExecutionRepository,
            IRepository<Project> projectRepository,
            IRepository<TestCycle> testCycleRepository,
            IRepository<AccountForTestRun> accountForTestRunRepository,
            IRepository<Client> clientsRepository,
            IRepository<ClientsInformation> clientsInformationRepository,
            IRepository<TestSuit> testSuitsRepository,
            IRepository<TestStep> testStepRepository,
            IRepository<TestCase> testCaseRepository,
            IRepository<SubStep> substepRepository,
            IRepository<RelExecutionStatu> relExecutionStatus,
            IRepository<HostsConfiguration> hostsConfiguration
            )
        {
            _sessionHelper = sessionHelper;
            _testExecutionRepository = testExecutionRepository;
            _projectRepository = projectRepository;
            _testCycleRepository = testCycleRepository;
            _accountForTestRunRepository = accountForTestRunRepository;
            _clientsRepository = clientsRepository;
            _clientsInformationRepository = clientsInformationRepository;
            _testSuitsRepository = testSuitsRepository;
            _testStepRepository = testStepRepository;
            _testCaseRepository = testCaseRepository;
            _substepRepository = substepRepository;
            _relExecutionStatus = relExecutionStatus;
            _hostsConfiguration = hostsConfiguration;
        }

        #endregion Initialize Constroctor

        #region Launch Rel Link

        public int GetLastTestsuitID(string rel, string vm)
        {
            List<TestSuit> tslist = GetTestSuitOnRelAndVM(rel, vm);
            //var orderedlist = (from x in tslist orderby x.ID descending select x).ToList();
            return (from x in tslist select x.ID).Max();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testsuitId"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        //public void UpdateTestSuitStartEndDateTime(int testsuitId, DateTime startDateTime, DateTime endDateTime)
        //{
        //    var testsuitdetails = _testSuitsRepository.GetSingleOrDefault(TS => TS.ID == testsuitId);
        //    if (testsuitdetails != null)
        //    {
        //        testsuitdetails.TSStart = startDateTime;
        //        testsuitdetails.DeliveryTime = endDateTime;
        //        _testSuitsRepository.SaveChanges();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <param name="env"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public Int64 TotalTestSuitCount(string rel, string vm)
        {
            List<TestSuit> tslist = GetTestSuitOnRelAndVM(rel, vm);
            return tslist.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public List<TestSuit> GetLastTestAsListSuitBasedOnRelAndVM(string rel, string vm)
        {
            var TSTotalList = GetTestSuitOnRelAndVM(rel, vm);
            return (from x in TSTotalList orderby x.ID descending select x).Take(1).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public List<TestSuit> GetTestSuitOnRelAndVM(string rel, string vm)
        {
            Guid relGuid = Guid.Parse(rel);

            var execution = _testExecutionRepository.GetSingleOrDefault(p => p.RemoteExecutionLink == relGuid);
            if (execution != null)
            {
                var clientdata = _clientsRepository.GetFirstOrDefault(C => C.ID == execution.Client);

                var totalTestSuitList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

                var getAllClientList = _clientsInformationRepository.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == clientdata.RemoteMachineIP);
                List<int> CIID = (from x in getAllClientList select x.ID).ToList();

                //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                var TSTotalList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle
                                    && TS.ParentClientInfo.HasValue && CIID.Contains(TS.ParentClientInfo.Value));

                return TSTotalList.ToList();
            }
            else
            {
                return new List<TestSuit>();
            }
        }

        public string HTMLForLaunchRELReportEmail(List<RelReturn> RelRelateIDs, string Message)
        {
            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            List<string> msglist;
            if (!string.IsNullOrEmpty(Message))
            {
                string[] msgArr = Message.Split(',');
                msglist = new List<string>(msgArr);
            }
            else
            {
                msglist = new List<string>();
            }

          
            string BaseURLAddressGRSLink = "http://10.236.4.153/nggrs/TestCycle/Index/{TESTCYCLEID}?testSuite={TESTSUITID}";

            string grsProjectnName = string.Empty;
            string grsTestCycleName = string.Empty;
            string grsTestURL = string.Empty;
            string grsEnvironment = string.Empty;
            List<RelRepotTotalTestsetWithExecution> TotalRunDetails = new List<RelRepotTotalTestsetWithExecution>();

            string FirstLineStaticHTML = "<table cellpadding='8' style='border: 1px solid #000000; width: auto!important; font-family: Verdana!important; font-size: 12px; border-collapse: collapse;'><tr><td colspan='2' align='center' style=' background-color: #FF8000; border: 1px solid #000000; color: #FFFFFF;'><b>Project and Environment Details</b></td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Project</td><td  align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{PROJECT}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>GRS Test Cycle</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{GRSTESTCYCLE}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Test URL</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{TESTURL}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Environment</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{ENVIRONMENT}</td></tr></table><br /><table cellpadding='8' style='border: 1px solid #000000; width: 100%!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td cellpadding='8' colspan='13' align='center' style='font-weight: 500; background-color: #FF8000; color: #FFFFFF;'><b>Test Summary</b></td></tr><tr><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Feature/Test Set</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Remote Machine IP</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>OS</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Browser</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Execution Time (dd.hh:mm:ss)</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Total Scenario/Test cases</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Pass</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Fail</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Completed</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Run</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Details</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>GRS Link</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Comment</td></tr>";

            string SecondLineDynamicInitializer = "<tr><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSETNAME}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{REMOTEMACHINEIP}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{OS}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{BROWSER}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{EXECUTIONTIME}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTCASES}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{PASS}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{FAIL}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NOTCOMPLETED}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NORUN}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'><a href='{ERRORDETAILS}' target='_blank'>Click Here</a></td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{GRSLINK}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{ERRORMESSAGE}</td></tr>";
            string SecondLineDynamicHTML = string.Empty;

            string ThirdLineStaticHTML = "<tr><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Total</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALEXECUTIONTIME}&nbsp;(Max of All)</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALSCENARIO/TESTCASES}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALPASS}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #000000; font-weight: 600;'>{TOTALFAIL}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALNOTCOMPLETED}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALNORUN}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td></tr></table><br /><br /><table cellpadding='8' align='left' style='font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td style='font-size: 12px!important; font-weight: 500;'>Note: Inorder to view the complete results from GRS link, user must have already logged in to GRS website.</td></tr></table><br /><br /><br /><br /><br /><br /><table cellpadding='8' style='width: auto!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td align='left' valign='middle' style='color:#DD5A43; font-weight:500; font-size:22px!important; width:40px!important; padding:0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height:32px; width:32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color:#DD5A43; font-weight:600; font-size:22px!important; padding:0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style=' font-weight: 600;padding-top:2px!important;padding-bottom:0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{CURRENTYEAR}", DateTime.Now.Year.ToString());

            TimeSpan FinalTotalExecutionTime = new TimeSpan(0);
            string totalexecutiontimeinlist = string.Empty;
            int FinalTotalTestCases = 0;
            int FinalTotalPass = 0;
            int FinalTotalFail = 0;
            int FinalTotalNotCompleted = 0;
            int FinalTotalNoRun = 0;

            var executionidlist = (from x in RelRelateIDs select x.ExecutionId).Distinct().ToList();

            var newrelexetestsuitlist = (from x in executionidlist
                                         select new RELREPEmail
                                         {
                                             EXECUTIONID = x,
                                             TESTSUITID = (from y in RelRelateIDs where y.ExecutionId == x select y.TestSuitId).ToList()
                                         }).ToList();


            foreach (var item in newrelexetestsuitlist)
            {
                var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == item.EXECUTIONID);
                if (execution != null)
                {
                    int executionId = execution.ID;

                    var pdata = _projectRepository.GetSingleOrDefault(p => p.ID == execution.BelongToProject);
                    int projectId = pdata.ID;
                    string projectName = pdata.ProjectName;

                    var tcdata = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == execution.TargetTestCycle);
                    int testCycleId = tcdata.ID;
                    string testcycleName = tcdata.CycleName;

                    string testsuitName = string.Empty;
                    string url = string.Empty;
                    string environment = string.Empty;
                    string ip = string.Empty;
                    string browser = string.Empty;
                    string os = string.Empty;



                    List<TestSuit> InternalTestSuitList = new List<TestSuit>();
                    foreach (var itemts in item.TESTSUITID)
                    {
                        var tsdata = _testSuitsRepository.GetSingleOrDefault(TS => TS.ID == itemts);
                        if (tsdata != null)
                        {
                            var istestcasedataexiset = _testCaseRepository.Count(tt => tt.ParentTestSuite == tsdata.ID);
                            DateTime starttime = tsdata.TSStart;
                            DateTime endtime = tsdata.DeliveryTime ?? DateTime.Now;
                            if (istestcasedataexiset > 0)
                            {
                                starttime = HRRP.GetMinStartDateTime_Testcase_Testsuit(tsdata.ID);// tsdata.TSStart,
                                endtime = HRRP.GetMaxEndDateTime_Testcase_Testsuit(tsdata.ID);// tsdata.DeliveryTime,
                            }
                            InternalTestSuitList.Add(new TestSuit()
                            {
                                ID = tsdata.ID,
                                TSName = tsdata.TSName,
                                TSStart = starttime,
                                DeliveryTime = endtime,
                                ParentClientInfo = tsdata.ParentClientInfo
                            });
                        }
                    }

                    DateTime end = (DateTime)SqlDateTime.Null;
                    DateTime start = DateTime.Now;
                    int tscount = 1;
                    int localparentclientinfo = 0;
                    string tslinkstr = string.Empty;
                    List<TestCase> testcasedetails = new List<TestCase>();
                    foreach (var internalcalTS in InternalTestSuitList)
                    {
                        Int64 testsuitId = internalcalTS.ID;
                        if (tscount == 1)
                        {
                            localparentclientinfo = internalcalTS.ParentClientInfo ?? 0;
                            start = internalcalTS.TSStart;
                            end = internalcalTS.DeliveryTime ?? DateTime.Now;
                            tslinkstr = internalcalTS.ID.ToString();
                        }
                        else
                        {
                            DateTime tempend = internalcalTS.DeliveryTime ?? DateTime.Now;
                            end = end > tempend ? end : tempend;
                            DateTime tempstart = internalcalTS.TSStart;
                            start = start < tempstart ? start : tempstart;
                            tslinkstr = tslinkstr + "," + internalcalTS.ID.ToString();
                        }

                        var TCLIST = _testCaseRepository.GetAllToList(TC => TC.ParentTestSuite == testsuitId).ToList();
                        testcasedetails.AddRange(TCLIST);

                        tscount = tscount + 1;
                    }

                    string ErrorDetails = "http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport?exid={EXECUTIONID}&tsid={TESTSUITID}";
                    ErrorDetails = ErrorDetails.Replace("{EXECUTIONID}", executionId.ToString());
                    ErrorDetails = ErrorDetails.Replace("{TESTSUITID}", tslinkstr);
                    string GRSLink = string.Empty;
                    if (InternalTestSuitList.Count == 1)
                    {
                        testsuitName = InternalTestSuitList[0].TSName;
                        string tempGRSLINK = BaseURLAddressGRSLink;
                        tempGRSLINK = tempGRSLINK.Replace("{TESTCYCLEID}", testCycleId.ToString());
                        tempGRSLINK = tempGRSLINK.Replace("{TESTSUITID}", InternalTestSuitList[0].ID.ToString());
                        string finalGRSLINK = "<a href='{GRSLINK}' target='_blank'>Click Here</a>";
                        finalGRSLINK = finalGRSLINK.Replace("{GRSLINK}", tempGRSLINK);
                        GRSLink = finalGRSLINK;
                    }
                    else if (InternalTestSuitList.Count > 1)
                    {
                        testsuitName = "<a href='{TSLINK}' target='_blank'>Test suit / Feature</a>";
                        testsuitName = testsuitName.Replace("{TSLINK}", ErrorDetails);
                        string GRSLINKREDIRECT = "<a href='{GRSLINK}' target='_blank'>Click Here</a>";
                        GRSLINKREDIRECT = GRSLINKREDIRECT.Replace("{GRSLINK}", ErrorDetails);
                        GRSLink = GRSLINKREDIRECT;
                    }
                    else { GRSLink = string.Empty; }

                    var cinfodetails = _clientsInformationRepository.GetSingleOrDefault(CI => CI.ID == localparentclientinfo);
                    if (cinfodetails != null)
                    {
                        url = cinfodetails.ClientURL;
                        ip = cinfodetails.ClientIP;
                        browser = cinfodetails.ClientBrowser;
                        os = cinfodetails.ClientOS;
                    }

                    int profilehost = execution.ProfileHost ?? 0;
                    if (profilehost > 0)
                    {
                        environment = _hostsConfiguration.GetFirstOrDefault(a => a.ID == profilehost).EnvironmentName;
                    }


                    TimeSpan executionTime = new TimeSpan();
                    executionTime = (end - start);
                    int totaltestcases = testcasedetails.Count;
                    int totalpass = testcasedetails.Count(t => t.TCState == "pass");
                    int totalfail = testcasedetails.Count(t => t.TCState == "fail");
                    int totalnotcompleted = testcasedetails.Count(t => t.TCState == "notcompleted");
                    int totalnorun = testcasedetails.Count(t => t.TCState == "norun");

                    TotalRunDetails.Add(new RelRepotTotalTestsetWithExecution()
                    {
                        ExecutionID = executionId,
                        ProjectID = projectId,
                        ProjectName = projectName,
                        TestCycleID = testCycleId,
                        TestCycleName = testcycleName,
                        TestSuitName = testsuitName,
                        RemoteMachineIP = ip,
                        OS = os,
                        Browser = browser,
                        TestURL = url,
                        Environment = environment,
                        ExecutionTime = executionTime,
                        TestCases = totaltestcases,
                        Pass = totalpass,
                        Fail = totalfail,
                        NotCompleted = totalnotcompleted,
                        NoRun = totalnorun,
                        ErrorDetails = ErrorDetails,
                        GrsLink = GRSLink
                    });
                }
            }

            int count = 0;

            foreach (var iteminlist in TotalRunDetails)
            {
                count = count + 1;
                string rowcolorcode = ((count % 2) == 0) ? "#DCDCDC" : "#FFFFFF";

                if (!string.IsNullOrEmpty(grsProjectnName))
                {
                    string pn = string.Empty;
                    string nn = string.Empty;
                    if (!string.IsNullOrEmpty(grsProjectnName)) { pn = grsProjectnName.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.ProjectName)) { nn = iteminlist.ProjectName.ToUpper(); }
                    if (!pn.Contains(nn)) { grsProjectnName = grsProjectnName + ", " + iteminlist.ProjectName; }
                }
                else { grsProjectnName = iteminlist.ProjectName; }
                if (!string.IsNullOrEmpty(grsTestCycleName))
                {
                    string tcn = string.Empty;
                    string ntcn = string.Empty;
                    if (!string.IsNullOrEmpty(grsTestCycleName)) { tcn = grsTestCycleName.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.TestCycleName)) { ntcn = iteminlist.TestCycleName.ToUpper(); }
                    if (!tcn.Contains(ntcn)) { grsTestCycleName = grsTestCycleName + ", " + iteminlist.TestCycleName; }
                }
                else { grsTestCycleName = iteminlist.TestCycleName; }
                if (!string.IsNullOrEmpty(grsTestURL))
                {
                    string tu = string.Empty;
                    string ntu = string.Empty;
                    if (!string.IsNullOrEmpty(grsTestURL)) { tu = grsTestURL.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.TestURL)) { ntu = iteminlist.TestURL.ToUpper(); }
                    if (!tu.Contains(ntu)) { grsTestURL = grsTestURL + ", " + iteminlist.TestURL; }
                }
                else { grsTestURL = iteminlist.TestURL; }
                if (!string.IsNullOrEmpty(grsEnvironment))
                {
                    string en = string.Empty;
                    string nen = string.Empty;
                    if (!string.IsNullOrEmpty(grsEnvironment)) { en = grsEnvironment.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.Environment)) { nen = iteminlist.Environment.ToUpper(); }
                    if (en.Contains(nen)) { grsEnvironment = grsEnvironment + ", " + iteminlist.Environment; }
                }
                else { grsEnvironment = iteminlist.Environment; }

                FinalTotalExecutionTime = FinalTotalExecutionTime > iteminlist.ExecutionTime ? FinalTotalExecutionTime : iteminlist.ExecutionTime; //FinalTotalExecutionTime + iteminlist.ExecutionTime;
                FinalTotalTestCases = FinalTotalTestCases + iteminlist.TestCases;
                FinalTotalPass = FinalTotalPass + iteminlist.Pass;
                FinalTotalFail = FinalTotalFail + iteminlist.Fail;
                FinalTotalNotCompleted = FinalTotalNotCompleted + iteminlist.NotCompleted;
                FinalTotalNoRun = FinalTotalNoRun + iteminlist.NoRun;
                string tempSecondhtml = SecondLineDynamicInitializer;
                tempSecondhtml = tempSecondhtml.Replace("{TESTSETNAME}", iteminlist.TestSuitName);
                tempSecondhtml = tempSecondhtml.Replace("{REMOTEMACHINEIP}", iteminlist.RemoteMachineIP);
                var listcontainsip = (from x in msglist where x.ToUpper().Contains(iteminlist.RemoteMachineIP) select x).FirstOrDefault();
                if (listcontainsip != null)
                {
                    int index = listcontainsip.IndexOf('-');
                    if (index >= 0)
                    {
                        string errocode = listcontainsip.Substring(index + 1, 3);
                        if (errocode.ToUpper() == "000") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", ""); }
                        else if (errocode.ToUpper() == "101") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Testsuit not found. Testcase result is not updated to the GRS server."); }
                        else if (errocode.ToUpper() == "102") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Invalid Rel link."); }
                        else if (errocode.ToUpper() == "103") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "This is Load balanced REL link."); }
                        else if (errocode.ToUpper() == "104") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Exception occured."); }
                        else if (errocode.ToUpper() == "104") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Client machine did not accept PING request."); }
                        else if (errocode.ToUpper() == "105") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Client machine did not accept PING request."); }
                        else if (errocode.ToUpper() == "106") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "Client machine did not accept PING request."); }
                        else if (errocode.ToUpper() == "107") { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "GAgent at client machine stoped after testcase execution start."); }
                        else { tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", ""); }
                    }
                    else
                    {
                        tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "");
                    }
                }
                else
                {
                    tempSecondhtml = tempSecondhtml.Replace("{ERRORMESSAGE}", "");
                }

                tempSecondhtml = tempSecondhtml.Replace("{OS}", iteminlist.OS);
                tempSecondhtml = tempSecondhtml.Replace("{BROWSER}", iteminlist.Browser);
                tempSecondhtml = tempSecondhtml.Replace("{EXECUTIONTIME}", iteminlist.ExecutionTime.ToString(@"dd\:hh\:mm\:ss"));
                tempSecondhtml = tempSecondhtml.Replace("{TESTCASES}", Convert.ToString(iteminlist.TestCases));
                tempSecondhtml = tempSecondhtml.Replace("{PASS}", Convert.ToString(iteminlist.Pass));

                if (iteminlist.Fail > 0)
                {
                    tempSecondhtml = tempSecondhtml.Replace("{FAIL}", "<span style='color:#FF0000'>" + Convert.ToString(iteminlist.Fail) + "</span>");
                }
                else
                {
                    tempSecondhtml = tempSecondhtml.Replace("{FAIL}", Convert.ToString(iteminlist.Fail));
                }

                tempSecondhtml = tempSecondhtml.Replace("{NOTCOMPLETED}", Convert.ToString(iteminlist.NotCompleted));
                tempSecondhtml = tempSecondhtml.Replace("{NORUN}", Convert.ToString(iteminlist.NoRun));
                tempSecondhtml = tempSecondhtml.Replace("{ERRORDETAILS}", iteminlist.ErrorDetails);
                tempSecondhtml = tempSecondhtml.Replace("{GRSLINK}", iteminlist.GrsLink);
                tempSecondhtml = tempSecondhtml.Replace("{ROWCOLOR}", rowcolorcode);
                SecondLineDynamicHTML = SecondLineDynamicHTML + tempSecondhtml;
            }

            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{PROJECT}", grsProjectnName);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{GRSTESTCYCLE}", grsTestCycleName);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{TESTURL}", grsTestURL);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{ENVIRONMENT}", grsEnvironment);
            totalexecutiontimeinlist = FinalTotalExecutionTime.ToString(@"dd\:hh\:mm\:ss");
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALEXECUTIONTIME}", totalexecutiontimeinlist);
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALSCENARIO/TESTCASES}", Convert.ToString(FinalTotalTestCases));
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALPASS}", Convert.ToString(FinalTotalPass));


            if (FinalTotalFail > 0)
            {
                ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALFAIL}", "<span style='color:#8B0000'>" + Convert.ToString(FinalTotalFail) + "</span>");
            }
            else
            {
                ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALFAIL}", Convert.ToString(FinalTotalFail));
            }


            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALNOTCOMPLETED}", Convert.ToString(FinalTotalNotCompleted));
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALNORUN}", Convert.ToString(FinalTotalNoRun));

            return FirstLineStaticHTML + SecondLineDynamicHTML + ThirdLineStaticHTML;
        }

        public string HTMLForLaunchRELReportEmail(List<RelReturn> RelRelateIDs, List<RelExecutionResponseP2> MachineStatus)
        {
            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            string BaseURLAddressGRSLink = "http://10.236.4.153/nggrs/TestCycle/Index/{TESTCYCLEID}?testSuite={TESTSUITID}";
            string grsProjectnName = string.Empty;
            string grsTestCycleName = string.Empty;
            string grsTestURL = string.Empty;
            string grsEnvironment = string.Empty;
            List<RelRepotTotalTestsetWithExecution> TotalRunDetails = new List<RelRepotTotalTestsetWithExecution>();

            string FirstLineStaticHTML = "<table cellpadding='8' style='border: 1px solid #000000; width: auto!important; font-family: Verdana!important; font-size: 12px; border-collapse: collapse;'><tr><td colspan='2' align='center' style=' background-color: #FF8000; border: 1px solid #000000; color: #FFFFFF;'><b>Project and Environment Details</b></td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Project</td><td  align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{PROJECT}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>GRS Test Cycle</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{GRSTESTCYCLE}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Test URL</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{TESTURL}</td></tr><tr><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>Environment</td><td align='left' style=' border: 1px solid #000000; background-color: #FFFFFF;'>{ENVIRONMENT}</td></tr></table><br /><table cellpadding='8' style='border: 1px solid #000000; width: 100%!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td cellpadding='8' colspan='13' align='center' style='font-weight: 500; background-color: #FF8000; color: #FFFFFF;'><b>Test Summary</b></td></tr><tr><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Feature/Test Set</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Remote Machine IP</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>OS</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Browser</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Execution Time (dd.hh:mm:ss)</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Total Scenario/Test cases</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Pass</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Fail</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Completed</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Run</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Details</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>GRS Link</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Execution Status</td></tr>";

            string SecondLineDynamicInitializer = "<tr><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSETNAME}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{REMOTEMACHINEIP}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{OS}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{BROWSER}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{EXECUTIONTIME}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTCASES}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{PASS}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{FAIL}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NOTCOMPLETED}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NORUN}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'><a href='{ERRORDETAILS}' target='_blank'>Click Here</a></td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{GRSLINK}</td><td align='center' style=' border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{EXECUTIONSTATUS}</td></tr>";
            string SecondLineDynamicHTML = string.Empty;

            string ThirdLineStaticHTML = "<tr><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Total</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALEXECUTIONTIME}&nbsp;(Max of All)</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALSCENARIO/TESTCASES}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALPASS}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #000000; font-weight: 600;'>{TOTALFAIL}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALNOTCOMPLETED}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>{TOTALNORUN}</td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td><td align='center' style=' border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'></td></tr></table><br /><br /><table cellpadding='8' align='left' style='font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td style='font-size: 12px!important; font-weight: 500;'>Note: Inorder to view the complete results from GRS link, user must have already logged in to GRS website.</td></tr></table><br /><br /><br /><br /><br /><br /><table cellpadding='8' style='width: auto!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td align='left' valign='middle' style='color:#DD5A43; font-weight:500; font-size:22px!important; width:40px!important; padding:0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height:32px; width:32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color:#DD5A43; font-weight:600; font-size:22px!important; padding:0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style=' font-weight: 600;padding-top:2px!important;padding-bottom:0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{CURRENTYEAR}", DateTime.Now.Year.ToString());

            TimeSpan FinalTotalExecutionTime = new TimeSpan(0);
            string totalexecutiontimeinlist = string.Empty;
            int FinalTotalTestCases = 0;
            int FinalTotalPass = 0;
            int FinalTotalFail = 0;
            int FinalTotalNotCompleted = 0;
            int FinalTotalNoRun = 0;

            var executionidlist = (from x in RelRelateIDs select x.ExecutionId).Distinct().ToList();

            var newrelexetestsuitlist = (from x in executionidlist
                                         select new RELREPEmail
                                         {
                                             EXECUTIONID = x,
                                             TESTSUITID = (from y in RelRelateIDs where y.ExecutionId == x select y.TestSuitId).ToList()
                                         }).ToList();


            foreach (var item in newrelexetestsuitlist)
            {
                var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == item.EXECUTIONID);
                if (execution != null)
                {
                    int executionId = execution.ID;

                    var pdata = _projectRepository.GetSingleOrDefault(p => p.ID == execution.BelongToProject);
                    int projectId = pdata.ID;
                    string projectName = pdata.ProjectName;

                    var tcdata = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == execution.TargetTestCycle);
                    int testCycleId = tcdata.ID;
                    string testcycleName = tcdata.CycleName;

                    string testsuitName = string.Empty;
                    string url = string.Empty;
                    string environment = string.Empty;
                    string ip = string.Empty;
                    string browser = string.Empty;
                    string os = string.Empty;

                    List<TestSuit> InternalTestSuitList = new List<TestSuit>();
                    foreach (var itemts in item.TESTSUITID)
                    {
                        var tsdata = _testSuitsRepository.GetSingleOrDefault(TS => TS.ID == itemts);
                        if (tsdata != null)
                        {
                            var istestcasedataexiset = _testCaseRepository.Count(tt => tt.ParentTestSuite == tsdata.ID);
                            DateTime starttime = tsdata.TSStart;
                            DateTime endtime = tsdata.DeliveryTime ?? DateTime.Now;
                            if (istestcasedataexiset > 0)
                            {
                                starttime = HRRP.GetMinStartDateTime_Testcase_Testsuit(tsdata.ID);// tsdata.TSStart,
                                endtime = HRRP.GetMaxEndDateTime_Testcase_Testsuit(tsdata.ID);// tsdata.DeliveryTime,
                            }
                            InternalTestSuitList.Add(new TestSuit()
                            {
                                ID = tsdata.ID,
                                TSName = tsdata.TSName,
                                TSStart = starttime,
                                DeliveryTime = endtime,
                                ParentClientInfo = tsdata.ParentClientInfo
                            });
                        }
                    }

                    DateTime end = (DateTime)SqlDateTime.Null;
                    DateTime start = DateTime.Now;
                    int tscount = 1;
                    int localparentclientinfo = 0;
                    string tslinkstr = string.Empty;

                    List<TestCase> testcasedetails = new List<TestCase>();
                    foreach (var internalcalTS in InternalTestSuitList)
                    {
                        Int64 testsuitId = internalcalTS.ID;
                        if (tscount == 1)
                        {
                            localparentclientinfo = internalcalTS.ParentClientInfo ?? 0;
                            start = internalcalTS.TSStart;
                            end = internalcalTS.DeliveryTime ?? DateTime.Now;
                            tslinkstr = internalcalTS.ID.ToString();
                        }
                        else
                        {
                            DateTime tempend = internalcalTS.DeliveryTime ?? DateTime.Now;
                            end = end > tempend ? end : tempend;
                            DateTime tempstart = internalcalTS.TSStart;
                            start = start < tempstart ? start : tempstart;
                            tslinkstr = tslinkstr + "," + internalcalTS.ID.ToString();
                        }

                        var TCLIST = _testCaseRepository.GetAllToList(TC => TC.ParentTestSuite == testsuitId).ToList();
                        testcasedetails.AddRange(TCLIST);

                        tscount = tscount + 1;
                    }

                    string ErrorDetails = "http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport?exid={EXECUTIONID}&tsid={TESTSUITID}";
                    ErrorDetails = ErrorDetails.Replace("{EXECUTIONID}", executionId.ToString());
                    ErrorDetails = ErrorDetails.Replace("{TESTSUITID}", tslinkstr);
                    string GRSLink = string.Empty;
                    if (InternalTestSuitList.Count == 1)
                    {
                        testsuitName = InternalTestSuitList[0].TSName;
                        string tempGRSLINK = BaseURLAddressGRSLink;
                        tempGRSLINK = tempGRSLINK.Replace("{TESTCYCLEID}", testCycleId.ToString());
                        tempGRSLINK = tempGRSLINK.Replace("{TESTSUITID}", InternalTestSuitList[0].ID.ToString());
                        string finalGRSLINK = "<a href='{GRSLINK}' target='_blank'>Click Here</a>";
                        finalGRSLINK = finalGRSLINK.Replace("{GRSLINK}", tempGRSLINK);
                        GRSLink = finalGRSLINK;
                    }
                    else if (InternalTestSuitList.Count > 1)
                    {
                        testsuitName = "<a href='{TSLINK}' target='_blank'>Test suit / Feature</a>";
                        testsuitName = testsuitName.Replace("{TSLINK}", ErrorDetails);
                        string GRSLINKREDIRECT = "<a href='{GRSLINK}' target='_blank'>Click Here</a>";
                        GRSLINKREDIRECT = GRSLINKREDIRECT.Replace("{GRSLINK}", ErrorDetails);
                        GRSLink = GRSLINKREDIRECT;
                    }
                    else { GRSLink = string.Empty; }

                    var cinfodetails = _clientsInformationRepository.GetSingleOrDefault(CI => CI.ID == localparentclientinfo);
                    if (cinfodetails != null)
                    {
                        url = cinfodetails.ClientURL;
                        ip = cinfodetails.ClientIP;
                        browser = cinfodetails.ClientBrowser;
                        os = cinfodetails.ClientOS;
                    }
                    int profilehost = execution.ProfileHost ?? 0;
                    if (profilehost > 0)
                    {
                        environment = _hostsConfiguration.GetFirstOrDefault(a => a.ID == profilehost).EnvironmentName;
                    }

                    TimeSpan executionTime = new TimeSpan();
                    executionTime = (end - start);
                    int totaltestcases = testcasedetails.Count;
                    int totalpass = testcasedetails.Count(t => t.TCState == "pass");
                    int totalfail = testcasedetails.Count(t => t.TCState == "fail");
                    int totalnotcompleted = testcasedetails.Count(t => t.TCState == "notcompleted");
                    int totalnorun = testcasedetails.Count(t => t.TCState == "norun");

                    TotalRunDetails.Add(new RelRepotTotalTestsetWithExecution()
                    {
                        ExecutionID = executionId,
                        ProjectID = projectId,
                        ProjectName = projectName,
                        TestCycleID = testCycleId,
                        TestCycleName = testcycleName,
                        TestSuitName = testsuitName,
                        RemoteMachineIP = ip,
                        OS = os,
                        Browser = browser,
                        TestURL = url,
                        Environment = environment,
                        ExecutionTime = executionTime,
                        TestCases = totaltestcases,
                        Pass = totalpass,
                        Fail = totalfail,
                        NotCompleted = totalnotcompleted,
                        NoRun = totalnorun,
                        ErrorDetails = ErrorDetails,
                        GrsLink = GRSLink
                    });
                }
            }

            int count = 0;

            foreach (var iteminlist in TotalRunDetails)
            {
                count = count + 1;
                string rowcolorcode = ((count % 2) == 0) ? "#DCDCDC" : "#FFFFFF";

                if (!string.IsNullOrEmpty(grsProjectnName))
                {
                    string pn = string.Empty;
                    string nn = string.Empty;
                    if (!string.IsNullOrEmpty(grsProjectnName)) { pn = grsProjectnName.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.ProjectName)) { nn = iteminlist.ProjectName.ToUpper(); }
                    if (!pn.Contains(nn)) { grsProjectnName = grsProjectnName + ", " + iteminlist.ProjectName; }
                }
                else { grsProjectnName = iteminlist.ProjectName; }

                if (!string.IsNullOrEmpty(grsTestCycleName))
                {
                    string tcn = string.Empty;
                    string ntcn = string.Empty;
                    if (!string.IsNullOrEmpty(grsTestCycleName)) { tcn = grsTestCycleName.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.TestCycleName)) { ntcn = iteminlist.TestCycleName.ToUpper(); }
                    if (!tcn.Contains(ntcn)) { grsTestCycleName = grsTestCycleName + ", " + iteminlist.TestCycleName; }
                }
                else { grsTestCycleName = iteminlist.TestCycleName; }

                if (!string.IsNullOrEmpty(grsTestURL))
                {
                    string tu = string.Empty;
                    string ntu = string.Empty;
                    if (!string.IsNullOrEmpty(grsTestURL)) { tu = grsTestURL.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.TestURL)) { ntu = iteminlist.TestURL.ToUpper(); }
                    if (!tu.Contains(ntu)) { grsTestURL = grsTestURL + ", " + iteminlist.TestURL; }
                }
                else { grsTestURL = iteminlist.TestURL; }

                if (!string.IsNullOrEmpty(grsEnvironment))
                {
                    string en = string.Empty;
                    string nen = string.Empty;
                    if (!string.IsNullOrEmpty(grsEnvironment)) { en = grsEnvironment.ToUpper(); }
                    if (!string.IsNullOrEmpty(iteminlist.Environment)) { nen = iteminlist.Environment.ToUpper(); }
                    if (en.Contains(nen)) { grsEnvironment = grsEnvironment + ", " + iteminlist.Environment; }
                }
                else { grsEnvironment = iteminlist.Environment; }

                FinalTotalExecutionTime = FinalTotalExecutionTime > iteminlist.ExecutionTime ? FinalTotalExecutionTime : iteminlist.ExecutionTime; //FinalTotalExecutionTime + iteminlist.ExecutionTime;
                FinalTotalTestCases = FinalTotalTestCases + iteminlist.TestCases;
                FinalTotalPass = FinalTotalPass + iteminlist.Pass;
                FinalTotalFail = FinalTotalFail + iteminlist.Fail;
                FinalTotalNotCompleted = FinalTotalNotCompleted + iteminlist.NotCompleted;
                FinalTotalNoRun = FinalTotalNoRun + iteminlist.NoRun;
                string tempSecondhtml = SecondLineDynamicInitializer;
                tempSecondhtml = tempSecondhtml.Replace("{TESTSETNAME}", iteminlist.TestSuitName);
                tempSecondhtml = tempSecondhtml.Replace("{REMOTEMACHINEIP}", iteminlist.RemoteMachineIP);
                string machinelaststatus = string.Empty;
                var machinestatusdata = (from x in MachineStatus where x.MachineIP.ToUpper() == iteminlist.RemoteMachineIP.ToUpper() select x).FirstOrDefault();
                if (machinestatusdata != null)
                {
                    machinelaststatus = machinestatusdata.Status.ToUpper();
                }
                tempSecondhtml = tempSecondhtml.Replace("{EXECUTIONSTATUS}", machinelaststatus);
                tempSecondhtml = tempSecondhtml.Replace("{OS}", iteminlist.OS);
                tempSecondhtml = tempSecondhtml.Replace("{BROWSER}", iteminlist.Browser);
                tempSecondhtml = tempSecondhtml.Replace("{EXECUTIONTIME}", iteminlist.ExecutionTime.ToString(@"dd\:hh\:mm\:ss"));
                tempSecondhtml = tempSecondhtml.Replace("{TESTCASES}", Convert.ToString(iteminlist.TestCases));
                tempSecondhtml = tempSecondhtml.Replace("{PASS}", Convert.ToString(iteminlist.Pass));

                if (iteminlist.Fail > 0)
                {
                    tempSecondhtml = tempSecondhtml.Replace("{FAIL}", "<span style='color:#FF0000'>" + Convert.ToString(iteminlist.Fail) + "</span>");
                }
                else
                {
                    tempSecondhtml = tempSecondhtml.Replace("{FAIL}", Convert.ToString(iteminlist.Fail));
                }


                tempSecondhtml = tempSecondhtml.Replace("{NOTCOMPLETED}", Convert.ToString(iteminlist.NotCompleted));
                tempSecondhtml = tempSecondhtml.Replace("{NORUN}", Convert.ToString(iteminlist.NoRun));
                tempSecondhtml = tempSecondhtml.Replace("{ERRORDETAILS}", iteminlist.ErrorDetails);
                tempSecondhtml = tempSecondhtml.Replace("{GRSLINK}", iteminlist.GrsLink);
                tempSecondhtml = tempSecondhtml.Replace("{ROWCOLOR}", rowcolorcode);
                SecondLineDynamicHTML = SecondLineDynamicHTML + tempSecondhtml;
            }

            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{PROJECT}", grsProjectnName);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{GRSTESTCYCLE}", grsTestCycleName);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{TESTURL}", grsTestURL);
            FirstLineStaticHTML = FirstLineStaticHTML.Replace("{ENVIRONMENT}", grsEnvironment);
            totalexecutiontimeinlist = FinalTotalExecutionTime.ToString(@"dd\:hh\:mm\:ss");
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALEXECUTIONTIME}", totalexecutiontimeinlist);
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALSCENARIO/TESTCASES}", Convert.ToString(FinalTotalTestCases));
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALPASS}", Convert.ToString(FinalTotalPass));

            if (FinalTotalFail > 0)
            {
                ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALFAIL}", "<span style='color:#8B0000'>" + Convert.ToString(FinalTotalFail) + "</span>");
            }
            else
            {
                ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALFAIL}", Convert.ToString(FinalTotalFail));
            }



            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALNOTCOMPLETED}", Convert.ToString(FinalTotalNotCompleted));
            ThirdLineStaticHTML = ThirdLineStaticHTML.Replace("{TOTALNORUN}", Convert.ToString(FinalTotalNoRun));

            return FirstLineStaticHTML + SecondLineDynamicHTML + ThirdLineStaticHTML;
        }

        public string HTMLForLaunchRELReportBrowser(int executionId, Int64 testsuitId)
        {
            List<RELReportErrorDetailsBrowser> TestCaseList = new List<RELReportErrorDetailsBrowser>();
            string HTMLFirstLine = "<html><head><title>GRS REPORT - Error Details</title><style type='text/css'>table {border: 1px solid #000000;width: 100%!important;font-family: Verdana!important;font-size: 12px!important;border-collapse: collapse; }ul {list-style-type: none!important;-webkit-margin-before: 0px!important;-webkit-margin-after: 0px!important;-webkit-margin-start: 0px!important;-webkit-margin-end: 0px!important;-webkit-padding-start: 0px!important;}</style></head><body><table><tr><td align='center' style='padding: 10px!important; background-color: #D3D3D3; border: 1px solid #000000;'><u><b>Error Details</b></u></td></tr>";
            string HTMLLastLine = "</table></body></html>";

            string HTMLMiddleLineInitializerOuterStart = "<tr><td align='center' style='padding: 10px!important;'><table><tr><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #FF8000; color: #FFFFFF; font-weight: 600;'>Test Set/Feature&nbsp;:&nbsp;{TESTSETNAME}</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #FF8000; color: #FFFFFF; font-weight: 600;'>Scenario/Test Case&nbsp;:&nbsp;{TESTCASENAME}</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #FF8000; color: #FFFFFF; font-weight: 600;'>Scenario/Test Case Status&nbsp;:&nbsp;{TESTCASESTATUS}</td></tr><tr><td colspan='3' style='padding: 10px!important;'><table><tr><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Test Step</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Status</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Expected</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Input Data</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Error Log(s)</td><td align='left' style='padding: 5px!important; border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Screen Shot(s)</td></tr>";
            string HTMLMiddleLineInitializerOuterMiddle = "<tr><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPNAME}</td><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPSTATUS}</td><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPEXPECTED}</td><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPINPUTDATA}</td><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPATTACHMENT}</td><td align='left' valign='top' style='padding: 5px!important; border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSTEPSCREENSHOT}</td></tr>";
            string HTMLMiddleLineInitializerOuterLast = "</table></td></tr></table></td></tr><tr><td style='padding:5px!important;'></td></tr>";


            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            List<Int64> tids = new List<Int64>();
            tids.Add(testsuitId);
            List<int> projectIdlist = HRRP.GetDistinctListOfProjectId(tids);
            List<int> testcycleIdlist = HRRP.GetDistinctListOfTestCycleId(tids);

            if (projectIdlist.Count == 1 && testcycleIdlist.Count == 1)
            {
                int localprojid = projectIdlist[0];
                int localtestcycleid = testcycleIdlist[0];
                //List<int> localclientinfolist = HRRP.GetDistinctListOfClientInfoId(tids);
                //string localenvname = HRRP.ListOfEnv(localprojid, localclientinfolist);


            //var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == executionId);
            //if (execution != null)
            //{
            //    var pdata = _projectRepository.GetSingleOrDefault(p => p.ID == execution.BelongToProject);
            //    string project = pdata.ProjectName;

            //    string test_cycle = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == execution.TargetTestCycle).CycleName;


             
                var pdata = _projectRepository.GetSingleOrDefault(p => p.ID == localprojid);
                string project = pdata.ProjectName;

                string test_cycle = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == localtestcycleid).CycleName;

                var testsuitdetails = _testSuitsRepository.GetSingleOrDefault(TS => TS.ID == testsuitId);
                string TestSuitName = testsuitdetails.TSName;

                var clientinformationdetails = _clientsInformationRepository.GetSingleOrDefault(CI => CI.ID == testsuitdetails.ParentClientInfo);

                bool IsShowAllPassed = true;
                var testcasedetails = _testCaseRepository.GetAllToList(TC => TC.ParentTestSuite == testsuitdetails.ID && (TC.TCState == "fail" || TC.TCState == "norun" || TC.TCState == "notcompleted"));
                foreach (var TestcaseItem in testcasedetails)
                {
                    IsShowAllPassed = false;
                    List<RELReportTestStepBrowser> relreportstep = new List<RELReportTestStepBrowser>();

                    string TestCaseName = TestcaseItem.TCName;
                    string TestCaseState = TestcaseItem.TCState;

                    var teststepdetails = _testStepRepository.GetAllToList(TS => TS.ParentTestCase == TestcaseItem.ID && (TS.StepType == "comment" || TS.StepType == "fail" || TS.StepType == "norun"));
                    foreach (var teststepItem in teststepdetails)
                    {
                        string TestStepName = teststepItem.StepDescription;
                        string TestStepStatus = teststepItem.StepType;
                        string TestStepExpected = teststepItem.StepExpected;
                        string InputData = teststepItem.StepInputData;
                        string TestCaseAttachment = teststepItem.Attachments;
                        string TestSubStepScreenShots = string.Empty;

                        var substepdetials = _substepRepository.GetAllToList(SS => SS.ParentStep == teststepItem.ID && SS.SubStepValid == false);

                        foreach (var substepItem in substepdetials)
                        {
                            string screenshot = substepItem.SubStepScreenShot;
                            string screenshotdriver = substepItem.SubStepScreenShotDriver;
                            if (!string.IsNullOrEmpty(screenshot))
                            {
                                if (!string.IsNullOrEmpty(TestSubStepScreenShots))
                                    TestSubStepScreenShots = TestSubStepScreenShots + ";" + screenshot;
                                else
                                    TestSubStepScreenShots = screenshot;
                            }
                            if (!string.IsNullOrEmpty(screenshotdriver))
                            {
                                if (!string.IsNullOrEmpty(TestSubStepScreenShots))
                                    TestSubStepScreenShots = TestSubStepScreenShots + ";" + screenshotdriver;
                                else
                                    TestSubStepScreenShots = screenshotdriver;
                            }
                        }

                        relreportstep.Add(new RELReportTestStepBrowser()
                        {
                            ProjectID = Convert.ToString(testsuitdetails.ParentProject),
                            TestStepName = TestStepName,
                            TestStepStatus = (!string.IsNullOrEmpty(TestStepStatus)) ? TestStepStatus.ToUpper() : "",
                            TestStepExpected = TestStepExpected,
                            InputData = InputData,
                            TestCaseAttachment = TestCaseAttachment,
                            TestSubStepScreenShots = TestSubStepScreenShots,
                        });
                    }

                    TestCaseList.Add(new RELReportErrorDetailsBrowser()
                    {
                        TestSetName = TestSuitName,
                        TestCaseName = TestCaseName,
                        TestCaseStatus = (!string.IsNullOrEmpty(TestCaseState)) ? TestCaseState.ToUpper() : "",
                        TestStepsDetails = relreportstep
                    });
                }

                string FINALInternalHTMLSTRING = string.Empty;

                foreach (var item in TestCaseList)
                {
                    string s1 = HTMLMiddleLineInitializerOuterStart;

                    string s3 = HTMLMiddleLineInitializerOuterLast;

                    s1 = s1.Replace("{TESTSETNAME}", item.TestSetName);
                    s1 = s1.Replace("{TESTCASENAME}", item.TestCaseName);
                    s1 = s1.Replace("{TESTCASESTATUS}", item.TestCaseStatus);

                    FINALInternalHTMLSTRING = FINALInternalHTMLSTRING + s1;
                    int count = 0;
                    foreach (var subitem in item.TestStepsDetails)
                    {
                        string s2 = HTMLMiddleLineInitializerOuterMiddle;
                        count = count + 1;
                        string rowcolor = ((count % 2) == 0) ? "#DCDCDC" : "#FFFFFF";
                        s2 = s2.Replace("{TESTSTEPNAME}", subitem.TestStepName);
                        s2 = s2.Replace("{TESTSTEPSTATUS}", subitem.TestStepStatus);
                        s2 = s2.Replace("{TESTSTEPEXPECTED}", subitem.TestStepExpected);
                        s2 = s2.Replace("{TESTSTEPINPUTDATA}", subitem.InputData);
                        s2 = s2.Replace("{ROWCOLOR}", rowcolor);

                        string attachmentpart = string.Empty;
                        if (!string.IsNullOrEmpty(subitem.TestCaseAttachment))
                        {
                            string[] attch = subitem.TestCaseAttachment.Split(';');
                            List<string> ullist = new List<string>(attch);
                            string attachmentUL = "<ul style='list-style-type: none!important;-webkit-margin-before: 0px!important;-webkit-margin-after: 0px!important;-webkit-margin-start: 0px!important;-webkit-margin-end: 0px!important;-webkit-padding-start: 0px!important;'>";
                            bool isdataattach = false;
                            int logcount = 0;
                            foreach (var lilist in ullist)
                            {
                                if (!string.IsNullOrEmpty(lilist))
                                {
                                    logcount = logcount + 1;
                                    string attachlink = "http://10.236.4.153/nggrs/harvest/Attachments/{PROJID}/{ATTACHFILENAME}";
                                    attachlink = attachlink.Replace("{PROJID}", subitem.ProjectID);
                                    attachlink = attachlink.Replace("{ATTACHFILENAME}", lilist);

                                    string attachin = "<li><a href='{ATTCHLINK}' target='_blank'>Log " + logcount + "</a></li>";
                                    attachin = attachin.Replace("{ATTCHLINK}", attachlink);
                                    attachmentUL = attachmentUL + attachin;
                                    isdataattach = true;
                                }
                            }
                            attachmentUL = attachmentUL + "</ul>";
                            if (isdataattach == false)
                                attachmentUL = string.Empty;
                            attachmentpart = attachmentUL;
                        }
                        s2 = s2.Replace("{TESTSTEPATTACHMENT}", attachmentpart);

                        string screenshotpart = string.Empty;
                        if (!string.IsNullOrEmpty(subitem.TestSubStepScreenShots))
                        {
                            string[] shot = subitem.TestSubStepScreenShots.Split(';');
                            List<string> shotlist = new List<string>(shot);
                            string shotUL = "<ul style='list-style-type: none!important;-webkit-margin-before: 0px!important;-webkit-margin-after: 0px!important;-webkit-margin-start: 0px!important;-webkit-margin-end: 0px!important;-webkit-padding-start: 0px!important;'>";
                            bool isshotdataattach = false;
                            int shotcount = 0;
                            foreach (var solist in shotlist)
                            {
                                if (!string.IsNullOrEmpty(solist))
                                {
                                    shotcount = shotcount + 1;
                                    string shotlink = "http://10.236.4.153/nggrs/harvest/ScreenShots/{SHOTFILE}";
                                    shotlink = shotlink.Replace("{SHOTFILE}", solist);

                                    string shotin = "<li><a href='{SHOTLINK}' target='_blank'>Screenshot " + shotcount + "</a></li>";
                                    shotin = shotin.Replace("{SHOTLINK}", shotlink);
                                    shotUL = shotUL + shotin;
                                    isshotdataattach = true;
                                }
                            }
                            shotUL = shotUL + "</ul>";
                            if (isshotdataattach == false)
                                shotUL = string.Empty;
                            screenshotpart = shotUL;
                        }

                        s2 = s2.Replace("{TESTSTEPSCREENSHOT}", screenshotpart);
                        FINALInternalHTMLSTRING = FINALInternalHTMLSTRING + s2;
                    }


                    FINALInternalHTMLSTRING = FINALInternalHTMLSTRING + s3;
                }
                if (IsShowAllPassed == false)
                {
                    return HTMLFirstLine + FINALInternalHTMLSTRING + HTMLLastLine;
                }
                else
                {
                    string L1 = "<html><head><title>GRS REPORT</title><style type='text/css'>table {border: 1px solid #000000;width: 100%!important;font-family: Verdana!important;font-size: 12px!important;border-collapse: collapse; }</style></head><body><table style='border: 1px solid #000000; width: auto!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse; '><tr><td align='center' style='padding: 10px!important; background-color: #D3D3D3; border: 1px solid #000000; font-size: 16px;'>All the Testcase Under the TestSuit ({TESTSUITNAME}) passed.</td></tr><tr><td align='center' style='padding: 10px!important; background-color: #D3D3D3; border: 1px solid #000000;'><table><tr><td colspan='2' align='center' style='padding: 10px!important; background-color: #FF8000; color: #FFFFFF; font-weight: 500; font-size: 14px; border: 1px solid #000000'>Testcase/Scenario Details</td></tr><tr><td align='left' style='padding: 5px!important; background-color: #828282; color: #FFFFFF; font-weight: 600; border: 1px solid #000000'>Testcase/Scenario Name</td><td align='left' style='padding: 5px!important; background-color: #828282; color: #FFFFFF; font-weight: 600; border: 1px solid #000000'>Status</td></tr>";
                    string L2Dynamic = "<tr><td align='left' style='padding: 5px!important; background-color: #FFFFFF; color: #000000; font-weight: 500; border: 1px solid #000000'>{TESTCASENAME}</td><td align='left' style='padding: 5px!important; background-color: #FFFFFF; color: #000000; font-weight: 500; border: 1px solid #000000'>{TESTCASESTATUS}</td></tr>";
                    string L2 = string.Empty;
                    string L3 = "</table></td></tr></table></body></html>";

                    L1 = L1.Replace("{TESTSUITNAME}", testsuitdetails.TSName);
                    var testcasedetailsAllPassed = _testCaseRepository.GetAllToList(TC => TC.ParentTestSuite == testsuitdetails.ID);

                    foreach (var TestcaseItem in testcasedetailsAllPassed)
                    {
                        string L2TEMP = L2Dynamic;
                        L2TEMP = L2TEMP.Replace("{TESTCASENAME}", TestcaseItem.TCName);
                        L2TEMP = L2TEMP.Replace("{TESTCASESTATUS}", TestcaseItem.TCState.ToUpper());
                        L2 = L2 + L2TEMP;
                    }
                    return L1 + L2 + L3;
                }
            }
            else
            {
                return "<html><head><title>GRS REPORT - Error Details</title><style type='text/css'>table {border: 1px solid #000000;width: 100%!important;font-family: Verdana!important;font-size: 12px!important;border-collapse: collapse; }ul {list-style-type: none!important;-webkit-margin-before: 0px!important;-webkit-margin-after: 0px!important;-webkit-margin-start: 0px!important;-webkit-margin-end: 0px!important;-webkit-padding-start: 0px!important;}</style></head><body><table><tr><td align='center' style='padding: 10px!important; background-color: #D3D3D3;'><u><b>Error Details</b></u></td></tr><tr><td align='center' style='padding: 10px!important; background-color: #FF0000; color:#FFFFFF;'>Invalid Parameter. Please check your URL</td></tr></table></body></html>";
            }
        }

        public string HTMLForLaunchRELTestSuitDetails(int executionId, List<Int64> testsuitId)
        {
            string FirstLineHTML = "<html><head><title>GRS REPORT</title></head><body><table cellpadding='8' style='border: 1px solid #000000; width: auto!important; font-family: Verdana!important; font-size: 12px; border-collapse: collapse;'><tr><td colspan='2' align='center' style='background-color: #FF8000; border: 1px solid #000000; color: #FFFFFF;'><b>Project and Environment Details</b></td></tr><tr><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>Project</td><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>{PROJECT}</td></tr><tr><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>GRS Test Cycle</td><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>{GRSTESTCYCLE}</td></tr><tr><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>Test URL</td><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>{TESTURL}</td></tr><tr><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>Environment</td><td align='left' style='border: 1px solid #000000; background-color: #FFFFFF;'>{ENVIRONMENT}</td></tr></table><br /><table cellpadding='8' style='border: 1px solid #000000; width: 100%!important; font-family: Verdana!important; font-size: 12px!important; border-collapse: collapse;'><tr><td cellpadding='8' colspan='12' align='center' style='font-weight: 500; background-color: #FF8000; color: #FFFFFF;'><b>Test Summary</b></td></tr><tr><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Feature/Test Set</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Remote Machine IP</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>OS</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Browser</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Execution Time (dd.hh:mm:ss)</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Total Scenario/Test cases</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Pass</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Fail</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Completed</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>No Run</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>Details</td><td align='center' style='border: 1px solid #000000; background-color: #828282; color: #FFFFFF; font-weight: 600;'>GRS Link</td></tr>";
            string SecondLineHTMLStatic = "<tr><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTSETNAME}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{REMOTEMACHINEIP}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{OS}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{BROWSER}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{EXECUTIONTIME}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{TESTCASES}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{PASS}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{FAIL}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NOTCOMPLETED}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'>{NORUN}</td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'><a href='{ERRORDETAILS}' target='_blank'>Click Here</a></td><td align='center' style='border: 1px solid #000000; background-color: {ROWCOLOR}; color: #000000; font-weight: 500;'><a href='{GRSLINK}' target='_blank'>Click Here</a></td></tr>";
            string ThirdLineHTML = "</table></body></html>";
            string secondTemp = string.Empty;


            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            List<int> projectIdlist = HRRP.GetDistinctListOfProjectId(testsuitId);
            List<int> testcycleIdlist = HRRP.GetDistinctListOfTestCycleId(testsuitId);

            if (projectIdlist.Count == 1 && testcycleIdlist.Count == 1)
            {
                int localprojid = projectIdlist[0];
                int localtestcycleid = testcycleIdlist[0];
                List<int> localclientinfolist = HRRP.GetDistinctListOfClientInfoId(testsuitId);
                string localenvname = HRRP.ListOfEnv(localprojid, localclientinfolist);
            //}

            ////Get Execution Id related information
            //var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == executionId);
            //if (execution != null)
            //{


                //var project = _projectRepository.GetSingleOrDefault(p => p.ID == execution.BelongToProject);
                //var test_cycle = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == execution.TargetTestCycle);
                //string TestURL = string.Empty;
                //string Environment = string.Empty;

                //int profilehost = execution.ProfileHost ?? 0;
                //if (profilehost > 0)
                //{
                //    Environment = _hostsConfiguration.GetFirstOrDefault(a => a.ID == profilehost).EnvironmentName;
                //}

                var project = _projectRepository.GetSingleOrDefault(p => p.ID == localprojid);
                var test_cycle = _testCycleRepository.GetSingleOrDefault(TC => TC.ID == localtestcycleid);
                string TestURL = string.Empty;
                string Environment = localenvname;

                int count = 0;
                foreach (var item in testsuitId)
                {
                    count = count + 1;
                    string rowcolorcode = ((count % 2) == 0) ? "#DCDCDC" : "#FFFFFF";

                    var testsuitdetails = _testSuitsRepository.GetSingleOrDefault(TS => TS.ID == item);
                    if (testsuitdetails != null)
                    {
                        var clientinformationdetails = _clientsInformationRepository.GetSingleOrDefault(CI => CI.ID == testsuitdetails.ParentClientInfo);
                        var testcasedetails = _testCaseRepository.GetAllToList(TC => TC.ParentTestSuite == testsuitdetails.ID);

                        if (!string.IsNullOrEmpty(TestURL))
                        {
                            if (!TestURL.Contains(clientinformationdetails.ClientURL))
                                TestURL = TestURL + "; " + clientinformationdetails.ClientURL;
                        }
                        else
                        {
                            TestURL = clientinformationdetails.ClientURL;
                        }

                        string ErrorDetails = "http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport?exid={EXECUTIONID}&tsid={TESTSUITID}";
                        ErrorDetails = ErrorDetails.Replace("{EXECUTIONID}", executionId.ToString());
                        ErrorDetails = ErrorDetails.Replace("{TESTSUITID}", item.ToString());
                        string GRSLink = "http://10.236.4.153/nggrs/TestCycle/Index/{TESTCYCLEID}?testSuite={TESTSUITID}";
                        GRSLink = GRSLink.Replace("{TESTCYCLEID}", test_cycle.ID.ToString());
                        GRSLink = GRSLink.Replace("{TESTSUITID}", item.ToString());
                        string innerSecondtemp = SecondLineHTMLStatic;
                        innerSecondtemp = innerSecondtemp.Replace("{TESTSETNAME}", testsuitdetails.TSName);
                        innerSecondtemp = innerSecondtemp.Replace("{REMOTEMACHINEIP}", clientinformationdetails.ClientIP);
                        innerSecondtemp = innerSecondtemp.Replace("{OS}", clientinformationdetails.ClientOS);
                        innerSecondtemp = innerSecondtemp.Replace("{BROWSER}", clientinformationdetails.ClientBrowser);
                        //DateTime end = testsuitdetails.DeliveryTime ?? DateTime.Now;
                        //DateTime start = testsuitdetails.TSStart;
                        //TimeSpan span = new TimeSpan();
                        //span = end - start;

                        //Custom
                        DateTime startdatetime = testsuitdetails.TSStart;
                        DateTime enddatetime = testsuitdetails.DeliveryTime ?? DateTime.Now;

                        var testcasecountforts = _testCaseRepository.Count(tc => tc.ParentTestSuite == testsuitdetails.ID);
                        //HelperRelReportProvider HRRP = new HelperRelReportProvider();
                        if (testcasecountforts > 0)
                        {
                            startdatetime = HRRP.GetMinStartDateTime_Testcase_Testsuit(item);
                            enddatetime = HRRP.GetMaxEndDateTime_Testcase_Testsuit(item);
                        }

                        DateTime minStartTC = startdatetime;
                        DateTime maxEndTC = enddatetime;
                        TimeSpan span = new TimeSpan();
                        span = (maxEndTC - minStartTC);


                        innerSecondtemp = innerSecondtemp.Replace("{EXECUTIONTIME}", span.ToString(@"dd\:hh\:mm\:ss"));
                        int totalfail = (from x in testcasedetails where x.TCState == "fail" select x).Count();
                        int totalnorun = (from x in testcasedetails where x.TCState == "norun" select x).Count();
                        int totalnotcompleted = (from x in testcasedetails where x.TCState == "notcompleted" select x).Count();
                        int totalpass = (from x in testcasedetails where x.TCState == "pass" select x).Count();
                        innerSecondtemp = innerSecondtemp.Replace("{TESTCASES}", testcasedetails.Count.ToString());
                        innerSecondtemp = innerSecondtemp.Replace("{PASS}", totalpass.ToString());

                        if (totalfail > 0)
                        {
                            innerSecondtemp = innerSecondtemp.Replace("{FAIL}", "<span style='color:#FF0000'>" + totalfail.ToString() + "</span>");
                        }
                        else
                        {
                            innerSecondtemp = innerSecondtemp.Replace("{FAIL}", totalfail.ToString());
                        }


                        innerSecondtemp = innerSecondtemp.Replace("{NOTCOMPLETED}", totalnotcompleted.ToString());
                        innerSecondtemp = innerSecondtemp.Replace("{NORUN}", totalnorun.ToString());
                        innerSecondtemp = innerSecondtemp.Replace("{ERRORDETAILS}", ErrorDetails);
                        innerSecondtemp = innerSecondtemp.Replace("{GRSLINK}", GRSLink);
                        innerSecondtemp = innerSecondtemp.Replace("{ROWCOLOR}", rowcolorcode);
                        secondTemp = secondTemp + innerSecondtemp;
                    }
                }

                FirstLineHTML = FirstLineHTML.Replace("{PROJECT}", project.ProjectName);
                FirstLineHTML = FirstLineHTML.Replace("{GRSTESTCYCLE}", test_cycle.CycleName);
                FirstLineHTML = FirstLineHTML.Replace("{TESTURL}", TestURL);
                FirstLineHTML = FirstLineHTML.Replace("{ENVIRONMENT}", Environment);
                return FirstLineHTML + secondTemp + ThirdLineHTML;
            }
            else
            {
                return "<html><head><title>GRS REPORT - Error Details</title><style type='text/css'>table {border: 1px solid #000000;width: 100%!important;font-family: Verdana!important;font-size: 12px!important;border-collapse: collapse; }ul {list-style-type: none!important;-webkit-margin-before: 0px!important;-webkit-margin-after: 0px!important;-webkit-margin-start: 0px!important;-webkit-margin-end: 0px!important;-webkit-padding-start: 0px!important;}</style></head><body><table><tr><td align='center' style='padding: 10px!important; background-color: #D3D3D3;'><u><b>Error Details</b></u></td></tr><tr><td align='center' style='padding: 10px!important; background-color: #FF0000; color:#FFFFFF;'>Invalid Parameter. Please check your URL</td></tr></table></body></html>";
            }
        }

        #endregion Launch Rel Link

        public RelExecutionStatusLastAndCount GetLastIdAndCountOfRelExecutionStatus(Int64 executionId, string MachineIP)
        {
            var relexecution = _relExecutionStatus.GetAllToList(s => s.ExecutionId == executionId && s.MachineIP == MachineIP);
            return new RelExecutionStatusLastAndCount()
            {
                LastRelExecutionId = (from x in relexecution orderby x.ID descending select x.ID).FirstOrDefault(),
                TotalRelExecutionCount = relexecution.Count
            };
        }

        public RelExecutionStatu GetRelExecutionStatusById(Int64 RelExecutionStatusId)
        {
            var updatedStatus = _relExecutionStatus.GetAllToList(R => R.ID == RelExecutionStatusId);
            return (from x in updatedStatus where x.ID == RelExecutionStatusId select x).FirstOrDefault();
            //return _relExecutionStatus.GetFirstOrDefault(R => R.ID == RelExecutionStatusId);
        }

        public RelExecutionResponseP2 GetLatestRelStatusByIP(string IP, DateTime serverdatetime)
        {
            var statuslist = _relExecutionStatus.GetAllToList(RS => RS.MachineIP == IP);
            var status = (from x in statuslist orderby x.ID descending select x).FirstOrDefault();
            string statusDisplay = (!string.IsNullOrEmpty(status.CurrentStatus)) ? status.CurrentStatus.ToUpper() : "NULL";
            DateTime serverCurrentDateTime = new DateTime(serverdatetime.Year, serverdatetime.Month, serverdatetime.Day, serverdatetime.Hour, serverdatetime.Minute, serverdatetime.Second);
            DateTime lastupdatedCurrentStatus = new DateTime(status.CurrentStatusCheckedAt.Year, status.CurrentStatusCheckedAt.Month, status.CurrentStatusCheckedAt.Day, status.CurrentStatusCheckedAt.Hour, status.CurrentStatusCheckedAt.Minute, status.CurrentStatusCheckedAt.Second);
            double difference = (serverCurrentDateTime - lastupdatedCurrentStatus).TotalMinutes;
            bool isdiffIncrisedFromLimit = false;
            if (status.IsExecutionCompleted == true)
            {
                isdiffIncrisedFromLimit = false;
                statusDisplay = "COMPLETED";
            }
            else if (status.IsExecutionCompleted == false && difference > 16)
            {
                isdiffIncrisedFromLimit = true;
                statusDisplay = "TIMEOUT AFTER START";
            }
            else
            {
                isdiffIncrisedFromLimit = false;
                if (!string.IsNullOrEmpty(status.CurrentStatus))
                {
                    statusDisplay = status.CurrentStatus.ToUpper();
                }
                else
                {
                    statusDisplay = "NULL";
                }
            }

            if (isdiffIncrisedFromLimit)
            {
                var statusupdatetodb = _relExecutionStatus.GetSingleOrDefault(i => i.ID == status.ID);
                if (statusupdatetodb != null)
                {
                    statusupdatetodb.CurrentStatus = statusDisplay;
                    _relExecutionStatus.SaveChanges();
                }
            }

            return new RelExecutionResponseP2()
            {
                RelExecutionStatusId = status.ID,
                MachineIP = status.MachineIP,
                Status = statusDisplay
            };
        }

        public RelExecutionResponseP2 GetLatestRelStatusByID(Int64 ID, DateTime serverdatetime)
        {
            var status = _relExecutionStatus.GetFirstOrDefault(RS => RS.ID == ID);
            string statusDisplay = (!string.IsNullOrEmpty(status.CurrentStatus)) ? status.CurrentStatus.ToUpper() : "NULL";
            DateTime serverCurrentDateTime = new DateTime(serverdatetime.Year, serverdatetime.Month, serverdatetime.Day, serverdatetime.Hour, serverdatetime.Minute, serverdatetime.Second);
            DateTime lastupdatedCurrentStatus = new DateTime(status.CurrentStatusCheckedAt.Year, status.CurrentStatusCheckedAt.Month, status.CurrentStatusCheckedAt.Day, status.CurrentStatusCheckedAt.Hour, status.CurrentStatusCheckedAt.Minute, status.CurrentStatusCheckedAt.Second);
            double difference = (serverCurrentDateTime - lastupdatedCurrentStatus).TotalMinutes;
            bool isdiffIncrisedFromLimit = false;
            if (status.IsExecutionCompleted == true)
            {
                isdiffIncrisedFromLimit = false;
                statusDisplay = "COMPLETED";
            }
            else if (status.IsExecutionCompleted == false && difference > 16)
            {
                isdiffIncrisedFromLimit = true;
                statusDisplay = "TIMEOUT AFTER START";
            }
            else
            {
                isdiffIncrisedFromLimit = false;
                if (!string.IsNullOrEmpty(status.CurrentStatus))
                {
                    statusDisplay = status.CurrentStatus.ToUpper();
                }
                else
                {
                    statusDisplay = "NULL";
                }
            }

            if (isdiffIncrisedFromLimit)
            {
                var statusupdatetodb = _relExecutionStatus.GetSingleOrDefault(i => i.ID == status.ID);
                if (statusupdatetodb != null)
                {
                    statusupdatetodb.CurrentStatus = statusDisplay;
                    _relExecutionStatus.SaveChanges();
                }
            }

            return new RelExecutionResponseP2()
            {
                RelExecutionStatusId = status.ID,
                MachineIP = status.MachineIP,
                Status = statusDisplay
            };
        }

        public EmailRelMapping GetRelExecutionResultById(Int64 ID)
        {
            var status = _relExecutionStatus.GetFirstOrDefault(I => I.ID == ID);
            return new EmailRelMapping()
            {
                RelStatusId = status.ID,
                IsCompleted = status.IsExecutionCompleted,
                ExecutionId = status.ExecutionId,
                TestSuitId = status.TestSuitIds
            };
        }

        public List<TestSuit> GetTestSuitByExecutionIdAndAfterTestSuitId(int executionId, int testsuitid, string vm)
        {
            var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == executionId);
            if (execution != null)
            {
                var clientdata = _clientsRepository.GetFirstOrDefault(c => c.ID == execution.Client);
                var totalTestSuitList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

                var getAllClientList = _clientsInformationRepository.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == clientdata.RemoteMachineIP);
                List<int> CIID = (from x in getAllClientList select x.ID).ToList();

                //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                var TSTotalList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject
                    && TS.ParentTestCycle == execution.TargetTestCycle && TS.ParentClientInfo.HasValue && CIID.Contains(TS.ParentClientInfo.Value));

                var finaltestsuit = (from x in TSTotalList where x.ID > testsuitid select x).ToList();
                return finaltestsuit;
            }
            else
            {
                return new List<TestSuit>();
            }
        }

        public List<TestSuit> GetTestSuitByExecutionIdWithinStartAndEndLimit(int executionId, int testsuitstartid, int testsuitendid, string vm)
        {
            var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == executionId);
            if (execution != null)
            {
                var client = _clientsRepository.GetFirstOrDefault(c => c.ID == execution.Client);
                if (testsuitstartid != testsuitendid)
                {

                    var totalTestSuitList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject
                        && TS.ParentTestCycle == execution.TargetTestCycle && TS.ID > testsuitstartid && TS.ID <= testsuitendid);
                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

                    var getAllClientList = _clientsInformationRepository.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == vm);
                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();

                    //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                    var TSTotalList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject
                        && TS.ParentTestCycle == execution.TargetTestCycle && TS.ID > testsuitstartid && TS.ID <= testsuitendid
                        && TS.ParentClientInfo.HasValue && CIID.Contains(TS.ParentClientInfo.Value));

                    var finaltestsuit = (from x in TSTotalList where x.ID > testsuitstartid && x.ID <= testsuitendid select x).ToList();
                    return finaltestsuit;
                }
                else
                {
                    return new List<TestSuit>();
                }
            }
            else
            {
                return new List<TestSuit>();
            }
        }


        //--------------In the Replacement of Load Balanced Execution Service-------------------------

        public Int64 CreateRelExecutionStatus(int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        {
            try
            {
                //Get the execution details                
                Int64 lastrowcount = 0;
                Int64 lastrowid = 0;
                //var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == ExecutionId);
                var execution = _testExecutionRepository.GetSingleOrDefault(p => p.ID == ExecutionId);

                if (execution != null)
                {
                    //var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);
                    var client = _clientsRepository.GetSingleOrDefault(C => C.ID == execution.Client);
                    //var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                    var totalTestSuitList = _testSuitsRepository.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

                    //var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);
                    var getAllClientList = _clientsInformationRepository.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);
                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();

                    //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                    //var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
                    //    TS => TS.ParentProject == execution.BelongToProject
                    //        && TS.ParentTestCycle == execution.TargetTestCycle
                    //        && TS.ParentClientInfo.HasValue
                    //        && CIID.Contains(TS.ParentClientInfo.Value));

                    var TSTotalList = _testSuitsRepository.GetAllToList(
                        TS => TS.ParentProject == execution.BelongToProject
                            && TS.ParentTestCycle == execution.TargetTestCycle
                            && TS.ParentClientInfo.HasValue
                            && CIID.Contains(TS.ParentClientInfo.Value));

                    lastrowcount = (from x in TSTotalList select x.ID).Count();
                    lastrowid = (from x in TSTotalList select x.ID).Max();
                }

                RelExecutionStatu relExecutionstatus = new RelExecutionStatu();
                relExecutionstatus.ExecutionId = ExecutionId;
                relExecutionstatus.MachineIP = IP;
                relExecutionstatus.ExecutionStarted = DateTime.Now;
                relExecutionstatus.IsExecutionCompleted = IsExecutionCompleted;
                if (!string.IsNullOrEmpty(currentstatus))
                    relExecutionstatus.CurrentStatus = currentstatus;
                if (!string.IsNullOrEmpty(StatusComment))
                    relExecutionstatus.Comment = StatusComment;
                relExecutionstatus.LastStatusCheckedAt = (DateTime)System.Data.SqlTypes.SqlDateTime.Null;
                relExecutionstatus.CurrentStatusCheckedAt = DateTime.Now;
                relExecutionstatus.LastRowCount = lastrowcount;
                relExecutionstatus.LastRowId = lastrowid;
                //var relexecutionlocator = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
                _relExecutionStatus.Add(relExecutionstatus);
                _relExecutionStatus.SaveChanges();
                return relExecutionstatus.ID;
            }
            catch
            {
                return 0;
            }
        }

        public void UpdateLatestRelExecutionStatusEntryLevelRecord(Int64 RelStatusID, Int64 LastRowCount, Int64 LastRowId, string ToAddress)
        {
            try
            {
                //var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();                
                var data = _relExecutionStatus.GetSingleOrDefault(p => p.ID == RelStatusID);
                if (data != null)
                {
                    //data.LastRowCount = LastRowCount;
                    //data.LastRowId = LastRowId;
                    data.ToAddress = ToAddress;
                    _relExecutionStatus.SaveChanges();
                }
            }
            catch
            {

            }
        }

        public string GetTestSuitIdString(int executionId, string testsuitids)
        {
            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            return HRRP.GetTestSuitIds(executionId, testsuitids);
        }

        //-----------------------------------------------------------------------------------------
        //Count for email from jenkins report
        public JenkinsReportCountStatus GetJenkinReportFinalCountByTestSuitIds(List<int> testsuitids)
        {
            HelperRelReportProvider HRRP = new HelperRelReportProvider();
            return HRRP.GetJenkinReportFinalCountByTestSuitIds(testsuitids);
        }

    }
}
