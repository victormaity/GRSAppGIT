using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ClosedXML.Excel;
using GlobalReportingSystem.BL.Properties;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.Executor;
using GlobalReportingSystem.Core.Models.GRS;
using Microsoft.Practices.ServiceLocation;
using Messages = GlobalReportingSystem.Core.Constants.Messages;
using QCClient.Client;
using QcClient.Entities.QcEntities;
using QcClient.Entities.XmlEntities;
using QcClient.Tools;
using GlobalReportingSystem.Core.Constants;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageExportToExcelProvider : IManageExportToExcel
    {
        private readonly RestClient _qcRestClient = new RestClient();
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TestCycle> _testCycleRepository;
        private readonly IRepository<TestSuit> _testSuitRepository;
        private readonly IRepository<TestCase> _testCaseRepository;
        private readonly IRepository<TestStep> _testStepRepository;
        private readonly IRepository<SubStep> _subStepRepository;
        private readonly IRepository<Analysis> _analysisRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<vw_FilesStorage> _vw_FilesStorageRepository;
        private readonly IRepository<HostsConfiguration> _hostsConfiguration;
        private readonly IRepository<AccountForTestRun> _accountForTestRun;
        private readonly IRepository<ExecutionConfiguration> _executionConfiguration;
        private readonly IRepository<TestsExecution> _testsExecutions;
        private readonly IRepository<User> _user;
        private readonly IRepository<AutoAnalysisCache> _autoAnalysisRepository;
        private readonly IRepository<QCExportAssignment> _qcExportAssignmentRepository;
        private readonly ISessionHelper _sessionHelper;
        private string userName;
        private string password;

        public ManageExportToExcelProvider(IRepository<Project> projectRepository, IRepository<QCExportAssignment> qcExportAssignmentRepository,
            IRepository<TestSuit> testSuitRepository, IRepository<TestCase> testCaseRepository, IRepository<TestStep> testStepRepository,
            ISessionHelper sessionHelper, IRepository<TestCycle> testCycleRepository, IRepository<SubStep> subStepRepository,
            IRepository<Analysis> analysisRepository, IRepository<Client> clientRepository, IRepository<vw_FilesStorage> vw_FilesStorageRepository,
            IRepository<HostsConfiguration> hostsConfiguration, IRepository<AccountForTestRun> accountForTestRun,
            IRepository<ExecutionConfiguration> executionConfiguration, IRepository<TestsExecution> testsExecutions,
            IRepository<User> user, IRepository<AutoAnalysisCache> autoAnalysisRepository)
        {
            _projectRepository = projectRepository;
            _qcExportAssignmentRepository = qcExportAssignmentRepository;
            _testSuitRepository = testSuitRepository;
            _testCaseRepository = testCaseRepository;
            _testStepRepository = testStepRepository;
            _testCycleRepository = testCycleRepository;
            _sessionHelper = sessionHelper;
            _subStepRepository = subStepRepository;
            _analysisRepository = analysisRepository;
            _clientRepository = clientRepository;
            _vw_FilesStorageRepository = vw_FilesStorageRepository;
            _hostsConfiguration = hostsConfiguration;
            _accountForTestRun = accountForTestRun;
            _executionConfiguration = executionConfiguration;
            _testsExecutions = testsExecutions;
            _user = user;
            _autoAnalysisRepository = autoAnalysisRepository;
        }

        public string ExportTestSetsToExcel(IPrincipal user, int[] testSuitesId)
        {
            int testCycleId = 0;
            int projectId = 0;
            var listForExport = new List<ExportToExcelModel>();
            var testSuites = _testSuitRepository.GetAll(p => testSuitesId.Contains(p.ID) && p.TestCases.Count != 0).ToList();
            foreach (var testSuite in testSuites)
            {
                //var testSuite = _testSuitRepository.GetSingleOrDefault_Services(p => p.ID == testSuiteId);

                if (testCycleId == 0)
                {
                    testCycleId = testSuite.ParentTestCycle ?? 0;
                    projectId = testSuite.ParentProject ?? 0;
                }

                if (testSuite != null)
                {
                    //var path = _qcExportAssignmentRepository.GetSingleOrDefault_Services(p => p.BelongToProject == projectId && p.TestSetName == testSuite.TSName);
                    var name = testSuite.TSName;
                    var clientIp = testSuite.ClientsInformation.ClientIP;
                    var endTime = testSuite.ClientsInformation.EndTime.ToString();

                    //var ParentStepId = _testStepRepository.GetSingleOrDefault(p => p.ID == stepId);
                    //string TS_Name = ParentStepId.TestCase.TCName;
                    //var StepNumber = ParentStepId.TestCase.TestSteps.OrderBy(p => p.ID).ToList().IndexOf(ParentStepId) + 1;

                    foreach (var test in testSuite.TestCases)
                    {
                        var status = GetTestStatuses(projectId, test.TCName);
                        var analysis = _analysisRepository.GetLastOrDefault(p => p.GlobalPositionTCName == test.TCName);
                        listForExport.Add(new ExportToExcelModel()
                        {
                            BusinessCriticality = test.Criticality,
                            Description = test.TCDescription,
                            //Steps = string.Join("\n", test.TestSteps.Select(p => p.StepDescription)),
                            TestName = test.TCName,
                            Subject = name,
                            CurrentStatus = test.TCState,
                            ClientIP = clientIp,
                            EndTime = endTime,
                            CurrentDefect = analysis == null ? "" : analysis.CurrentDefects,
                            GlobalStatus = status == null ? "" : status.StatusName,
                            OldDefect = analysis == null ? "" : analysis.OldDefects
                            //Attachments = string.IsNullOrEmpty(test.TCAttachments) ? "" :
                            //string.Join("\r\n",
                            //test.TCAttachments.Split(';')
                            //.Select(p => ("http://" + HttpContext.Current.Request.Url.Authority + "/" + "Attachments/" + projectId + "/" + p))),
                            //Screenshots = string.Join("\r\n", GetScreenShots(test).
                            //Select(p => ("http://" + HttpContext.Current.Request.Url.Authority + "/" + "Screenshots/" + p)))
                        });
                    }
                }
            }

            //export to excel
            var workbook = new XLWorkbook();

            var maxCount = listForExport.Count();
            var count = 0;
            const int portion = 50000;
            if (maxCount < portion)
            {
                workbook.Worksheets.Add(ToDataTable(listForExport), "Export");
            }
            else
            {
                while (count - maxCount < portion)
                {
                    workbook.Worksheets.Add(ToDataTable(listForExport.Skip(count).Take(portion).ToList()), "Export");
                    count += portion;
                }
            }

            //workbook.Worksheets.Add(ToDataTable(listForExport), "Export");
            var fileName = HttpContext.Current.Server.MapPath("~/Temp") + "\\" + Guid.NewGuid() + ".xlsx";
            workbook.SaveAs(fileName);
            return fileName;
        }

        private Core.Models.GRS.DB.Status GetTestStatuses(int projId, string tcName)
        {
            var testDetails = _analysisRepository.GetLastOrDefault(p => p.GlobalPositionTCName == tcName);
            var customStatuses = new CustomStatuses.CustomStatuses(projId, _projectRepository);

            if (testDetails != null)
                try
                {
                    return customStatuses.getStatus((Guid.Parse(testDetails.StepState)));
                }
                catch (Exception)
                {
                    return null;
                }
            return null;
        }

        private List<string> GetScreenShots(TestCase testCase)
        {
            var list = new List<string>();

            testCase.TestSteps.ForEach(p =>
                                p.SubSteps.Where(g => !string.IsNullOrEmpty(g.SubStepScreenShotDriver)).ForEach(g => list.Add(g.SubStepScreenShotDriver)));
            return list;
        }

        private DataTable ToDataTable<T>(IList<T> data)
        {
            var props =
                TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            var values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}