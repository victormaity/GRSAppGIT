using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.DataLINQ;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageHomeProvider : IManageHomeProvider
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<HostsConfiguration> _hostsConfiguration;
        private readonly IRepository<TestCycle> _testCycle;
        private readonly IRepository<TestCase> _testCase;
        private readonly IRepository<ClientsInformation> _clientsInformation;

        public ManageHomeProvider(ISessionHelper sessionHelper, IRepository<Project> projectRepository, IRepository<HostsConfiguration> hostsConfiguration,
            IRepository<TestCycle> testCycle, IRepository<TestCase> testCase, IRepository<ClientsInformation> clientsInformation)
        {
            _sessionHelper = sessionHelper;
            _projectRepository = projectRepository;
            _hostsConfiguration = hostsConfiguration;
            _testCycle = testCycle;
            _testCase = testCase;
            _clientsInformation = clientsInformation;
        }

        public HomePageStats GetHomePageStats(IPrincipal user, int deliveryItems)
        {
            var ret = new HomePageStats();
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var testSuitList = project.TestSuits.OrderByDescending(p => p.ID).Where(p => p.TestCycle != null).Take(deliveryItems).ToList();
            ret.TestSuites = testSuitList;
            
            // 1277 Error..
            //var x = project.TestSuits.OrderByDescending(p => p.ID).ToList();          
            // ret.TestSuites = (from xx in x where xx.TestCycle != null select xx).Take(deliveryItems).ToList();
            //defects
            // var alldefects = project.Analyses.Select(p => p.Defect).Distinct().Count();
            //ret.TotalDefects = alldefects;
            //var alltests = project.TestSuits.Select(p => p.TestCases).ToList();
            return ret;
        }


        public ExecutionReportPageDataModel GetExecutionReport(IPrincipal user, string duration = "30DAYS", string release = "ALL", string team = "ALL", string startdate = "", string enddate = "", int cycleid = 0)
        {
            var projectid = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperHomeProvider HHP = new HelperHomeProvider();
            return HHP.GetExecutionReport(projectid, duration, release, team, startdate, enddate, cycleid);
        }

        public CountAndVMInfo GetSuitTestCaseExecutionCountAndClientInfo(string suitIds)
        {
            HelperHomeProvider HHP = new HelperHomeProvider();
            return HHP.GetSuitTestCaseExecutionCountAndClientInfo(suitIds);
        }

        public TSTCInforBase GetTestSuitWithTestCasesDetails(string suitIds)
        {
            HelperHomeProvider HHP = new HelperHomeProvider();
            return HHP.GetTestSuitWithTestCasesDetails(suitIds);
        }
    }
}
