using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using Microsoft.Practices.ServiceLocation;

namespace GRSReporting
{
    /// <summary>
    /// Summary description for AnalysisService
    /// </summary>
   /* [WebService(Namespace = "http://tempuri.org/")]*/
    [WebService(Namespace = "http://microsoft.com/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ReportingService : System.Web.Services.WebService
    {
        //private readonly ISessionHelper _sessionHelper;

        //private readonly IRepository<Project> _projectRepository;

        //public ManageReportingProvider(ISessionHelper sessionHelper, IRepository<Project> projectRepository)
        //{
        //    _sessionHelper = sessionHelper;
        //    _projectRepository = projectRepository;
        //}

        public List<CyclesModel> GetCumulativeStatistics(int projectId)
        {
            var testCycles = new List<CyclesModel>();
            ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetAll(p => p.ParentProject == projectId).ToList()
                .ForEach(p => testCycles.Add(new CyclesModel { Id = p.ID, Name = p.CycleName }));
            return testCycles;
        }


       public List<ReportCommulativeModel> AddReport(int cycleId)
        {
            var reports = new List<ReportCommulativeModel>();

           var testCycle =
               ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetSingleOrDefault(p => p.ID == cycleId);
            if (testCycle != null)
            {
                var testSuits = testCycle.TestSuits.ToList();
                var customStatuses = 
                    new CustomStatuses.CustomStatuses(testCycle.Project.ID, _projectRepository);
                var allBrowsers = testSuits.GroupBy(p => p.ClientsInformation.ClientBrowser).ToList();

                foreach (var item in allBrowsers)
                {
                    var testSuites = item.GroupBy(p => p.TSName).Distinct().ToList();

                    foreach (var testSuite in testSuites)
                    {
                        var statusesWithCount = new List<Counts>();
                        var defects = new List<string>();

                        var browser = new Browser();
                        browser.Name = item.Key;
                        browser.Analyzed = testSuite.Select(p => p.TestCases.Count(d => d.isAnalyzed)).Sum();
                        browser.Fail = testSuite.Select(p => p.TestCases.Count(d => d.TCState == "fail")).Sum();
                        browser.Pass = testSuite.Select(p => p.TestCases.Count(d => d.TCState == "pass")).Sum();
                        browser.NotCompleted = testSuite.Select(p => p.TestCases.Count(d => d.TCState == "notcompleted")).Sum();
                        browser.NotAnalyzed = testSuite.Select(p => p.TestCases.Count(d => !d.isAnalyzed)).Sum();
                        browser.CustomStatuses = new List<CustomStatus>();
                        browser.AvailableStatuses = customStatuses.Statuses.Select(p => p.StatusName).ToList();

                        var report = new ReportCommulativeModel();
                        List<string> defectsTemp;
                        testSuite.ToList().ForEach(p =>
                        {
                            statusesWithCount.AddRange(customStatuses.GetStatusForTestSuite(p,
                                true, customStatuses, out defectsTemp));
                            defects.AddRange(defectsTemp);
                        });

                        statusesWithCount.ForEach(p => browser.CustomStatuses.Add(
                            new CustomStatus
                            {
                                Name = p.Status.StatusName,
                                Status = p.Count
                            }));

                        report.TestCycleName = testCycle.CycleName;
                        report.Browser = browser;
                        report.Bugs = defects;
                        report.TestSiuteName = testSuite.Key;

                        var tc = testSuite.OrderBy(p => p.TSStart).Last().TestCases;
                        if (tc.Count != 0)
                        {
                            report.Time = testSuite.OrderBy(p => p.TSStart).Last().TestCases.Last().TCEndTime -
                                          testSuite.OrderBy(p => p.TSStart).First().TestCases.First().TCStartTime;
                        }
                        else
                        {
                            report.Time = new TimeSpan(0,0,0);
                        }
                        report.Comments = string.Join("\r\n", testSuite.Select(p => p.Comments));

                        reports.Add(report);
                    }
                }
            }
            return reports;
        }

    }
}
