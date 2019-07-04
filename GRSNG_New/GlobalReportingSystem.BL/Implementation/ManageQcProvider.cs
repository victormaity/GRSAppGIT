using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalReportingSystem.BL.Properties;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
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
    public class ManageQcProvider : IManageQcProvider
    {
        private readonly RestClient _qcRestClient = new RestClient();
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TestSuit> _testSuitRepository;
        private readonly IRepository<TestCase> _testCaseRepository;
        private readonly IRepository<TestStep> _testStepRepository;
        private readonly IRepository<QCExportAssignment> _qcExportAssignmentRepository;
        private readonly ISessionHelper _sessionHelper;
        private string userName;
        private string password;

        public ManageQcProvider(IRepository<Project> projectRepository, IRepository<QCExportAssignment> qcExportAssignmentRepository,
            IRepository<TestSuit> testSuitRepository, IRepository<TestCase> testCaseRepository, IRepository<TestStep> testStepRepository, ISessionHelper sessionHelper)
        {
            _projectRepository = projectRepository;
            _qcExportAssignmentRepository = qcExportAssignmentRepository;
            _testSuitRepository = testSuitRepository;
            _testCaseRepository = testCaseRepository;
            _testStepRepository = testStepRepository;
            _sessionHelper = sessionHelper;
        }
        ~ManageQcProvider()
        {
            _qcRestClient.Dispose();
        }

        private void QcAuth(IPrincipal user, string userName, string password)
        {
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var project = _projectRepository.GetFirstOrDefault_Services(p => p.ID == pId);
            var qcDomain = String.Empty;
            var qcProject = String.Empty;
            if (!String.IsNullOrEmpty(project.QCServer))
            {
                _qcRestClient.ServerUrl = project.QCServer;
            }
            else
            {
                throw new Exception("Project does not have QC server path. Please check project configuration.");
            }
            if (!String.IsNullOrEmpty(project.QCLocation))
            {
                if (project.QCLocation.Split('/').Length == 2)
                {
                    qcDomain = project.QCLocation.Split('/')[0];
                    qcProject = project.QCLocation.Split('/')[1];
                }
                else
                {
                    throw new Exception(String.Format("Current QC location path is not valid - '{0}'. Valid QC location format is - 'Domain/Project'. Please check project configuration.", project.QCLocation));
                }
            }
            else
            {
                throw new Exception("Project does not have QC location path (QC location format is - 'Domain/Project'). Please check project configuration.");
            }

            var access = _qcRestClient.Auth(userName, password);
            if (String.IsNullOrEmpty(access))
            {
                ConnectToDomain(qcDomain);
                ConnectToProject(qcProject);
            }
        }

        private void ConnectToDomain(string qcDomain)
        {
            var domain = _qcRestClient.GetDomains().Domain.SingleOrDefault(p => p.Name == qcDomain);
            if (domain == null)
            {
                throw new Exception(String.Format("Domain '{0}' was not found. Please check project configuration.", qcDomain));
            }
            else
            {
                _qcRestClient.Domain = domain.Name;
            }
        }

        private void ConnectToProject(string qcProject)
        {
            var project = _qcRestClient.GetProjects().Project.SingleOrDefault(p => p.Name == qcProject);
            if (project == null)
            {
                throw new Exception(String.Format("Project '{0}' was not found. Please check project configuration.", qcProject));
            }
            else
            {
                _qcRestClient.Project = project.Name;
            }
        }

        private TestPlanFolderEntity createTestPlanFolder(string qcTestPlanPath)
        {
            int destinationFolderId = 0;
            var lastIndex = qcTestPlanPath.LastIndexOf('/') != -1 ? qcTestPlanPath.LastIndexOf('/') : qcTestPlanPath.LastIndexOf('\\');
            if (lastIndex != -1)
            {
                var parentFolderId = getTestPlanFolderIdByPath(qcTestPlanPath.Substring(0, lastIndex).ToString());
                if (parentFolderId != -1)
                {
                    destinationFolderId = getTestPlanFolderIdByPath(qcTestPlanPath);
                    if (destinationFolderId != -1)
                    {
                        _qcRestClient.TestPlan.MoveFolderInTrash(destinationFolderId);
                    }
                    destinationFolderId = parentFolderId;
                }
                else
                {
                    throw new Exception(String.Format("Can't find folder {0}", qcTestPlanPath.Substring(0, lastIndex).ToString()));
                }
            }
            else
            {
                destinationFolderId = getTestPlanFolderIdByPath(qcTestPlanPath);
                if (destinationFolderId != -1)
                {
                    _qcRestClient.TestPlan.MoveFolderInTrash(destinationFolderId);
                }
                destinationFolderId = _qcRestClient.TestPlan.GetRootId();
            }
            return _qcRestClient.TestPlan.CreateFolderByName(qcTestPlanPath.Substring(lastIndex + 1).ToString(), destinationFolderId.ToString(), String.Empty);
        }

        private TestLabFolderEntity createTestLabFolder(string qcTestLabPath)
        {
            int destinationLabFolderId = 0;
            var lastIndexLab = qcTestLabPath.LastIndexOf('/') != -1 ? qcTestLabPath.LastIndexOf('/') : qcTestLabPath.LastIndexOf('\\');
            if (lastIndexLab != -1)
            {
                var parentFolderId = getTestLabFolderIdByPath(qcTestLabPath.Substring(0, lastIndexLab).ToString());
                if (parentFolderId != -1)
                {
                    destinationLabFolderId = getTestLabFolderIdByPath(qcTestLabPath);////////////////////
                    if (destinationLabFolderId != -1)
                    {
                        _qcRestClient.TestLab.MoveFolderInTrash(destinationLabFolderId);
                    }
                    destinationLabFolderId = parentFolderId;
                }
                else
                {
                    throw new Exception(String.Format("Can't find folder {0}", qcTestLabPath.Substring(0, lastIndexLab).ToString()));
                }
            }
            else
            {
                destinationLabFolderId = getTestLabFolderIdByPath(qcTestLabPath);
                if (destinationLabFolderId != -1)
                {
                    _qcRestClient.TestLab.MoveFolderInTrash(destinationLabFolderId);
                }
                destinationLabFolderId = _qcRestClient.TestLab.GetRootId();
            }
            return _qcRestClient.TestLab.CreateFolderByName(qcTestLabPath.Substring(lastIndexLab + 1).ToString(), destinationLabFolderId.ToString(), String.Empty);
        }

        public void ExportTestResult(IPrincipal user, List<int> testSuitesIds)
        {
            try
            {
                string userName = _sessionHelper.GetSessionDetails(user).User.UserQCLogin;
                string password = _sessionHelper.GetSessionDetails(user).User.UserQCPassword;
                var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
                QcAuth(user, userName, password);

                var project = _projectRepository.GetFirstOrDefault_Services(p => p.ID == pId);
                var qcTestPlanPath = project.QCTestPlan;
                var qcTestLabPath = project.QcResultsPath;

                var destinationFolder = createTestPlanFolder(qcTestPlanPath);
                var destinationLabFolder = createTestLabFolder(qcTestLabPath);

                foreach (var testSuiteId in testSuitesIds)
                {
                    var testSuite = _testSuitRepository.GetFirstOrDefault_Services(p => p.ID == testSuiteId);
                    var testCases = testSuite.TestCases.ToList();
                    var tsFolder = _qcRestClient.TestPlan.CreateFolderByName(testSuite.TSName, destinationFolder.Id, String.Empty);
                    var labFolder = _qcRestClient.TestLab.CreateFolderByName(testSuite.TSName, destinationLabFolder.Id, String.Empty);
                    var labTestSet = _qcRestClient.TestLab.CreateTestSet(testSuite.TSName, labFolder.Id);
                    var priorityFieldName = _qcRestClient.GetNameOfPriorityField();
                    foreach (var testCase in testCases)
                    {
                        //add test plan test case
                        var tC = _qcRestClient.TestPlan.AddTestCasesToTestPlanFolder(tsFolder.Id, new TestEntity()
                            {
                                Name = testCase.TCName,
                                Description = testCase.TCDescription,
                                User06 = getCriticality(testCase.Criticality),
                                Status = "Ready",
                                Owner = userName
                            }, priorityFieldName);
                        //add test lab test case
                        var labTC = _qcRestClient.TestLab.AddTestToTestSet(new TestEntity()
                        {
                            Id = tC.Id,
                            Owner = tC.Owner
                        }
                       , labTestSet);
                        string tsName = "Step";
                        int i = 1;
                        var trun = _qcRestClient.TestRun.AddTestRun(labTC, tC, labTestSet);
                        var stepStatuses = new List<string>();
                        foreach (var testStep in testCase.TestSteps)
                        {
                            //add test plan test step
                            var tpStep = _qcRestClient.TestPlan.AddTestStepToTestCase(tC.Id, new DesignStepEntity()
                            {
                                Name = String.Concat(tsName, " ", i),
                                Description = testStep.StepDescription,
                                Expected = testStep.StepExpected
                            });
                            //add test lab test step
                            var tlStep = _qcRestClient.TestRun.AddTestStepToTestCase(trun.Fields.Where(p => p.Name == "id").Select(p => p.Value).ToList()[0], new RunStepEntity()
                            {
                                Name = String.Concat(tsName, " ", i),
                                Description = testStep.StepDescription,
                                Expected = testStep.StepExpected,
                                Actual = testStep.StepActual,
                                Status = testStep.StepType,
                                ParentId = testCase.ID.ToString(),
                                TestId = tC.Id,
                                DesStepId = tpStep.Id
                            });
                            stepStatuses.Add(testStep.StepType);
                            i++;
                        }
                        _qcRestClient.UnLockTest(tC.Id);

                        ///// updating run status
                        if (stepStatuses.Count > 0)
                        {
                            var runStat = QcStatuses.Passed;
                            if (stepStatuses.Contains(DbStatuses.Fail))
                            {
                                runStat = QcStatuses.Failed;
                            }
                            else if (stepStatuses.Contains(DbStatuses.NoRun))
                            {
                                runStat = QcStatuses.NotComplited;
                            }
                            _qcRestClient.TestRun.UpdateTestRunStatus(trun.Fields.Where(p => p.Name == "id").Select(p => p.Value).ToList()[0], runStat);
                        }
                    }
                }
                ServiceLocator.Current.GetInstance<IEmailer>().SendMail(new List<string> { _sessionHelper.GetSessionDetails(user).User.UserEmail }, "grs@clarivate.com",
                                            "[Global Reporting System - " + project.DisplayName +
                                            "] QC export result", Resources.EmailTemplateCommon.Replace("%user_name%", _sessionHelper.GetSessionDetails(user).User.USerFullName)
                                            .Replace("%text%", String.Concat("Export has been successfully ended. Chech out your test plan - ", project.QCTestPlan,
                                            ", test lab results - ", project.QcResultsPath, ".")));
            }
            catch (Exception ex)
            {
                ServiceLocator.Current.GetInstance<IEmailer>().SendMail(new List<string> { _sessionHelper.GetSessionDetails(user).User.UserEmail }, "grs@clarivate.com",
                                           "[Global Reporting System - " + _sessionHelper.GetSessionDetails(user).Project.ProjectName +
                                           "] QC export result", Resources.EmailTemplateCommon.Replace("%user_name%", _sessionHelper.GetSessionDetails(user).User.USerFullName)
                                           .Replace("%text%", String.Concat("Errors was occured during the export. See details below: <br />", ex.Message, "<br />Inner exception:"
                                           , ex.InnerException)));
            }
            finally
            {
                _qcRestClient.Dispose();
            }
        }

        private string getCriticality(string dbCriticality)
        {
            var validCriticality = new List<string>() { "Critical", "High", "Medium", "Low" };
            var criticality = "High";
            if (String.IsNullOrEmpty(dbCriticality))
            {
                var dbCriticalityFormatted = Char.ToUpper((dbCriticality.ToLower())[0]) + dbCriticality.Substring(1);
                if (validCriticality.Contains(dbCriticalityFormatted))
                {
                    criticality = dbCriticalityFormatted;
                }
            }
            return criticality;

        }

        private int getTestPlanFolderIdByPath(string path)
        {
            path = path.Replace('/', '\\');
            var hierarchy = path.Split('\\');
            var count = hierarchy.Length;
            var folderTree = _qcRestClient.TestPlan.GetFolderTree();
            var destinationFolderId = _qcRestClient.TestPlan.GetRootId();
            for (var i = 0; i < hierarchy.Length; i++)
            {
                var t = _qcRestClient.TestPlan.GetFolderId(hierarchy[i], destinationFolderId);
                if (t == -1 && i != hierarchy.Length - 1)
                {
                    throw new Exception(String.Format("Can't find folder {0}", String.Join("/", hierarchy.Take(i - 1))));
                }
                else
                {
                    destinationFolderId = t;
                }
            }
            return destinationFolderId;
        }
        private int getTestLabFolderIdByPath(string path)
        {
            path = path.Replace('/', '\\');
            var hierarchy = path.Split('\\');
            var count = hierarchy.Length;
            var folderTree = _qcRestClient.TestLab.GetFolderTree();
            var destinationFolderId = _qcRestClient.TestLab.GetRootId();
            for (var i = 0; i < hierarchy.Length; i++)
            {
                var t = _qcRestClient.TestLab.GetFolderId(hierarchy[i], destinationFolderId);
                if (t == -1 && i != hierarchy.Length - 1)
                {
                    throw new Exception(String.Format("Can't find folder {0} in Test Lab", String.Join("/", hierarchy.Take(i - 1))));
                }
                else
                {
                    destinationFolderId = t;
                }
            }
            return destinationFolderId;
        }
    }
}