using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Castle.Core.Internal;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.GRS.DB;
using TechTalk.JiraRestClient; 

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageReportingProvider : IManageReportingProvider
    {
        private readonly ISessionHelper _sessionHelper;

        private readonly IRepository<Project> _projectRepository;

        private readonly IRepository<vw_AnalyzedSubSteps> _vw_AnalyzedSubSteps;

        private readonly IRepository<vw_Defects> _vw_Defects;

        private readonly IRepository<Analyser> _analyser;

        private readonly IRepository<User> _user;

        public ManageReportingProvider(ISessionHelper sessionHelper, IRepository<Project> projectRepository, IRepository<vw_AnalyzedSubSteps> vw_AnalyzedSubSteps,
            IRepository<vw_Defects> vw_Defects, IRepository<Analyser> analyser, IRepository<User> user)
        {
            _sessionHelper = sessionHelper;
            _projectRepository = projectRepository;
            _vw_AnalyzedSubSteps = vw_AnalyzedSubSteps;
            _vw_Defects = vw_Defects;
            _analyser = analyser;
            _user = user;
        }

        public List<CyclesModel> GetCumulativeStatistics(IPrincipal user)
        {
            var testCycles = new List<CyclesModel>();
            _sessionHelper.GetSessionDetails(user)
                .Project.TestCycles.OrderByDescending(p => p.CycleStart).ToList()
                .ForEach(p => testCycles.Add(new CyclesModel
                {
                    Id = p.ID,
                    Name = p.CycleName,
                    IsDeactivated = p.isInnactive == null ? false : (bool)p.isInnactive
                }));
            return testCycles;
        }

        public UsersAndCyclesModel GetAnalysisStatistics(IPrincipal user)
        {
            var tcs = _sessionHelper.GetSessionDetails(user)
                .Project.TestCycles.Where(p => p.isInnactive == null ? true : !(bool)p.isInnactive).OrderByDescending(p => p.CycleStart).ToList();

            var users = _sessionHelper.GetSessionDetails(user).Project.Accesses.Select(p => p.User).OrderBy(p => p.USerFullName).Distinct().ToList();
            var usersAndCycles = new UsersAndCyclesModel
            {
                Cycles = tcs,
                Analysers = users
            };
            return usersAndCycles;
        }

        public List<TestCase> getAnalysisDataByCycle(IPrincipal user, int cycleId)
        {
            var testCases = new List<TestCase>();
            var testCycle = _sessionHelper.GetSessionDetails(user).Project.TestCycles.SingleOrDefault(p => p.ID == cycleId);
            if (testCycle != null && testCycle.TestSuits.Count > 0)
            {
                testCycle.TestSuits.ForEach(p => testCases.AddRange(p.TestCases.ToList()));
            }
            return testCases;
        }

        public UsersAndTestCasesModel getAnalysisDataByUser(int userId, DateTime startDate, DateTime? endDate)
        {
            var testCasesAndCycle = new List<TestCasesAndCycle>();
            var reqUser = _user.GetFirstOrDefault_Services(p => p.ID == userId);
            if (endDate.HasValue)
            {
                var tcs = reqUser.TestCases.Where(p => p.AnalyzedDate.HasValue).Where(p => DateTime.Compare(p.AnalyzedDate.Value.Date, startDate.Date) >= 0)
                    .Where(p => DateTime.Compare(p.AnalyzedDate.Value.Date, (DateTime)endDate.Value.Date) <= 0).ToList().GroupBy(p => p.TestSuit.TestCycle.CycleName);
                tcs.ForEach(p => testCasesAndCycle.Add(new TestCasesAndCycle()
                {
                    ParentTestCycleId = (int)p.First().TestSuit.ParentTestCycle,
                    ParentTestCycleName = p.Key,
                    TestCases = p.Select(tc => tc).ToList()
                }));
            }
            else
            {
                var tcs = reqUser.TestCases.Where(p => p.AnalyzedDate.HasValue).Where(p => DateTime.Compare(p.AnalyzedDate.Value.Date, startDate.Date) == 0)
                                   .ToList().GroupBy(p => p.TestSuit.TestCycle.CycleName);
                tcs.ForEach(p => testCasesAndCycle.Add(new TestCasesAndCycle()
                {
                    ParentTestCycleId = (int)p.First().TestSuit.ParentTestCycle,
                    ParentTestCycleName = p.Key,
                    TestCases = p.Select(tc => tc).ToList()
                }));
            }
            var user = new UsersAndTestCasesModel()
            {
                Id = reqUser.ID,
                FullName = reqUser.USerFullName,
                TestCasesAndCycle = testCasesAndCycle
            };
            return user;
        }

        public List<ReportCommulativeModel> AddReport(IPrincipal user, int cycleId)
        {
            var reports = new List<ReportCommulativeModel>();

            var project = _sessionHelper.GetSessionDetails(user).Project;

            var testCycle = project.TestCycles.SingleOrDefault(p => p.ID == cycleId);
            if (testCycle != null)
            {
                var testSuits = testCycle.TestSuits.ToList();
                var customStatuses = new CustomStatuses.CustomStatuses(testCycle.Project.ID, _projectRepository);
                var allBrowsers = testSuits.GroupBy(p => p.ClientsInformation.ClientBrowser).ToList();

                foreach (var item in allBrowsers)
                {
                    var testSuites = item.GroupBy(p => p.TSName).Distinct().ToList();
                    var testSuiteIDs = item.Select(p => p.ID).ToList();
                    var testSuitesFromView =
                        _vw_AnalyzedSubSteps.GetAll(p => testSuiteIDs.Contains(p.ID) && p.ClientBrowser == item.Key).ToList();


                    foreach (var testSuite in testSuites)
                    {
                        var report = new ReportCommulativeModel();
                        try
                        {
                            report.TestCycleName = testCycle.CycleName;
                            var statusesWithCount = new List<Counts>();
                            var defects = new List<string>();
                            var testSuitelist = testSuite.ToList();
                            List<TestCase> testCases = new List<TestCase>();
                            testSuitelist.ForEach(p =>
                                testCases.AddRange(p.TestCases));
                            var browser = new Browser();
                            browser.Name = item.Key;
                            browser.Fail = testCases.Count(d => d.TCState == "fail");
                            browser.Pass = testCases.Count(d => d.TCState == "pass");
                            browser.NotCompleted = testCases.Count(d => d.TCState == "notcompleted");
                            browser.CustomStatuses = new List<CustomStatus>();
                            browser.AvailableStatuses = customStatuses.Statuses.Select(p => p.StatusName).ToList();
                            browser.AvailableStatusesIds = customStatuses.Statuses.Select(p => p.StatusID).ToList();

                            List<string> defectsTemp;
                            int analyzed;

                            var testCasesFromView = testSuitesFromView.Where(p => p.TSName == testSuite.Key).ToList();

                            statusesWithCount.AddRange(customStatuses.GetStatusForTestSuite(testCasesFromView,
                                true, customStatuses, out defectsTemp, out analyzed));
                            defects.AddRange(defectsTemp);

                            browser.NotAnalyzed = browser.Fail + browser.NotCompleted - analyzed;
                            browser.Analyzed = analyzed;

                            statusesWithCount.ForEach(p => browser.CustomStatuses.Add(
                                new CustomStatus
                                {
                                    Name = p.Status.StatusName,
                                    Status = p.Count
                                }));

                            report.Browser = browser;
                            report.Bugs = defects;
                            report.Bugtracker = project.Bugracker.Url;
                            report.TestSiuteName = testSuite.Key;
                            report.TestSiuteId = testSuite.First().ID;
                            report.Tester = String.Join(" ;", _analyser.GetAllToList(p => p.TestSet == report.TestSiuteId).Select(p => p.User1.USerFullName).ToList());

                            var tc = testSuite.OrderBy(p => p.TSStart).Last().TestCases;
                            if (tc.Count != 0)
                            {
                                var tcl = testSuite.OrderBy(p => p.TSStart).Last(p => p.TestCases.Count != 0);
                                var tcf = testSuite.OrderBy(p => p.TSStart).First(p => p.TestCases.Count != 0);
                                if (tcl != null && tcf != null)
                                    report.Time = tcl.TestCases.Last().TCEndTime -
                                                  tcf.TestCases.First().TCStartTime;
                            }
                            else
                            {
                                report.Time = new TimeSpan(0, 0, 0);
                            }
                            report.Comments = string.Join("\r\n", testSuite.Select(p => p.Comments));

                            reports.Add(report);

                        }
                        catch (Exception ex)
                        {
                            report.Comments = ex.Message;
                        }
                    }
                }
            }

            return reports;
        }

        public List<TestSuit> GetTestSuits(IPrincipal user, int cycleId)
        {
            var testCycle = _sessionHelper.GetSessionDetails(user)
                .Project.TestCycles.SingleOrDefault(p => p.ID == cycleId);
            if (testCycle != null)
            {
                return testCycle.TestSuits.ToList();
            }
            return new List<TestSuit>();
        }

        //public List<BugListModel> GetAllBugList(int cycleId)
        //{
        //    var allDefects = _vw_Defects.GetAll(p => p.TCycleID == cycleId).Select(p => p.Defects).ToList();
        //    var bugtracker = _vw_Defects.GetFirstOrDefault(p => p.TCycleID == cycleId).Bugtracker;
        //    var defectsFromDb = allDefects.Distinct();
        //    var bugList = new List<BugListModel>();
        //    defectsFromDb.ForEach(p => bugList.Add(new BugListModel()
        //    {
        //        BugCount = allDefects.Count(b => b == p),
        //        BugID = p,
        //        BugUrl = bugtracker + "browse/" + p
        //    }));
        //    return bugList;
        //}
        public List<BugListModel> GetAllBugList(int cycleId)
        {
            var allDefects = _vw_Defects.GetAllToList(p => p.TCycleID == cycleId);
            var bugtracker = _vw_Defects.GetFirstOrDefault(p => p.TCycleID == cycleId).Bugtracker;
            var defectsFromDb = new List<string>();
            allDefects.ForEach(p => defectsFromDb.Add(p.Defects.Trim()));
            defectsFromDb = defectsFromDb.Distinct().ToList();
            var bugList = new List<BugListModel>();

            foreach (var defect in defectsFromDb)
            {
                var tcDetails = new List<TestCaseDetails>();
                allDefects.Where(p => p.Defects.Trim() == defect).ForEach(p =>
                    tcDetails.Add(
                    new TestCaseDetails() { TCName = p.TCName, TCDescription = p.TCDescription, TestSuiteName = p.TSName }
                    )
                    );
                bugList.Add(new BugListModel()
                {
                    BugCount = allDefects.Count(b => b.Defects.Trim() == defect),
                    BugID = defect,
                    BugUrl = bugtracker.TrimEnd('/') + "/browse/" + defect,
                    TCycleId = cycleId
                });
            }
            bugList = bugList.OrderBy(p => p.BugID).ToList();
            return bugList;
        }

        public List<TestCaseDetails> GetFailedTestCases(string bugId, int cycleId)
        {
            var allDefects = _vw_Defects.GetAllToList(p => p.Defects.Trim() == bugId && p.TCycleID == cycleId);
            var tcDetails = new List<TestCaseDetails>();
            allDefects.ForEach(p =>
                tcDetails.Add(
                new TestCaseDetails() { TCName = p.TCName, qc_id = p.qc_id, TCDescription = p.TCDescription, TestSuiteName = p.TSName, bugId = bugId }
                )
                );
            return tcDetails;
        }

        public JiraIssueModel GetJiraIssue(string key)
        {
            var client = new JiraClient("http://jira.bjz.apac.ime.reuters.com/", "dmitry.bakaev", "test");
            var result = client.LoadIssue(key);
            return new JiraIssueModel
                {
                    Summary = result.fields.summary,
                    Status = result.fields.status.name
                };

        }
    }
}
