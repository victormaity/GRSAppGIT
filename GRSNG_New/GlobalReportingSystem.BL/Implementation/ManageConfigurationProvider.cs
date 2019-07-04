using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Castle.Core.Internal;
using GlobalReportingSystem.BL.Helper;
using GlobalReportingSystem.BL.Properties;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Enums;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.Executor;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.GRS.DB;
using Ionic.Zip;
using System.Net;
using Microsoft.Practices.ServiceLocation;
using SharpSvn;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Messages = GlobalReportingSystem.Core.Constants.Messages;
using GlobalReportingSystem.DataLINQ;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageConfigurationProvider : Core.Models.GRS.Serializer<GlobalReportingSystem.Core.Models.GRS.DB.CustomStatuses>, IManageConfigurationProvider
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IRepository<vw_FilesStorage> _vwFilesStorageRepository;
        private readonly IRepository<AccountForTestRun> _accountForTestRunRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Access> _accessRepository;
        private readonly IRepository<TestSuit> _testSuitRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<FilesStorage> _fileStorageRepository;
        private readonly IRepository<HostsConfiguration> _hostConfigurationRepository;
        private readonly IRepository<ExecutionConfiguration> _executionConfigurationRepository;
        private readonly IRepository<SubStep> _subStepRepository;
        private readonly IRepository<Analysis> _analysesRepository;
        private readonly IRepository<Issue> _issueRepository;
        private readonly IRepository<SikuliObject> _sikuliObjectRepository;
        private readonly IRepository<QCExportAssignment> _qcExportAssignmentRepository;
        private readonly IRepository<vw_ActualTestSuites> _vwActualTestSuitesRepository;
        private readonly IRepository<Bugracker> _bugtackerObjectRepository;
        private readonly IRepository<LoadBalancedMachine> _loadBalancedMachine;
        private readonly IRepository<AnalysisHistory> _analysisHistory;
        private readonly IRepository<TestsExecution> _testsExecution;
        private readonly IRepository<Analyser> _analyser;
        private readonly IRepository<TestCase> _testCase;
        private readonly IRepository<TestStep> _testStep;
        private readonly IRepository<ClientsInformation> _clientsInformation;
        private readonly IRepository<TestCycle> _testCycle;
        private readonly IEmailer _emailer;
        private readonly IRepository<RelExecutionStatu> _relExecutionStatu;
        private readonly IRepository<RelExecutionStatusLog> _relExecutionStatusLog;
        private readonly IRepository<ProjectType> _projectType;
        private readonly IRepository<ExecutionGroup> _executionGroup;
        private readonly IRepository<GrpExecutionInfo> _grpExecutionInfo;

        public ManageConfigurationProvider(ISessionHelper sessionHelper,
            IRepository<vw_FilesStorage> vwFilesStorageRepository,
            IRepository<AccountForTestRun> accountForTestRunRepository,
            IRepository<Project> projectRepository,
            IRepository<User> userRepository,
            IRepository<Access> accessRepository,
            IRepository<TestSuit> testSuitRepository,
            IRepository<Client> clientRepository,
            IRepository<FilesStorage> fileStorageRepository,
            IRepository<HostsConfiguration> hostConfigurationeRepository,
            IRepository<ExecutionConfiguration> executionConfigurationRepository,
            IRepository<SubStep> subStepRepository,
            IRepository<Analysis> analysesRepository,
            IRepository<Issue> issueRepository,
            IRepository<QCExportAssignment> qcExportAssignmentRepository,
            IRepository<SikuliObject> sikuliObjectRepository,
            IRepository<Bugracker> bugtackerObjectRepository,
            IRepository<vw_ActualTestSuites> vwActualTestSuitesRepository,
            IRepository<LoadBalancedMachine> loadBalancedMachine,
            IRepository<AnalysisHistory> analysisHistory,
            IRepository<TestsExecution> testsExecution,
            IRepository<Analyser> analyser,
            IRepository<TestCase> testCase,
            IRepository<TestStep> testStep,
            IRepository<ClientsInformation> clientsInformation,
            IRepository<TestCycle> testCycle, IEmailer emailer,
            IRepository<RelExecutionStatu> relExecutionStatu,
            IRepository<RelExecutionStatusLog> relExecutionStatusLog,
            IRepository<ProjectType> projectType,
            IRepository<ExecutionGroup> executionGroup,
            IRepository<GrpExecutionInfo> grpExecutionInfo
            )
        {
            _sessionHelper = sessionHelper;
            _vwFilesStorageRepository = vwFilesStorageRepository;
            _accountForTestRunRepository = accountForTestRunRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _accessRepository = accessRepository;
            _testSuitRepository = testSuitRepository;
            _clientRepository = clientRepository;
            _fileStorageRepository = fileStorageRepository;
            _hostConfigurationRepository = hostConfigurationeRepository;
            _executionConfigurationRepository = executionConfigurationRepository;
            _subStepRepository = subStepRepository;
            _analysesRepository = analysesRepository;
            _issueRepository = issueRepository;
            _qcExportAssignmentRepository = qcExportAssignmentRepository;
            _sikuliObjectRepository = sikuliObjectRepository;
            _vwActualTestSuitesRepository = vwActualTestSuitesRepository;
            _bugtackerObjectRepository = bugtackerObjectRepository;
            _loadBalancedMachine = loadBalancedMachine;
            _analysisHistory = analysisHistory;
            _testsExecution = testsExecution;
            _analyser = analyser;
            _testCase = testCase;
            _testStep = testStep;
            _clientsInformation = clientsInformation;
            _testCycle = testCycle;
            _emailer = emailer;
            _relExecutionStatu = relExecutionStatu;
            _relExecutionStatusLog = relExecutionStatusLog;
            _projectType = projectType;
            _executionGroup = executionGroup;
            _grpExecutionInfo = grpExecutionInfo;
        }

        public CyclesModel GetCycles(IPrincipal user, string testcycletype)
        {
            var session = _sessionHelper.GetSessionDetails(user);

            if (!string.IsNullOrEmpty(testcycletype))
            {
                if (testcycletype.ToUpper() == "ALL")
                {
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "ALLACTIVE")
                {
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.isInnactive != true).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "ALLINACTIVE")
                {
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.isInnactive == true).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "THISYEAR")
                {
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart.Year == DateTime.Now.Year).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "LASTONEYEAR")
                {
                    DateTime startLimitWithTime = DateTime.Now.AddYears(-1);
                    DateTime startLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "LASTSIXMONTH")
                {
                    DateTime startLimitWithTime = DateTime.Now.AddMonths(-6);
                    DateTime startLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "THISMONTH")
                {
                    int sday = 1;
                    int month = DateTime.Now.Month;
                    int year = DateTime.Now.Year;
                    int eday = 0;
                    if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) { eday = 31; }
                    else if (month == 4 || month == 6 || month == 9 || month == 11) { eday = 30; }
                    else if (month == 2) { if (DateTime.IsLeapYear(year)) { eday = 29; } else { eday = 28; } }
                    DateTime startLimit = new DateTime(year, month, sday, 0, 0, 0);
                    DateTime endLimit = new DateTime(year, month, eday, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "LASTONEMONTH")
                {
                    DateTime startLimitWithTime = DateTime.Now.AddMonths(-1);
                    DateTime startLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "LASTONEWEEK")
                {
                    DateTime startLimitWithTime = DateTime.Now.AddDays(-7);
                    DateTime startLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "YESTERDAY")
                {
                    DateTime startLimitWithTime = DateTime.Now.AddDays(-1);
                    DateTime startLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(startLimitWithTime.Year, startLimitWithTime.Month, startLimitWithTime.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else if (testcycletype.ToUpper() == "TODAY")
                {
                    DateTime startLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    DateTime endLimit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                    };
                }
                else
                {
                    return new CyclesModel
                    {
                        CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart.Year == DateTime.Now.Year).OrderByDescending(p => p.ID).ToList()
                    };
                }
            }
            else
            {
                int sday = 1;
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                int eday = 0;
                if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) { eday = 31; }
                else if (month == 4 || month == 6 || month == 9 || month == 11) { eday = 30; }
                else if (month == 2) { if (DateTime.IsLeapYear(year)) { eday = 29; } else { eday = 28; } }
                DateTime startLimit = new DateTime(year, month, sday, 0, 0, 0);
                DateTime endLimit = new DateTime(year, month, eday, 23, 59, 59);
                return new CyclesModel
                {
                    CyclesFromDb = session.Project.TestCycles.Where(p => p.CycleStart >= startLimit && p.CycleStart <= endLimit).OrderByDescending(p => p.ID).ToList()
                };
            }

            //return new CyclesModel
            //{
            //    CyclesFromDb = session.Project.TestCycles.OrderByDescending(p => p.ID).ToList()
            //};            
        }

        public void AddCycle(IPrincipal user, CyclesModel model)
        {
            if (_sessionHelper.GetSessionDetails(user).Project.TestCycles.FirstOrDefault(p => p.CycleName.SequenceEqual(model.Name)) == null)
            {
                HelperConfigurationProvider HCP = new HelperConfigurationProvider();
                var release = HCP.GetReleaseInformationById(model.ReleaseId);
                var team = HCP.GetTeamInformationById(model.TeamId);
                string releasename = Convert.ToString(release.ReleaseName);
                string releasedate = (!string.IsNullOrEmpty(Convert.ToString(release.ReleaseDate))) ? release.ReleaseDate.ToString("MM/dd/yyyy") : DateTime.Now.ToString("MM/dd/yyyy");
                string teamname = Convert.ToString(team.TeamName);
                //if it is the first cycle for project set it as current
                var isCurrent = _sessionHelper.GetSessionDetails(user).Project.TestCycles.Count > 0 ? false : true;
                _sessionHelper.GetSessionDetails(user).Project.TestCycles.Add(new TestCycle
                {
                    CycleName = model.Name,
                    CycleStart = HCP.DateParseMM_DD_YYYY(model.Start),
                    CycleEnd = HCP.DateParseMM_DD_YYYY(model.End),
                    CycleComments = model.Comments ?? "",
                    CycleIsCurrent = isCurrent,
                    ReleaseName = releasename,
                    ReleaseDate = HCP.DateParseMM_DD_YYYY(releasedate),
                    TeamName = teamname
                });
                _sessionHelper.SaveChanges();
            }
            else
            {
                throw new Exception("Test cycle with this name already exists for this project!");
            }
        }

        public void SetCurrentTestCycle(IPrincipal user, int id)
        {
            var allCycles = _sessionHelper.GetSessionDetails(user).Project.TestCycles.ToList();
            allCycles.ForEach(p => p.CycleIsCurrent = false);
            var firstOrDefault = allCycles.FirstOrDefault(p => p.ID == id);
            if (firstOrDefault != null)
            {
                firstOrDefault.CycleIsCurrent = true;
            }
            _sessionHelper.SaveChanges();
        }

        public void SetTestCycleInnactive(IPrincipal user, int id)
        {
            var testCycle = _sessionHelper.GetSessionDetails(user).Project.TestCycles.FirstOrDefault(p => p.ID == id);
            if (!testCycle.isInnactive.HasValue)
            {
                testCycle.isInnactive = true;
                testCycle.CycleIsCurrent = false;

            }
            else
            {
                testCycle.isInnactive = !testCycle.isInnactive;
                if (testCycle.isInnactive.Value)
                {
                    testCycle.CycleIsCurrent = false;
                }
            }
            //if there are no cycles, marked as current
            if (!testCycle.Project.TestCycles.Any(p => p.CycleIsCurrent.HasValue && p.CycleIsCurrent.Value))
            {
                //select all active cycles
                var allActiveCycles = testCycle.Project.TestCycles.Where(p => p.isInnactive.HasValue ? !p.isInnactive.Value : false);
                if (allActiveCycles.Count() > 0)
                {
                    //set cycle with max dateEnd as current
                    var maxDateEnd = allActiveCycles.Max(p => p.CycleEnd);
                    allActiveCycles.FirstOrDefault(p => p.CycleEnd == maxDateEnd).CycleIsCurrent = true;
                }
            }
            _sessionHelper.SaveChanges();
        }

        public string DeleteTestCycles(IPrincipal user, int testCycleId)
        {
            try
            {
                int currentProjectId = _sessionHelper.GetSessionDetails(user).Project.ID;
                var testCycleData = _sessionHelper.GetSessionDetails(user).Project.TestCycles.FirstOrDefault(p => p.ID == testCycleId);
                if (testCycleData != null)
                {
                    if (testCycleData.ParentProject == currentProjectId)
                    {
                        //Get Analysis  and delete
                        var analysisData = _analysesRepository.GetAllToList(A => A.ParentTestCycle == testCycleData.ID);
                        foreach (var anitem in analysisData)
                        {
                            //Get AnalysisHistory and delete
                            var analysisHistoryData = _analysisHistory.GetAllToList(H => H.ParentAnalysis == anitem.ID);
                            foreach (var anhitem in analysisHistoryData)
                            {
                                var deleteAnalysisHistory = _analysisHistory.GetSingleOrDefault(h => h.ID == anhitem.ID);
                                _analysisHistory.Delete(deleteAnalysisHistory);
                                _analysisHistory.SaveChanges();
                            }
                            var deleteAnalysis = _analysesRepository.GetSingleOrDefault(a => a.ID == anitem.ID);
                            _analysesRepository.Delete(deleteAnalysis);
                            _analysesRepository.SaveChanges();
                        }

                        //Get LoadBalancedMachines and delete
                        var loadBalancedMachinedata = _loadBalancedMachine.GetAllToList(LM => LM.TargetTestCycle == testCycleData.ID);
                        foreach (var lbmitem in loadBalancedMachinedata)
                        {
                            var deleteLoadBalanceMachine = _loadBalancedMachine.GetSingleOrDefault(l => l.ID == lbmitem.ID);
                            _loadBalancedMachine.Delete(deleteLoadBalanceMachine);
                            _loadBalancedMachine.SaveChanges();
                        }

                        //Get TestsExecutions
                        var testExecutiondata = _testsExecution.GetAllToList(TE => TE.TargetTestCycle == testCycleData.ID);

                        List<int> testSuitIds = new List<int>();
                        foreach (var teitem in testExecutiondata)
                        {
                            //Collectiong Test Suit IDs
                            testSuitIds.Add(teitem.ID);
                        }

                        //Get TestSuits and delete
                        var testSuitData = _testSuitRepository.GetAllToList(TS => TS.ParentTestCycle == testCycleData.ID);
                        foreach (var tsitem in testSuitData)
                        {
                            //Colleting TestSuit Ids
                            testSuitIds.Add(tsitem.ID);

                            //Delete analyer data
                            var analysersData = _analyser.GetAllToList(A => A.TestSet == tsitem.ID);
                            foreach (var anlritem in analysersData)
                            {
                                var deleteAnalyser = _analyser.GetSingleOrDefault(a => a.ID == anlritem.ID);
                                _analyser.Delete(deleteAnalyser);
                                _analyser.SaveChanges();
                            }

                            //delete test case data
                            var testCaseData = _testCase.GetAllToList(TC => TC.ParentTestSuite == tsitem.ID);
                            foreach (var tcitem in testCaseData)
                            {
                                //delete Test Step
                                var testStepData = _testStep.GetAllToList(TS => TS.ParentTestCase == tcitem.ID);
                                foreach (var tstepitem in testStepData)
                                {
                                    //Delete Sub step
                                    var subStepData = _subStepRepository.GetAllToList(SS => SS.ParentStep == tstepitem.ID);
                                    foreach (var substepitem in subStepData)
                                    {
                                        var deleteSubStep = _subStepRepository.GetSingleOrDefault(a => a.ID == substepitem.ID);
                                        _subStepRepository.Delete(deleteSubStep);
                                        _subStepRepository.SaveChanges();
                                    }
                                    var deleteTestStep = _testStep.GetSingleOrDefault(a => a.ID == tstepitem.ID);
                                    _testStep.Delete(deleteTestStep);
                                    _testStep.SaveChanges();
                                }
                                var deleteTestCase = _testCase.GetSingleOrDefault(a => a.ID == tcitem.ID);
                                _testCase.Delete(deleteTestCase);
                                _testCase.SaveChanges();
                            }
                        }

                        //Delete TestSuit
                        List<int> fileteredTSIDS = (from x in testSuitIds select x).Distinct().ToList();
                        var finalTestSuitToDelete = _testSuitRepository.GetAllToList(TSS => fileteredTSIDS.Contains(TSS.ID));
                        foreach (var delTestSuititem in finalTestSuitToDelete)
                        {
                            var deleteTestSuit = _testSuitRepository.GetSingleOrDefault(a => a.ID == delTestSuititem.ID);
                            _testSuitRepository.Delete(deleteTestSuit);
                            _testSuitRepository.SaveChanges();

                            Int64 testSuitOtherThanCurrentCount = _testSuitRepository.Count(TSID => TSID.ParentClientInfo == delTestSuititem.ParentClientInfo && TSID.ParentProject != delTestSuititem.ParentProject && TSID.ParentTestCycle != delTestSuititem.ParentTestCycle);
                            if (testSuitOtherThanCurrentCount == 0)
                            {
                                //Delete Clients Informatioin 
                                var clientInformationData = _clientsInformation.GetAllToList(CI => CI.ID == delTestSuititem.ParentClientInfo);
                                foreach (var ciItem in clientInformationData)
                                {
                                    var deleteClientInformation = _clientsInformation.GetSingleOrDefault(a => a.ID == ciItem.ID);
                                    _clientsInformation.Delete(deleteClientInformation);
                                    _clientsInformation.SaveChanges();
                                }
                            }
                        }

                        //Delete Test Execution
                        foreach (var delTestExecutionItem in testExecutiondata)
                        {
                            var deleteTestExecution = _testsExecution.GetSingleOrDefault(a => a.ID == delTestExecutionItem.ID);
                            _testsExecution.Delete(deleteTestExecution);
                            _testsExecution.SaveChanges();
                        }
                    }
                    //Delete TestCycle 
                    var deleteTestCycle = _testCycle.GetSingleOrDefault(a => a.ID == testCycleData.ID);
                    _testCycle.Delete(deleteTestCycle);
                    _testCycle.SaveChanges();
                }
                return "Test cycle " + testCycleData.CycleName + " and all it's data deleted successfully.";
            }
            catch (Exception ex)
            {
                return "Exception occured while deleting test cycle. Exception : " + ex.Message;
            }
        }

        public ExecutionConfigurationsModel GetExecutionConfigurations(IPrincipal user)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            return new ExecutionConfigurationsModel { Configurations = project.ExecutionConfigurations };
        }

        public void SetExecutionConfiguration(IPrincipal user, ExecutionConfigurationsModel model)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            project.ExecutionConfigurations.Add(new ExecutionConfiguration
            {
                Name = model.Configuration.Name,
                FileName = model.Configuration.FileName,
                Content = model.Configuration.Content
            });
            _sessionHelper.SaveChanges();
        }

        public IEnumerable<vw_FilesStorage> GetVwFilesStorage(IPrincipal user)
        {
            var session = _sessionHelper.GetSessionDetails(user);
            return _vwFilesStorageRepository.GetAllToList(p => p.FilesStorage_BelongToProject == session.Project.ID);
        }

        public ExecutionEnvironmentsModel GetExecutionEnvironments(IPrincipal user)
        {
            var model = new ExecutionEnvironmentsModel();
            var project = _sessionHelper.GetSessionDetails(user).Project;
            model.Environments = project.HostsConfigurations;
            return model;
        }

        public void AddHostsConfiguration(IPrincipal user, ExecutionEnvironmentsModel model)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            project.HostsConfigurations.Add(new HostsConfiguration
            {
                EnvironmentName = model.Environment.EnvironmentName,
                ApplicationURL = model.Environment.ApplicationURL,
                HostFileContent = model.Environment.HostFileContent ?? ""
            });
            _sessionHelper.SaveChanges();
        }

        public ExecutionAccountsModel GetExecutionAccounts(IPrincipal user)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            return new ExecutionAccountsModel { AccountForTestRun = project.AccountForTestRuns };
        }

        public void AddAccountForTestRun(IPrincipal user, ExecutionAccountsModel model)
        {
            _sessionHelper.GetSessionDetails(user).Project.AccountForTestRuns.Add
           (new AccountForTestRun
           {
               AccountName = model.Account.AccountName,
               UserLogin = model.Account.UserLogin,
               UserPassword = model.Account.UserPassword,
               Comments = model.Account.Comments
           });
            _sessionHelper.SaveChanges();
        }

        public ProjectAndUsersModel GetProjectAndUsers(IPrincipal user)
        {
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            //var assignedUsers = new List<User>();
            //_accessRepository.GetAll(p => p.AttachedProject == pId).Select(p => p.AttachedUser).ToList()//GetAllToList()-->GetAll()
            //    .ForEach(p => assignedUsers.Add(_userRepository.GetSingleOrDefault(o => o.ID == p)));
            var model = new ProjectAndUsersModel
            {
                Project = _projectRepository.GetIncluding(p => p.Bugracker)
                            .SingleOrDefault(p => p.ID == pId),
                User = _userRepository.GetIncluding(p => p.Accesses).OrderBy(p => p.UserEmail).Where(x => x != null).ToList(),
                //AssignedUser = assignedUsers.Where(x => x != null).OrderBy(p => p.UserEmail).ToList(),
                TsNames = _vwActualTestSuitesRepository.GetAll(p => p.BelongToProject == pId).Select(p => p.TestSetName).ToList()
                //TsNames = _testSuitRepository.GetAll(p => p.ParentProject == pId).Select(p => p.TSName).Distinct().ToList()//GetAllToList()-->GetAll()
            };
            return model;
        }

        public ProjectAndUsersModel GetProjectAndUsers(IPrincipal user, string filter)
        {
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var model = new ProjectAndUsersModel
            {
                Project = _projectRepository.GetIncluding(p => p.Bugracker)
                            .SingleOrDefault(p => p.ID == pId),
                User = _userRepository.GetIncluding(p => p.Accesses).OrderBy(p => p.USerFullName)
                            .Where(x => (filter == "free") ? (x.Accesses.FirstOrDefault(p => p.AttachedProject == pId) == null) :
                                (x.Accesses.FirstOrDefault(p => p.AttachedProject == pId) != null)).ToList(),
                TsNames = _testSuitRepository.GetAll(p => p.ParentProject == pId).Select(p => p.TSName).Distinct().ToList()
            };
            return model;
        }

        public List<Project> GetAvailableProjects()
        {
            var projects = _projectRepository.GetAllToList().OrderBy(s => s.DisplayName).ToList();
            return projects;
        }

        public ProjectAndUsersModel GetProjectAndUsers(int projectId)
        {
            //var assignedUsers = new List<User>();
            //_accessRepository.GetAll(p => p.AttachedProject == pId).Select(p => p.AttachedUser).ToList()//GetAllToList()-->GetAll()
            //    .ForEach(p => assignedUsers.Add(_userRepository.GetSingleOrDefault(o => o.ID == p)));
            var model = new ProjectAndUsersModel
            {
                Project = _projectRepository.GetIncluding(p => p.Bugracker)
                            .SingleOrDefault(p => p.ID == projectId),
                User = _userRepository.GetIncluding(p => p.Accesses).OrderBy(p => p.UserEmail).Where(x => x != null).ToList(),
                //AssignedUser = assignedUsers.Where(x => x != null).OrderBy(p => p.UserEmail).ToList(),
                TsNames = _vwActualTestSuitesRepository.GetAll(p => p.BelongToProject == projectId).Select(p => p.TestSetName).ToList()
                //TsNames = _testSuitRepository.GetAll(p => p.ParentProject == pId).Select(p => p.TSName).Distinct().ToList()//GetAllToList()-->GetAll()
            };
            return model;
        }

        public ProjectAndUsersModel GetProjectAndUsers(int projectId, string filter)
        {
            var model = new ProjectAndUsersModel
            {
                Project = _projectRepository.GetIncluding(p => p.Bugracker)
                            .SingleOrDefault(p => p.ID == projectId),
                User = _userRepository.GetIncluding(p => p.Accesses).OrderBy(p => p.USerFullName)
                            .Where(x => (filter == "free") ? (x.Accesses.FirstOrDefault(p => p.AttachedProject == projectId) == null) :
                                (x.Accesses.FirstOrDefault(p => p.AttachedProject == projectId) != null)).ToList()
            };
            return model;
        }

        public ExecutionMachniesModel GetExecutionMachnies(IPrincipal user, string filter, ref string message)
        {
            var model = new ExecutionMachniesModel();
            var all = _clientRepository.GetIncluding(new Expression<Func<Client, object>>[] { p => p.User, p => p.Project })
                        .ToList();
            if (!string.IsNullOrEmpty(filter))
            {
                var machinesStType = (MachinesStatusType)Enum.Parse(typeof(MachinesStatusType), filter, true);
                var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
                switch (machinesStType)
                {
                    case MachinesStatusType.Free:
                        all = all.Where(p => p.Project == null).ToList();
                        message = "Only free machines";
                        break;
                    case MachinesStatusType.Assigned:
                        all = all.Where(p => p.BelongToProject == pId).ToList();
                        message = "Assigned to current project";
                        break;
                }
            }
            else
            {
                message = "All available machines";
            }
            model.Machines = all;
            return model;
        }

        public void AddClient(IPrincipal user, ExecutionMachniesModel model, ref string message)
        {
            if (_clientRepository.Count(p => p.RemoteMachineIP == model.Client.RemoteMachineIP) != 0)
            {
                message = Messages.AddMachineMessage;
            }
            else
            {
                _sessionHelper.GetSessionDetails(user).Project.Clients.Add(new Client
                {
                    RemoteMachineIP = model.Client.RemoteMachineIP,
                    AddedDate = DateTime.Now,
                    User = _sessionHelper.GetSessionDetails(user).User
                });
                _sessionHelper.SaveChanges();
            }
        }

        public void DeleteClient(IPrincipal user, int id)
        {
            var machine = _clientRepository.GetSingleOrDefault(p => p.ID == id);

            if (machine != null)
            {
                _sessionHelper.GetSessionDetails(user).Project.Clients.Remove(machine);
                _sessionHelper.SaveChanges();
                _clientRepository.Delete(machine);
                _clientRepository.SaveChanges();
            }
        }

        public void Sync(string zipFileLocation, IPrincipal user, string serverMapPath)
        {
            var fileLocation = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(fileLocation);
            var name = Path.GetFileNameWithoutExtension(zipFileLocation);
            using (var zFileFile = new ZipFile(zipFileLocation))
            {
                zFileFile.ParallelDeflateThreshold = -1;
                zFileFile.ExtractAll(fileLocation);
                zFileFile.Dispose();
            }


            if (!System.IO.File.Exists(fileLocation + @"\MultiUnit.xml"))
                throw new Exception(Messages.ZipArchiveHasWrongFormatException);
            string assembly = getXpathValue(System.IO.File.ReadAllText(fileLocation + @"\MultiUnit.xml"),
                    "//TestsAssembly");
            if (string.IsNullOrEmpty(assembly))
                throw new Exception(Messages.ConfigFileCanNotFoundException);
            var parser = new FrameworkParser();
            var xmlResponse = parser.GetFrameworkStructure(fileLocation + @"\" + assembly,
                serverMapPath + @"\bin");

            //((IObjectContextAdapter)db).ObjectContext.CommandTimeout = 500;
            var curSessionProjectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var exitingFile =
                _fileStorageRepository.GetSingleOrDefault(p => p.BelongToProject == curSessionProjectId && p.Name == name);
            if (exitingFile == null)
            {
                var singleOrDefault = _projectRepository.GetSingleOrDefault(p => p.ID == curSessionProjectId);
                if (singleOrDefault != null)
                {
                    _sessionHelper.GetSessionDetails(user).Project.FilesStorages.Add(new FilesStorage
                    {
                        AddedDate = DateTime.Now,
                        FileContent = File.ReadAllBytes(zipFileLocation/* + @"\" + assembly*/),
                        Comments = xmlResponse.Serialize(),
                        Name = name,
                        AddedBy = _sessionHelper.GetSessionDetails(user).User.ID,
                        BelongToProject = _sessionHelper.GetSessionDetails(user).Project.ID,
                    });
                }
            }
            else
            {
                exitingFile.FileContent = System.IO.File.ReadAllBytes(zipFileLocation/* + @"\" + assembly*/);
                exitingFile.Comments = xmlResponse.Serialize();
                exitingFile.AddedDate = DateTime.Now;
                exitingFile.Name = name;
                exitingFile.AddedBy = _sessionHelper.GetSessionDetails(user).User.ID;
            }

            _sessionHelper.SaveChanges();
            _fileStorageRepository.SaveChanges();

            try
            {
                Directory.Delete(Path.GetDirectoryName(fileLocation), true);
            }
            catch
            {
            }
        }
        private string getXpathValue(string file, string query)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(file);
            var selectSingleNode = xDoc.SelectSingleNode(query);
            if (selectSingleNode != null) return selectSingleNode.InnerText;
            return null;
        }

        public void DeleteFileStorage(IPrincipal user, int id)
        {
            _sessionHelper.GetSessionDetails(user)
                .Project.FilesStorages.Remove(_sessionHelper.GetSessionDetails(user)
                .Project.FilesStorages.FirstOrDefault(p => p.ID == id));
            _sessionHelper.SaveChanges();
        }

        public void DeleteAccount(IPrincipal user, int id)
        {
            _sessionHelper.GetSessionDetails(user)
                 .Project.AccountForTestRuns.Remove(_sessionHelper.GetSessionDetails(user)
                 .Project.AccountForTestRuns.FirstOrDefault(p => p.ID == id));
            _sessionHelper.SaveChanges();
        }

        public void DeleteHostsConfiguration(IPrincipal user, int id)
        {
            _sessionHelper.GetSessionDetails(user)
               .Project.HostsConfigurations.Remove(_sessionHelper.GetSessionDetails(user)
               .Project.HostsConfigurations.FirstOrDefault(p => p.ID == id));
            _sessionHelper.SaveChanges();
        }
        public void DeleteExecutionConfiguration(IPrincipal user, int id)
        {
            _sessionHelper.GetSessionDetails(user)
              .Project.ExecutionConfigurations.Remove(_sessionHelper.GetSessionDetails(user)
              .Project.ExecutionConfigurations.FirstOrDefault(p => p.ID == id));
            _sessionHelper.SaveChanges();
        }

        public void EditExecutionConfiguration(IPrincipal user, int id, string name, string fileName, string content)
        {
            var configuration = _sessionHelper.GetSessionDetails(user)
                .Project.ExecutionConfigurations.SingleOrDefault(p => p.ID == id);
            if (configuration != null)
            {
                configuration.Name = name;
                configuration.FileName = fileName;
                configuration.Content = content;
                _sessionHelper.SaveChanges();
            }
        }

        public void EditExecutionAccount(IPrincipal user, int id, string name, string login, string password,
            string comments)
        {
            var configuration = _sessionHelper.GetSessionDetails(user)
    .Project.AccountForTestRuns.SingleOrDefault(p => p.ID == id);
            if (configuration != null)
            {
                configuration.AccountName = name;
                configuration.UserLogin = login;
                configuration.UserPassword = password;
                configuration.Comments = comments;
                _sessionHelper.SaveChanges();
            }
        }

        public void EditExecutionEnvironment(IPrincipal user, int id, string name, string url, string content)
        {
            var configuration = _sessionHelper.GetSessionDetails(user)
.Project.HostsConfigurations.SingleOrDefault(p => p.ID == id);
            if (configuration != null)
            {
                configuration.EnvironmentName = name;
                configuration.ApplicationURL = url;
                configuration.HostFileContent = content;
                _sessionHelper.SaveChanges();
            }
        }

        public void EditTestCycle(IPrincipal user, int id, string name, string start, string end, string comment, string ReleaseName, string ReleaseDate, string TeamName)
        {
            var tcycle = _sessionHelper.GetSessionDetails(user).Project.TestCycles.SingleOrDefault(p => p.ID == id);
            if (tcycle != null)
            {
                tcycle.CycleName = name;
                tcycle.CycleStart = DateTime.Parse(start);
                tcycle.CycleEnd = DateTime.Parse(end);
                tcycle.CycleComments = comment;
                tcycle.ReleaseName = ReleaseName;
                tcycle.TeamName = TeamName;
                try { tcycle.ReleaseDate = DateTime.Parse(ReleaseDate); }
                catch { }
                _sessionHelper.SaveChanges();
            }
        }

        public void SetMachineToProject(IPrincipal user, int id)
        {
            var proj = _sessionHelper.GetSessionDetails(user);
            _clientRepository.GetSingleOrDefault(p => p.ID == id).BelongToProject = proj.Project.ID;
            _clientRepository.SaveChanges();
            _sessionHelper.SaveChanges();
        }

        public void ReleaseMachine(int id)
        {
            _clientRepository.GetSingleOrDefault(p => p.ID == id).BelongToProject = null;
            _clientRepository.SaveChanges();
        }

        public void AssignUserOnProject(IPrincipal user, int id)
        {
            var userToSubscribe = _userRepository.GetFirstOrDefault(p => p.ID == id);
            var proj = _sessionHelper.GetSessionDetails(user).Project;
            proj.Accesses.Add(new Access
            {
                AttachedUser = id,
                DeliveryResult = false
            });
            _sessionHelper.SaveChanges();
            var message = @"Your GRS account has a new subscription to: <b>" + proj.DisplayName + "</b> project.";
            ServiceLocator.Current.GetInstance<IEmailer>()
                         .SendMail(new List<string> { userToSubscribe.UserEmail }, "grs.notifications@thomsonreuters.com",
                                   "[Global Reporting System - System notification]",
                                   Resources.EmailTemplateCommon.Replace("%user_name%", userToSubscribe.USerFullName)
                                            .Replace("%text%", message.Replace(Environment.NewLine, "<br />")));
        }

        public void AssignUserOnProject(int projectId, int userId)
        {
            var userToSubscribe = _userRepository.GetFirstOrDefault(p => p.ID == userId);
            var proj = _projectRepository.GetFirstOrDefault(p => p.ID == projectId);
            proj.Accesses.Add(new Access
            {
                AttachedUser = userId,
                DeliveryResult = false
            });
            _projectRepository.SaveChanges();
            //var message = @"Your GRS account has a new subscription to: <b>" + proj.DisplayName + "</b> project.";
            //ServiceLocator.Current.GetInstance<IEmailer>()
            //             .SendMail(new List<string> { userToSubscribe.UserEmail }, "grs.notifications@thomsonreuters.com",
            //                       "[Global Reporting System - System notification]",
            //                       Resources.EmailTemplateCommon.Replace("%user_name%", userToSubscribe.USerFullName)
            //                                .Replace("%text%", message.Replace(Environment.NewLine, "<br />")));
        }

        public void UnassignUser(IPrincipal user, int id)
        {
            var proj = _sessionHelper.GetSessionDetails(user).Project;
            var acces = proj.Accesses.FirstOrDefault(p => p.ID == id);
            proj.Accesses.Remove(acces);
            _sessionHelper.SaveChanges();
        }

        public void UnassignUser(int projectId, int userId)
        {
            //var proj = _projectRepository.GetFirstOrDefault(p => p.ID == projectId);
            //var acces = proj.Accesses.FirstOrDefault(p => p.AttachedProject == projectId && p.AttachedUser == userId);
            //proj.Accesses.Remove(acces);
            //_projectRepository.SaveChanges();
            var accessremove = _accessRepository.GetFirstOrDefault(p => p.AttachedUser == userId && p.AttachedProject == projectId);
            _accessRepository.Delete(accessremove);
            _accessRepository.SaveChanges();
        }

        public List<string> GetListOfPossibleDefects(IPrincipal user, out string bugtrackerLogin, out string bugtrackerPass, out int projId)
        {
            var proj = _sessionHelper.GetSessionDetails(user).Project;
            projId = proj.ID;
            bugtrackerLogin = proj.Bugracker.Login;
            bugtrackerPass = proj.Bugracker.Password;
            var listOfPossibleDefects = new List<string>();
            var allDefects =
                _subStepRepository.GetAll( //GetAllToList()-->GetAll
                    p => !string.IsNullOrEmpty(p.Defects) && p.TestStep.TestCase.TestSuit.Project.ID == proj.ID)
                    .Select(p => p.Defects).ToList();
            allDefects.AddRange(
                _analysesRepository.GetAll(//GetAllToList()-->GetAll
                    p => !string.IsNullOrEmpty(p.CurrentDefects) && p.BelongToProject == proj.ID)
                    .Select(p => p.CurrentDefects)
                    .ToList());

            if (allDefects.Count().Equals(0)) throw new Exception(Messages.NoDefectsToUpdateException);

            allDefects.ForEach(p => listOfPossibleDefects.AddRange(p.ToUpper().Replace(" ", "").Trim().Split(',')));
            listOfPossibleDefects =
                listOfPossibleDefects.Distinct().Where(p => p.Contains("-")).ToList();
            _issueRepository.Delete(p => p.BelongToProject == proj.ID);
            _issueRepository.SaveChanges();
            return listOfPossibleDefects;
        }

        public Issue CreateIssue(int projId, string defect)
        {
            return new Issue()
            {
                BelongToProject = projId,
                DefectId = defect
            };
        }
        public void SetIssueStatus(bool isJiraIssueIsNotNull, string jiraIssueSummary, Issue newIssue, string theStatName)
        {
            if (isJiraIssueIsNotNull)
            {
                newIssue.Title = jiraIssueSummary;
                var issueStType = (IssueStatusType)Enum.Parse(typeof(IssueStatusType), theStatName.Replace(" ", ""), true);
                switch (issueStType)
                {
                    case IssueStatusType.Open:
                    case IssueStatusType.Resolved:
                    case IssueStatusType.InProgress:
                    case IssueStatusType.Reopened:
                    case IssueStatusType.Validating:
                        newIssue.Status = "O";
                        break;
                    case IssueStatusType.Closed:
                    case IssueStatusType.Validated:
                        newIssue.Status = "C";
                        break;
                    default:
                        newIssue.Status = "U";
                        break;

                }
            }
            else newIssue.Status = "!";
            _issueRepository.Add(newIssue);
            _issueRepository.SaveChanges();
        }

        public List<Status> GetCustomStatuses(IPrincipal user)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var customStatuses = project.CustomStatuses != null ? Deserialize(project.CustomStatuses).Statuses : new List<Status>();
            return customStatuses;
        }
        public Status GetCustomStatus(IPrincipal user, string name)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var customStatuses = project.CustomStatuses != null ? Deserialize(project.CustomStatuses).Statuses : new List<Status>();
            return customStatuses.FirstOrDefault(p => p.StatusName == name);
        }

        public bool CheckUniqueValues(IPrincipal user, string color, string statusName, string statusID, Guid uniqueID, ref string message)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var customStatuses = project.CustomStatuses != null ? Deserialize(project.CustomStatuses).Statuses : new List<Status>();
            if (customStatuses.Count() != 0)
            {
                if (customStatuses.FirstOrDefault(p => p.Color == color && p.UniqueID != uniqueID) != null)
                {
                    message = "This color already exists for another custom status, please, choose another color.";
                    return false;
                }
                if (customStatuses.FirstOrDefault(p => p.StatusName == statusName && p.UniqueID != uniqueID) != null)
                {
                    message = "This status name already exists for another custom status, please, choose another name.";
                    return false;
                }
                if (customStatuses.FirstOrDefault(p => p.StatusID == statusID && p.UniqueID != uniqueID) != null)
                {
                    message = "This status id already exists for another custom status, please, choose another id.";
                    return false;
                }
            }
            return true;
        }

        public void AddCustomStatus(IPrincipal user, Status status)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var customStatuses = project.CustomStatuses != null ? Deserialize(project.CustomStatuses).Statuses : new List<Status>();
            var custStatus = new GlobalReportingSystem.Core.Models.GRS.DB.CustomStatuses();
            customStatuses.Add(status);
            custStatus.Statuses = customStatuses;
            project.CustomStatuses = Serialize(custStatus);
            _sessionHelper.SaveChanges();
        }
        public void EditCustomStatus(IPrincipal user, Status status)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var customStatuses = project.CustomStatuses != null ? Deserialize(project.CustomStatuses).Statuses : new List<Status>();
            var custStatus = new GlobalReportingSystem.Core.Models.GRS.DB.CustomStatuses();
            var itemCustomStatus = customStatuses.FirstOrDefault(p => p.UniqueID == status.UniqueID);
            if (itemCustomStatus != null)
            {
                itemCustomStatus.Priority = status.Priority;
                itemCustomStatus.StatusID = status.StatusID;
                itemCustomStatus.StatusName = status.StatusName;
                itemCustomStatus.Description = status.Description;
                itemCustomStatus.Color = status.Color;
                itemCustomStatus.CountAsPass = status.CountAsPass;
                itemCustomStatus.NotAddToCache = status.NotAddToCache;
            }
            custStatus.Statuses = customStatuses;
            project.CustomStatuses = Serialize(custStatus);
            _sessionHelper.SaveChanges();
        }

        public List<SikuliObject> GetSikuliObjects(IPrincipal user)
        {
            return _sessionHelper.GetUserSession_Services(user).Project.SikuliObjects.ToList();
        }

        public FilesStorage GetFramework(int id)
        {
            return _fileStorageRepository.GetSingleOrDefault(p => p.ID == id);
        }

        public int GetProject(IPrincipal user)
        {
            var proj = _sessionHelper.GetSessionDetails(user).Project;
            return proj.ID;
        }

        //public string UpdateFrameworkFromSvn(int projId, IPrincipal user, string path)
        //{
        //    string projName = _projectRepository.GetFirstOrDefault_Services(p => p.ID == projId).ProjectName;
        //    string svnPath = _projectRepository.GetFirstOrDefault_Services(p => p.ID == projId).SVNpath;
        //    string svnPass = _projectRepository.GetFirstOrDefault_Services(p => p.ID == projId).SVNpassword;
        //    string svnLogin = _projectRepository.GetFirstOrDefault_Services(p => p.ID == projId).SVNlogin;

        //    string outPath, debugPath, zipPath;
        //    if (svnPath != null && svnPass != null && svnLogin != null)
        //    {
        //        string discPath = @"D:\GRS\SVN_NEW2\" + projName;
        //        LogMessage("SVNCheckoutUpdate start");
        //        if (SVNCheckoutUpdate(svnPath, svnPass, svnLogin, discPath))
        //        {
        //            LogMessage("SVNCheckoutUpdate completed");
        //            LogMessage("BuildProject start");
        //            if (BuildProject(projName, discPath, out outPath, out debugPath))
        //            {
        //                LogMessage("BuildProject completed");
        //                LogMessage("ZIPproject start");
        //                ZIPproject(projName, projId, debugPath, out zipPath);
        //                LogMessage("ZIPproject completed");
        //                if (user != null)
        //                {
        //                    LogMessage("Sync start");
        //                    Sync(zipPath, user, path);
        //                    LogMessage("Sync completed");
        //                }

        //                string[] zipFiles = Directory.GetFiles(@debugPath).Where(p => p.EndsWith(".zip")).ToArray();
        //                foreach (string file in zipFiles)
        //                {
        //                    File.Delete(file);
        //                }
        //            }
        //            else
        //            {
        //                LogMessage("throw new Exception: Project was uploaded from SVN but wasn't built.");
        //                throw new Exception("Project was uploaded from SVN but wasn't built.");
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("SVN credentials are wrong. Please update SVN info.");
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("SVN credentials are empty. Please fill SVN info.");
        //    }

        //    return zipPath;
        //}

        private bool SVNCheckoutUpdate(string svnPath, string svnPass, string svnLogin, string discPath)
        {
            bool isSucceeded = false;
            killFileProcesses(discPath);
            ProcessStartInfo strtInfo = new ProcessStartInfo(@"svn.exe", String.Format("checkout {0} {1} --username {2} --password {3}", svnPath, discPath, svnPass, svnLogin));
            if (!Directory.Exists(discPath)) //C:\Program Files (x86)\Subversion\bin\
                Process.Start(strtInfo).WaitForExit();
            Process.Start(@"svn.exe", String.Format("cleanup {0}", discPath)).WaitForExit();
            Process.Start(@"svn.exe", String.Format("update {0} --username {1} --password {2}", discPath, svnPass, svnLogin)).WaitForExit();
            var folder = new DirectoryInfo(discPath);
            if (folder.Exists)
            {
                isSucceeded = folder.GetFileSystemInfos().Length != 0;
            }

            return isSucceeded;
        }

        private static void killFileProcesses(string strFile)
        {
            Process[] processes = Process.GetProcesses();

            for (var i = 0; i < processes.GetUpperBound(0); i++)
            {
                var myProcess = processes[i];
                try
                {
                    if (!myProcess.HasExited)
                    {
                        ProcessModuleCollection modules = Process.GetCurrentProcess().Modules;

                        for (var j = 0; j < modules.Count; j++)
                        {
                            if (modules[j].FileName.ToLower().Contains(strFile.ToLower()))
                            {
                                myProcess.Kill();
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private bool BuildProject(string projName, string discPath, out string outputPath, out string debugPath)
        {
            LogMessage("Inside BuildProject");
            string csprojRegex =
                String.Concat(
                    "Project\\(\"\\{[A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12}\\}\"\\) *= *\"",
                    projName, "\" *, *\"(.+\\.csproj)\"");
            string slnPath = System.IO.Directory.GetFiles(@discPath, "*.sln").FirstOrDefault();
            Match match = Regex.Match(File.ReadAllText(slnPath), csprojRegex);
            string csprojPath = match.Groups[1].Value;
            string projPath = string.Format(@"{0}\{1}", @discPath, csprojPath);
            bool success = false;

            //if (File.Exists(@discPath + "\\" + csprojPath.Replace(projName + ".csproj", "") + "packages.config"))
            //{
            //    LogMessage("NuGet restore start");
            //    try
            //    {
            //        LogMessage("NuGet restore try to change proxy");
            //        Process.Start("nuget.exe", "config -set http_proxy=http://webproxy.h.corp.services:80").WaitForExit();
            //        LogMessage("NuGet restore finish changing proxy");
            //        Process.Start("nuget.exe", "restore " + slnPath + " -source " + "http://packages.nuget.org/v1/FeedService.svc").WaitForExit();
            //        LogMessage("NuGet restore passed");
            //    }
            //    catch (Exception e)
            //    { LogMessage("NuGet restore failed"); }
            //}

            Microsoft.Build.Evaluation.Project proj = null;

            try
            {
                LogMessage("Inside msbuild");
                proj = new Microsoft.Build.Evaluation.Project(projPath);
                outputPath = proj.GetProperty("OutDir").EvaluatedValue.Replace(@"\\", @"\");

                if (!outputPath.StartsWith(@discPath))
                {
                    debugPath = string.Format(@"{0}\{1}\{2}", @discPath, csprojPath.Replace(projName + ".csproj", ""), @outputPath).Replace(@"\\", @"\");
                    proj.SetProperty("OutDir", debugPath);
                }
                else
                {
                    debugPath = @outputPath;
                }

                //success = proj.Build();
                try
                {
                    LogMessage("Start msbuild");
                    Process.Start("msbuild", slnPath).WaitForExit();
                    success = true;
                    LogMessage("Msbuild succeeded");
                }
                catch (Exception e)
                {
                    LogMessage("Msbuild failed: " + e.Message);
                    success = false;
                }
            }
            finally
            {
                LogMessage("Finally");
                if (proj != null)
                {
                    Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(proj);
                }
            }
            return success;
        }

        public void ZIPproject(string projName, int projId, string debugPath, out string zipPath)
        {
            zipPath = string.Format(@debugPath + "\\From_SVN.zip");

            var AllConfigurations = new List<string>();

            AllConfigurations = _executionConfigurationRepository.GetAll(p => p.BelongToProject == projId).Select(p => @"\" + p.FileName).ToList();
            AllConfigurations.Add(@"\GRS_Configuration.xml");
            AllConfigurations = AllConfigurations.Distinct().ToList();

            using (ZipFile zip = new ZipFile())
            {
                string[] directories = Directory.GetDirectories(@debugPath);
                string[] files =
                    Directory.GetFiles(@debugPath)
                        .Where(p => !AllConfigurations.Contains(@"\" + p.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last())
                    && !p.EndsWith(".pdb") && !p.EndsWith(".tmp"))
                        .ToArray();

                foreach (string file in files)
                {
                    zip.AddFile(file, "");
                }

                foreach (string directory in directories)
                {
                    zip.AddDirectory(directory, "");
                }
                zip.Save(zipPath);
            }
        }

        public Dictionary<string, string> GetQcPath(int projectId, string testSet)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(testSet))
            {
                var assignment = _qcExportAssignmentRepository.GetFirstOrDefault_Services(p => p.BelongToProject == projectId && p.TestSetName.ToLower().Equals(testSet.ToLower()));
                if (assignment == null)
                {
                    _qcExportAssignmentRepository.Add(new QCExportAssignment()
                    {
                        TestSetName = testSet,
                        BelongToProject = projectId
                    });
                    _qcExportAssignmentRepository.SaveChanges();
                    assignment = _qcExportAssignmentRepository.GetFirstOrDefault_Services(p => p.BelongToProject == projectId && p.TestSetName.ToLower().Equals(testSet.ToLower()));
                }
                result.Add("id", assignment.ID.ToString());
                result.Add("qcPath", assignment.QCPath);
            }
            else
            {
                throw new Exception("QC test set name is empty!");
            }
            return result;
        }

        public bool SetQcPath(string testSetId, string qcPath)
        {
            bool isUpdated = false;
            int id = 0;
            id = Int32.Parse(testSetId);
            var assignment = _qcExportAssignmentRepository.GetFirstOrDefault(p => p.ID == id);
            assignment.QCPath = qcPath;
            _qcExportAssignmentRepository.SaveChanges();
            if (_qcExportAssignmentRepository.GetFirstOrDefault_Services(p => p.ID == id && p.QCPath == qcPath) != null)
            {
                isUpdated = true;
            }
            return isUpdated;
        }

        //public List<string> SubmitProjectSettings(Dictionary<string, string> values, int projectId, IPrincipal user)
        //{
        //    var updatedFields = new List<string>();
        //    var project = _projectRepository.GetFirstOrDefault(p => p.ID == projectId);
        //    var qcExportAssignment = _qcExportAssignmentRepository.GetAll(p => p.BelongToProject == projectId).ToList();
        //    string projectName, displayName, bugtracker, svnPath, svnLogin, svnPass, qcPath, qcServer, qcResultsPath, qcTestPlanPath, reviewTestPlan;
        //    if (values.TryGetValue("projectId", out projectName) && values.TryGetValue("projectName", out displayName) && values.TryGetValue("bugtracker", out bugtracker))
        //    {
        //        if (project.ProjectName != projectName)
        //        {
        //            project.ProjectName = projectName;
        //            updatedFields.Add("projectId");
        //        }
        //        if (project.DisplayName != displayName)
        //        {
        //            project.DisplayName = displayName;
        //            updatedFields.Add("projectName");
        //        }
        //        if (project.AssignedBugTracker != Convert.ToInt32(bugtracker))
        //        {
        //            project.AssignedBugTracker = Convert.ToInt32(bugtracker);
        //            updatedFields.Add("bugtracker");
        //        }
        //        _projectRepository.SaveChanges();
        //    }
        //    if (values.TryGetValue("svnPath", out svnPath) && values.TryGetValue("svnLogin", out svnLogin) && values.TryGetValue("svnPass", out svnPass))
        //    {

        //        if (CheckIfSVNValuesAreValid(svnLogin, svnPass, svnPath))
        //        {

        //            if (project.SVNpath != svnPath)
        //            {
        //                project.SVNpath = svnPath;
        //                updatedFields.Add("svnPath");
        //            }

        //            if (project.SVNlogin != svnLogin)
        //            {
        //                project.SVNlogin = svnLogin;
        //                updatedFields.Add("svnLogin");
        //            }

        //            if (project.SVNpassword != svnPass)
        //            {
        //                project.SVNpassword = svnPass;
        //                updatedFields.Add("svnPass");
        //            }
        //            _projectRepository.SaveChanges();

        //            UpdateFrameworkFromSvn(projectId, user, svnPath);
        //        }
        //        else
        //        {
        //            throw new Exception("SVN credentials\\path are not valid!");
        //        }
        //    }
        //    if (values.TryGetValue("qcPath", out qcPath) && values.TryGetValue("qcServer", out qcServer) && values.TryGetValue("qcResultsPath", out qcResultsPath) && values.TryGetValue("qcTestPlanPath", out qcTestPlanPath))
        //    {
        //        if (project.QCLocation != qcPath)
        //        {
        //            project.QCLocation = qcPath;
        //            updatedFields.Add("qcPath");
        //        }
        //        if (project.QCServer != qcServer)
        //        {
        //            project.QCServer = qcServer;
        //            updatedFields.Add("qcServer");
        //        }
        //        if (project.QcResultsPath != qcResultsPath)
        //        {
        //            project.QcResultsPath = qcResultsPath;
        //            updatedFields.Add("qcResultsPath");
        //        }
        //        if (project.QCTestPlan != qcTestPlanPath)
        //        {
        //            qcTestPlanPath = qcTestPlanPath != null ? qcTestPlanPath.TrimEnd('\\') : "";
        //            project.QCTestPlan = qcTestPlanPath;
        //            updatedFields.Add("qcTestPlanPath");
        //        }
        //        if (values.TryGetValue("reviewTestPlan", out reviewTestPlan))
        //        {
        //            for (int i = 0; i < qcExportAssignment.Count; i++)
        //            {
        //                qcExportAssignment[i].QCPath = String.Concat(qcTestPlanPath, "\\", qcExportAssignment[i].TestSetName);
        //            }
        //            updatedFields.Add("testSets");
        //        }
        //        _projectRepository.SaveChanges();
        //        _qcExportAssignmentRepository.SaveChanges();
        //    }

        //    return updatedFields;
        //}

        void Authentication_SslServerTrustHandlers(object sender, SharpSvn.Security.SvnSslServerTrustEventArgs e)
        {
            // Look at the rest of the arguments of E, whether you wish to accept
            // If accept:
            e.AcceptedFailures = e.Failures;
            e.Save = true; // Save acceptance to authentication store
        }

        public string PerformSvnSync(IPrincipal user, string path)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var projName = project.ProjectName;
            string returnmessage = string.Empty;

            bool IsProjectUpdatingNow = project.IsProjectUpdatingOnServer == true ? true : false;
            int projectUpdatedingBy = project.ProjectUpdatingBy ?? 0;
            int projectId = project.ID;
            int userid = _sessionHelper.GetSessionDetails(user).User.ID;
            string userFullName = _sessionHelper.GetSessionDetails(user).User.USerFullName;

            if (project != null && IsProjectUpdatingNow == false)
            {
                //Update Project section  - isprojectupdatingonserver to true and updated by with current userid
                var updateProjectRow = _projectRepository.GetSingleOrDefault(p => p.ID == projectId);
                updateProjectRow.IsProjectUpdatingOnServer = true;
                updateProjectRow.ProjectUpdatingBy = userid;
                _projectRepository.SaveChanges();

                //Start Update
                if ((!string.IsNullOrEmpty(project.IsGITDefault)) && Convert.ToString(project.IsGITDefault).ToUpper().Equals("Y"))
                {
                    if ((!string.IsNullOrEmpty(project.GITPath)))
                    {
                        //GIT Integration Code
                        try
                        {
                            var userProfile = _sessionHelper.GetSessionDetails(user).User;
                            var gitPath = project.GITPath;  //GIT PATH                    
                            var gitLogin = project.GITUsername;
                            var gitPass = project.GITPassword;
                            var diskPath = @"D:\GRS\SVN_NEW2\" + projName;
                            string username = userProfile.USerFullName;
                            string useremail = userProfile.UserEmail;
                            bool isUrlValid = false;
                            string CoverURL = gitPath.ToUpper();
                            string IsGitDefault = project.IsGITDefault;

                            string containsPart1 = "HTTPS://GIT.SAMI.INT.THOMSONREUTERS.COM/";
                            string containsPart2 = "HTTPS://GIT.CLARIVATE.IO/";
                            bool isFirstPartExist1 = CoverURL.Contains(containsPart1);
                            bool isFirstPartExist2 = CoverURL.Contains(containsPart2);
                            if (isFirstPartExist1 || isFirstPartExist2)
                            {
                                string lastPart = CoverURL.Substring((CoverURL.Length - 4));
                                if (lastPart.Equals(".GIT"))
                                {
                                    isUrlValid = true;
                                }
                                else
                                {
                                    returnmessage = "Invalid GIT URL. Please update your GIT URL and submit then Update test.";
                                }
                            }
                            else
                            {
                                returnmessage = "Invalid GIT URL. Please update your test project GIT URL and submit then Update test.";
                            }

                            if ((isFirstPartExist1 || isFirstPartExist2) && isUrlValid && (!string.IsNullOrEmpty(gitLogin)) && (!string.IsNullOrEmpty(gitPass)) && (!string.IsNullOrEmpty(gitPath)) && (!string.IsNullOrEmpty(username)) && (!string.IsNullOrEmpty(useremail)))
                            {
                                if (Directory.Exists(diskPath))
                                {
                                    DeleteDirectory(diskPath);
                                }
                                //if (!Directory.Exists(diskPath))
                                //{
                                CloneOptions getclone = new CloneOptions();
                                getclone.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitLogin, Password = gitPass };
                                Repository.Clone(gitPath, diskPath, getclone);
                                #region collapse commented
                                //}
                                //else
                                //{
                                //    if (!Repository.IsValid(diskPath))
                                //    {
                                //        DeleteDirectory(diskPath);
                                //        CloneOptions getclone = new CloneOptions();
                                //        getclone.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitLogin, Password = gitPass };
                                //        Repository.Clone(gitPath, diskPath, getclone);
                                //    }
                                //    else
                                //    {
                                //        bool isURLCorrect = false;
                                //        using (var repo = new Repository(diskPath))
                                //        {
                                //            string workingCopyUri = repo.Network.Remotes["origin"].Url;
                                //            if (new Uri(gitPath).AbsoluteUri.SequenceEqual(workingCopyUri))
                                //            {
                                //                repo.RemoveUntrackedFiles();
                                //                isURLCorrect = true;
                                //                PullOptions options = new PullOptions();
                                //                options.FetchOptions = new FetchOptions();
                                //                options.FetchOptions.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = gitLogin, Password = gitPass });
                                //                repo.Network.Pull(new LibGit2Sharp.Signature(username, useremail, new DateTimeOffset(DateTime.Now)), options);
                                //            }
                                //            else
                                //            {
                                //                repo.RemoveUntrackedFiles();
                                //                repo.Reset(ResetMode.Hard);
                                //                repo.Dispose();
                                //            }
                                //        }
                                //
                                //        if (isURLCorrect == false)
                                //        {
                                //            Thread.Sleep(2000);
                                //            DeleteDirectory(diskPath);
                                //
                                //            CloneOptions getclone = new CloneOptions();
                                //            getclone.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitLogin, Password = gitPass };
                                //            Repository.Clone(gitPath, diskPath, getclone);
                                //        }
                                //    }
                                //}

                                #endregion collapse commented

                                var model = new ExecutionTestSuiteModel { IsJava = File.Exists(diskPath + @"\pom.xml") };
                                if (!model.IsJava && Directory.Exists(diskPath + @"\" + projName))
                                    diskPath = diskPath + @"\" + projName;
                                model = !model.IsJava ? ExtractDotNet(diskPath, model) : ExtractJava(diskPath, model);

                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);
                                File.WriteAllText(path + @"\" + project.ProjectName + ".xml", model.Serialize());
                                returnmessage = projName + " updated from GIT repository.";
                            }
                            else
                            {
                                returnmessage = "Please check your git repository path, git username, git password, your username, your email address are correct.";
                            }
                        }
                        catch (Exception ex)
                        {
                            returnmessage = ex.Message;
                        }
                    }
                    else
                    {
                        returnmessage = "Please check your git repository path, git username, git password, your username, your email address are correct.";
                    }
                }
                else
                {
                    //SVN Integration Code
                    try
                    {
                        var svnPath = project.SVNpath;
                        var svnPass = project.SVNpassword;
                        var svnLogin = project.SVNlogin;
                        if ((!string.IsNullOrEmpty(svnPath)) && (!string.IsNullOrEmpty(svnLogin)) && (!string.IsNullOrEmpty(svnPass)))
                        {
                            var diskPath = @"D:\GRS\SVN_NEW2\" + projName;
                            svnPath = svnPath.EndsWith("/") ? svnPath : svnPath + "/";
                            using (var svnClient = new SvnClient())
                            {
                                svnClient.Authentication.SslServerTrustHandlers += new EventHandler<SharpSvn.Security.SvnSslServerTrustEventArgs>(Authentication_SslServerTrustHandlers);
                                svnClient.Authentication.DefaultCredentials = new NetworkCredential(svnLogin, svnPass);
                                svnClient.LoadConfiguration(Path.Combine(Path.GetTempPath(), "Svn"), true);
                                //var workingCopyUri = svnClient.GetUriFromWorkingCopy(diskPath);

                                if (Directory.Exists(diskPath))
                                {
                                    Thread.Sleep(2000);
                                    DeleteDirectory(diskPath);
                                }
                                svnClient.CheckOut(new SvnUriTarget(svnPath), diskPath);


                                #region collapse commented
                                //if (Directory.Exists(diskPath) && workingCopyUri != null && new Uri(svnPath).AbsoluteUri.SequenceEqual(workingCopyUri.AbsoluteUri))
                                //{
                                //    //For Future if required
                                //    //string folderPath = diskPath;
                                //    //string adminUserName = Environment.UserName;
                                //    //System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                                //    //System.Security.AccessControl.FileSystemAccessRule fsa = new System.Security.AccessControl.FileSystemAccessRule(adminUserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Deny);
                                //    //ds.RemoveAccessRule(fsa);
                                //    //Directory.SetAccessControl(folderPath, ds);

                                //    svnClient.Update(diskPath);
                                //}
                                //else if (Directory.Exists(diskPath) && workingCopyUri != null && !new Uri(svnPath).AbsoluteUri.SequenceEqual(workingCopyUri.AbsoluteUri))
                                //{
                                //    svnClient.CleanUp(diskPath);
                                //    Thread.Sleep(2000);
                                //    DeleteDirectory(diskPath);                                
                                //    svnClient.CheckOut(new SvnUriTarget(svnPath), diskPath);
                                //}
                                //else
                                //{
                                //    if (Directory.Exists(diskPath))
                                //    {
                                //        Thread.Sleep(2000);
                                //        DeleteDirectory(diskPath);
                                //    }
                                //    svnClient.CheckOut(new SvnUriTarget(svnPath), diskPath);
                                //}
                            }

                            //if (!SVNCheckoutUpdate(svnPath, svnPass, svnLogin, diskPath))
                            //    throw new Exception("Can't perform SVN sync.");

                                #endregion collapse commented

                            var model = new ExecutionTestSuiteModel { IsJava = File.Exists(diskPath + @"\pom.xml") };
                            if (!model.IsJava && Directory.Exists(diskPath + @"\" + projName))
                                diskPath = diskPath + @"\" + projName;
                            model = !model.IsJava ? ExtractDotNet(diskPath, model) : ExtractJava(diskPath, model);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            File.WriteAllText(path + @"\" + project.ProjectName + ".xml", model.Serialize());
                            returnmessage = projName + " updated from SVN repository.";
                        }
                        else
                        {
                            returnmessage = "Please check your svn repository path, svn username, svn password are correct.";
                        }
                    }
                    catch (Exception ex)
                    {
                        returnmessage = ex.Message;
                    }
                }

                //release project section  - isprojectupdating on server to false
                var releaseProjectRow = _projectRepository.GetSingleOrDefault(p => p.ID == projectId);
                releaseProjectRow.IsProjectUpdatingOnServer = false;
                _projectRepository.SaveChanges();
            }
            else
            {
                //return response message that it is updating by *** user. try after some time
                if (IsProjectUpdatingNow == true)
                {
                    returnmessage = "Project source is downloading to the server by " + _sessionHelper.GetSessionDetails(user).User.USerFullName + ". Please try after some time.";
                }
                if (project == null)
                {
                    returnmessage = "Project not defined";
                }
            }
            return returnmessage;
        }

        public void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                //Delete all files from the Directory
                foreach (string file in Directory.GetFiles(path))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                //Delete all child Directories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    DeleteDirectory(directory);
                }
                //Delete a Directory
                Directory.Delete(path);
            }
        }

        private static ExecutionTestSuiteModel ExtractJava(string diskPath, ExecutionTestSuiteModel model)
        {
            var files =
               Directory.GetFiles(diskPath, "*.feature", SearchOption.AllDirectories)
            .Where(p => File.ReadAllLines(p).Count(f => f.Trim().StartsWith("@")) > 0).Select(File.ReadAllText).ToList();
            foreach (var file in files)
            {
                var ts = new ExecutionTestSuite();
                var tags = file.Split('\n').Where(p => p.Trim().StartsWith("@")).ToList();
                var globalName = tags[0].Trim().Remove(0, 1).Split('@');
                ts.Name = globalName[0].Trim();

                if (globalName.Count() > 1)
                    ts.Tests.AddRange(globalName.Skip(1).Select(p => p.Trim()));

                foreach (var tst in tags.Skip(1))
                {
                    ts.Tests.AddRange(tst.Trim().Remove(0, 1).Split('@').Select(p => p.Trim()));
                }
                ts.Tests = ts.Tests.Distinct().ToList();
                model.ExecutionTestSuites.Add(ts);
            }
            return model;
        }

        //private static ExecutionTestSuiteModel ExtractJava(string diskPath, ExecutionTestSuiteModel model)
        //{
        //    //var files =
        //    //   Directory.GetFiles(diskPath, "*.feature", SearchOption.AllDirectories)
        //    //            .Where(p => File.ReadAllText(p).Contains("@")).Select(File.ReadAllText).ToList();
        //    var files =
        //       Directory.GetFiles(diskPath, "*.feature", SearchOption.AllDirectories)
        //    .Where(p => File.ReadAllLines(p).Count(f => f.StartsWith("@")) > 0).Select(File.ReadAllText).ToList();
        //    foreach (var file in files)
        //    {
        //        var ts = new ExecutionTestSuite();
        //        //var tags = file.Split('@').Skip(1).ToList(); 
        //        var tags = file.Replace(" ", "").Replace("\t", "").Split('\n').Where(p => p.StartsWith("@")).ToList();
        //        ts.Name = tags[0].Remove(0, 1).Split(new char[] { '\r', '\n' })[0].Trim().Split('@')[0];
        //        //foreach (var tst in tags.Skip(1))
        //        //{
        //        //    ts.Tests.Add(tst.Split(new char[] { '\r', '\n' })[0].Trim());
        //        //}
        //        foreach (var tst in tags.Skip(1))
        //        {
        //            ts.Tests.AddRange(tst.Remove(0, 1).Split(new char[] { '\r', '\n' })[0].Trim().Split('@'));
        //        }
        //        ts.Tests = ts.Tests.Distinct().ToList();
        //        model.ExecutionTestSuites.Add(ts);
        //    }
        //    return model;
        //}

        private static ExecutionTestSuiteModel ExtractDotNet(string diskPath, ExecutionTestSuiteModel model)
        {
            var files =
                Directory.GetFiles(diskPath, "*.cs", SearchOption.AllDirectories)
                         .Where(p => File.ReadAllText(p).Contains("[Test]")).Select(File.ReadAllText).ToList();

            foreach (var file in files)
            {
                var ts = new ExecutionTestSuite();

                var ns = Regex.Split(file, "namespace ");
                ts.Namespace = ns[1].Split(new char[] { '\r', '\n' })[0];

                var name = Regex.Split(file, "class ");
                ts.Name = name[1].Split(new char[] { '\r', '\n' })[0].Split(':')[0].Trim();
                var tsts = Regex.Split(file, "public void ").Skip(1);
                foreach (var tst in tsts.Where(p => !p.Contains("[SetUp]") && !p.Contains("[TearDown]")))
                {
                    ts.Tests.Add(tst.Split('(')[0].Replace(" : Test", ""));
                }
                var cats = file.Split(new string[] { " [Category(" }, StringSplitOptions.None).Skip(1);
                foreach (var cat in cats)
                {
                    ts.Categories.Add(cat.Split(')')[0].Replace("\"", ""));
                }
                ts.Categories = ts.Categories.Distinct().ToList();
                ts.Categories = parseCategories(diskPath, ts.Categories);
                model.ExecutionTestSuites.Add(ts);
            }
            return model;
        }

        private static List<string> parseCategories(string diskPath, List<string> categories)
        {
            var finalCategories = new List<string>();
            var categoriesToParse = categories.Where(p => p.Contains(".")).ToList();
            finalCategories.AddRange(categories.Where(p => !p.Contains(".")));
            if (categoriesToParse.Count > 0)
            {
                var classes = new List<string>();
                categoriesToParse.ForEach(p => classes.Add(p.Split('.')[0]));
                classes = classes.Distinct().ToList();

                var files =
                    Directory.GetFiles(diskPath, "*.cs", SearchOption.AllDirectories)
                             .Where(p => File.ReadAllText(p).Contains("public static class ")).Select(File.ReadAllText).ToList();

                foreach (var cs in classes)
                {
                    string csfile = "";
                    try
                    {
                        csfile = files.Where(p => p.Contains("public static class " + cs)).ToList()[0].Split(new string[] { "public static class " + cs }, StringSplitOptions.None)[1];
                    }
                    catch { }
                    if (String.IsNullOrEmpty(csfile))
                    {
                        continue;
                    }
                    var consts = new List<string>();
                    categoriesToParse.Where(p => p.StartsWith(cs)).ForEach(p => consts.Add(p.Split('.')[1]));
                    foreach (var constant in consts)
                    {
                        //public const string
                        string cat = String.Empty;
                        try
                        {
                            cat = csfile.Split(new string[] { "public const string " + constant + " = \"" }, StringSplitOptions.None)[1].Split(new string[] { "\";" }, StringSplitOptions.None)[0];
                        }
                        catch { }
                        if (!String.IsNullOrEmpty(cat))
                        {
                            finalCategories.Add(cat);
                        }
                        else
                        {
                            finalCategories.Add(cs + "." + constant);
                        }

                    }

                }
            }
            return finalCategories;
        }

        public void SetUpdatedConfiguration(IPrincipal user, ProjectConfigurationModel model)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            project.ProjectName = model.ProjectId;
            project.DisplayName = model.DisplayName;
            project.QCServer = model.QcLocation;
            project.QcResultsPath = model.QcResultsPath;
            project.QCLocation = model.QcDomProj;
            project.QCTestPlan = model.QcTestPlanPath;
            project.IsGITDefault = model.IsGITDefaultAccount == true ? "Y" : "N";
            if (model.IsGITDefaultAccount == false)
            {
                project.SVNpath = model.SvnRepo;
                project.SVNlogin = model.SvnUser;
                project.SVNpassword = model.SvnPassword;
            }
            else if (model.IsGITDefaultAccount == true)
            {
                project.GITPath = model.GITPath;

                string gpath = model.GITPath.ToUpper();
                if (gpath.Contains("HTTPS://GIT.SAMI.INT.THOMSONREUTERS.COM"))
                {
                    project.GITUsername = model.GITUser;
                    project.GITPassword = model.GITPassword;
                }
                else if (gpath.Contains("HTTPS://GIT.CLARIVATE.IO"))
                {
                    project.GITUsername = model.GITUser;
                    if ((!string.IsNullOrEmpty(model.GITPassword)) && (!string.IsNullOrEmpty(model.GITUser)))
                    {
                        project.GITPassword = model.GITPassword;
                    }
                }
                else
                {
                    project.GITUsername = model.GITUser;
                    project.GITPassword = model.GITPassword;
                }
            }
            project.AssignedBugTracker = model.CurrentBugTracker;
            project.Notification_ResultsDelivery = model.EmailNotification;
            _sessionHelper.SaveChanges();
        }

        public ProjectConfigurationModel GetProjectConfiguration(IPrincipal user)
        {
            var userdetails = _sessionHelper.GetSessionDetails(user).User;
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var project = _projectRepository.GetFirstOrDefault(p => p.ID == pId);
            var retModel = new ProjectConfigurationModel
            {
                Id = project.ID,
                CurrentBugTracker = project.Bugracker != null ? project.Bugracker.ID : -1,
                DisplayName = project.DisplayName,
                ProjectId = project.ProjectName,
                QcDomProj = project.QCLocation,
                QcLocation = project.QCServer,
                QcResultsPath = project.QcResultsPath,
                QcTestPlanPath = project.QCTestPlan,
                SvnPassword = project.SVNpassword,
                SvnRepo = project.SVNpath,
                SvnUser = project.SVNlogin,
                GITPath = project.GITPath,
                GITUser = project.GITUsername,
                GITPassword = project.GITPassword,
                IsGITDefaultAccount = project.IsGITDefault == "Y" ? true : false,
                EmailNotification = project.Notification_ResultsDelivery == null ? false : (bool)project.Notification_ResultsDelivery,
                Bugtrackers = _bugtackerObjectRepository.GetAllToList().Select(p => new IssueTracker
                {
                    Id = p.ID,
                    Name = p.Name
                }).ToList()
            };
            return retModel;

        }

        private bool CheckIfSVNValuesAreValid(string login, string password, string path)
        {
            bool isValid = false;
            if (!String.IsNullOrWhiteSpace(login) && !String.IsNullOrWhiteSpace(password) && !String.IsNullOrWhiteSpace(path))
            {
                NetworkCredential myCred = new NetworkCredential(login, password);
                CredentialCache myCache = new CredentialCache();
                myCache.Add(new Uri(path), "Basic", myCred);
                WebRequest wr = WebRequest.Create(path);
                wr.Credentials = myCache;
                wr.Method = WebRequestMethods.Http.Get;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                    if (response != null)
                    {
                        isValid = true;
                    }
                }
                catch (WebException ex)
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        private static void LogMessage(string message)
        {
            using (var writer = File.AppendText(@"D:\GRS\log.txt"))
            {
                writer.WriteLine(message + " - " + DateTime.Now);
            }
        }

        public string userEmail { get; set; }

        //---------------SUNDRAM--------------------------------------------

        public bool CloneProjectForFeaturesAndTagsFromGIT(string LocalDiskPath, bool IsGITDefault, string SVNPath, string SVNLogin, string SVNPassword, string GITPath, string GITLogin, string GITPassword, string ProjectName)
        {
            if (IsGITDefault == true)
            {
                if ((!string.IsNullOrEmpty(GITPath)))
                {
                    try
                    {
                        var gitPath = GITPath;
                        var gitLogin = GITLogin;
                        var gitPass = GITPassword;
                        var diskPath = LocalDiskPath;
                        bool isUrlValid = false;
                        string CoverURL = gitPath.ToUpper();
                        string containsPart1 = "HTTPS://GIT.SAMI.INT.THOMSONREUTERS.COM/";
                        string containsPart2 = "HTTPS://GIT.CLARIVATE.IO/";
                        bool isFirstPartExist1 = CoverURL.Contains(containsPart1);
                        bool isFirstPartExist2 = CoverURL.Contains(containsPart2);
                        if (isFirstPartExist1 || isFirstPartExist2)
                        {
                            string lastPart = CoverURL.Substring((CoverURL.Length - 4));
                            if (lastPart.Equals(".GIT"))
                            {
                                isUrlValid = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                        if ((isFirstPartExist1 || isFirstPartExist2) && isUrlValid && (!string.IsNullOrEmpty(gitLogin)) && (!string.IsNullOrEmpty(gitPass)) && (!string.IsNullOrEmpty(gitPath)))
                        {
                            if (Directory.Exists(diskPath)) { DeleteDirectory(diskPath); }
                            CloneOptions getclone = new CloneOptions();
                            getclone.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitLogin, Password = gitPass };
                            Repository.Clone(gitPath, diskPath, getclone);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else
            {
                try
                {
                    var svnPath = SVNPath;
                    var svnPass = SVNPassword;
                    var svnLogin = SVNLogin;
                    if ((!string.IsNullOrEmpty(svnPath)) && (!string.IsNullOrEmpty(svnLogin)) && (!string.IsNullOrEmpty(svnPass)))
                    {
                        var diskPath = LocalDiskPath;
                        svnPath = svnPath.EndsWith("/") ? svnPath : svnPath + "/";
                        using (var svnClient = new SvnClient())
                        {
                            svnClient.Authentication.SslServerTrustHandlers += new EventHandler<SharpSvn.Security.SvnSslServerTrustEventArgs>(Authentication_SslServerTrustHandlers);
                            svnClient.Authentication.DefaultCredentials = new NetworkCredential(svnLogin, svnPass);
                            svnClient.LoadConfiguration(Path.Combine(Path.GetTempPath(), "Svn"), true);
                            if (Directory.Exists(diskPath)) { Thread.Sleep(2000); DeleteDirectory(diskPath); }
                            svnClient.CheckOut(new SvnUriTarget(svnPath), diskPath);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public Project GetProjectDataById(int ProjectId)
        {
            return _projectRepository.GetSingleOrDefault(p => p.ID == ProjectId);
        }

        public string GetUserNameById(int UserId)
        {
            return _userRepository.GetSingleOrDefault(U => U.ID == UserId).USerFullName;
        }

        public void LockProjectForFeatureTagUpdate(int ProjectId, int UserId)
        {
            var projectData = _projectRepository.GetSingleOrDefault(P => P.ID == ProjectId);
            if (projectData != null)
            {
                projectData.IsTestCaseUpdating = true;
                projectData.TestCaseUpdatedBy = UserId;
                _projectRepository.SaveChanges();
            }
        }

        public void ReleaseProjectForFeatureTagUpdate(int ProjectId, int UserId)
        {
            var projectData = _projectRepository.GetSingleOrDefault(P => P.ID == ProjectId);
            if (projectData != null)
            {
                projectData.IsTestCaseUpdating = false;
                projectData.TestCaseUpdatedBy = 0;
                _projectRepository.SaveChanges();
            }
        }

        //=============================================

        public string DeleteOldRecordByAdmin(IPrincipal currentuser, int projectId, DateTime beforeDate)
        {
            try
            {
                int userId = _sessionHelper.GetSessionDetails(currentuser).User.ID;
                // verify user type
                var userData = _userRepository.GetSingleOrDefault(u => u.ID == userId);
                if (userData.UserGlobalAdmin == true)
                {
                    DateTime castBeforeDateTime = new DateTime(beforeDate.Year, beforeDate.Month, beforeDate.Day, 0, 0, 0);
                    List<string> attachmentToDelete = new List<string>();
                    List<string> ScreenShotToDelete = new List<string>();

                    //Get all the project
                    var projdata = _projectRepository.GetSingleOrDefault(p => p.ID == projectId);
                    //Get All TestCycle created before date of specific project
                    var testCycleList = _testCycle.GetAllToList(TC => TC.ParentProject == projectId && TC.CycleStart < castBeforeDateTime);
                    foreach (var tCycle in testCycleList)
                    {
                        //Get All Test suit
                        var testSuitsList = _testSuitRepository.GetAllToList(TS => TS.ParentProject == projectId && TS.ParentTestCycle == tCycle.ID && TS.TSStart < castBeforeDateTime);
                        foreach (var tSuit in testSuitsList)
                        {
                            //Get All TestCase
                            var testCaseList = _testCase.GetAllToList(TCa => TCa.ParentTestSuite == tSuit.ID);
                            foreach (var tCase in testCaseList)
                            {
                                //Get All TestStep
                                var testStepList = _testStep.GetAllToList(TSt => TSt.ParentTestCase == tCase.ID);
                                foreach (var tStep in testStepList)
                                {
                                    //Get all subsetp
                                    var subStepList = _subStepRepository.GetAllToList(SS => SS.ParentStep == tStep.ID);
                                    foreach (var sStep in subStepList)
                                    {
                                        // add screen shot file name in list
                                        if (!string.IsNullOrEmpty(sStep.SubStepScreenShot)) { ScreenShotToDelete.Add(sStep.SubStepScreenShot); }
                                        // delete this sub step
                                        var substepdel = _subStepRepository.GetSingleOrDefault(d => d.ID == sStep.ID);
                                        _subStepRepository.Delete(substepdel);
                                        _subStepRepository.SaveChanges();
                                    }
                                    //Add attachment file name in List
                                    if (!string.IsNullOrEmpty(tStep.Attachments)) { attachmentToDelete.Add(tStep.Attachments); }
                                    //Add screenshot to list
                                    if (!string.IsNullOrEmpty(tStep.StepScreenshot)) { ScreenShotToDelete.Add(tStep.StepScreenshot); }
                                    if (!string.IsNullOrEmpty(tStep.StepScreenshotDriver)) { ScreenShotToDelete.Add(tStep.StepScreenshotDriver); }
                                    //delete this test step
                                    var teststepdel = _testStep.GetSingleOrDefault(d => d.ID == tStep.ID);
                                    _testStep.Delete(teststepdel);
                                    _testStep.SaveChanges();
                                }
                                //add attachment file to delete list
                                if (!string.IsNullOrEmpty(tCase.TCAttachments)) { attachmentToDelete.Add(tCase.TCAttachments); }
                                // delete this test case
                                var testcasedel = _testCase.GetSingleOrDefault(d => d.ID == tCase.ID);
                                _testCase.Delete(testcasedel);
                                _testCase.SaveChanges();
                            }
                            //Delete Analysers
                            var analysersData = _analyser.GetAllToList(A => A.TestSet == tSuit.ID);
                            foreach (var anlritem in analysersData)
                            {
                                var deleteAnalyser = _analyser.GetSingleOrDefault(a => a.ID == anlritem.ID);
                                _analyser.Delete(deleteAnalyser);
                                _analyser.SaveChanges();
                            }
                            int clientinformationid = tSuit.ParentClientInfo ?? 0;
                            // delete this test suit
                            var testsuitdel = _testSuitRepository.GetSingleOrDefault(d => d.ID == tSuit.ID);
                            _testSuitRepository.Delete(testsuitdel);
                            _testSuitRepository.SaveChanges();
                            //delete ClientInformation
                            if (clientinformationid > 0)
                            {
                                Int64 isclientinfoidexist = _testSuitRepository.Count(c => c.ParentClientInfo.HasValue && c.ParentClientInfo == clientinformationid);
                                if (isclientinfoidexist <= 0)
                                {
                                    var parentclientinfodel = _clientsInformation.GetSingleOrDefault(d => d.ID == clientinformationid);
                                    _clientsInformation.Delete(parentclientinfodel);
                                    _clientsInformation.SaveChanges();
                                }
                            }
                        }
                    }

                    // Delete attachment file
                    //string ATTACHMENTPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Attachments\" + projectId + "\\";
                    string ATTACHMENTPATH = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Attachments\" + projectId + "\\";
                    foreach (var atfile in attachmentToDelete)
                    {
                        if (File.Exists(ATTACHMENTPATH + atfile))
                        {
                            File.Delete(ATTACHMENTPATH + atfile);
                        }
                    }

                    // Delete ScreenShots
                    //string SCREENSHOTPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Screenshots\";
                    string SCREENSHOTPATH = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Screenshots\";
                    foreach (var scsfile in ScreenShotToDelete)
                    {
                        if (File.Exists(SCREENSHOTPATH + scsfile))
                        {
                            File.Delete(SCREENSHOTPATH + scsfile);
                        }
                    }

                    // Delete Temp Files in harvaest
                    //string HARVESTTEMPPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Temp\";
                    //DirectoryInfo dirHarvestTempFiles = new DirectoryInfo(HARVESTTEMPPATH);
                    //FileInfo[] filesHarvestTemp = dirHarvestTempFiles.GetFiles().OrderByDescending(p => p.CreationTime).Where(c => c.CreationTime < castBeforeDateTime).ToArray();
                    //foreach (var filetemp in filesHarvestTemp)
                    //{
                    //    filetemp.Delete();
                    //}

                    // Delete Charts
                    //string CHARTPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Charts\";
                    //DirectoryInfo dirHarvestCharts = new DirectoryInfo(CHARTPATH);
                    //FileInfo[] filesHarvestCharts = dirHarvestCharts.GetFiles().OrderByDescending(p => p.CreationTime).Where(c => c.CreationTime < castBeforeDateTime).ToArray();
                    //foreach (var filechart in filesHarvestCharts)
                    //{
                    //    filechart.Delete();
                    //}

                    // Delete Temp File in UI
                    //string UITEMPPATH = @"E:\Jenkins\workspace\GRSNG\GlobalReportingSystem.Web.UI\Temp\";
                    //DirectoryInfo dirUiTemp = new DirectoryInfo(UITEMPPATH);
                    //FileInfo[] filesUiTemp = dirUiTemp.GetFiles().OrderByDescending(p => p.CreationTime).Where(c => c.CreationTime < castBeforeDateTime).ToArray();
                    //foreach (var fileuitemp in filesUiTemp)
                    //{
                    //    fileuitemp.Attributes = FileAttributes.ReadOnly;
                    //    fileuitemp.Delete();
                    //}

                    return "Older record (before the date " + castBeforeDateTime.ToString("dd/MM/yyyy") + ") deleted from the project '" + projdata.DisplayName + "' successfully.";
                }
                else
                {
                    return "You are not authorized to delete record. Please contact to global administrator.";
                }
            }
            catch (Exception ex)
            {
                return "Exception '" + ex.Message + "' occured while deleting the record. Please try aftersome time.";
            }
        }

        public UserListModel GetUserListData(IPrincipal currentuser)
        {
            int UserID = _sessionHelper.GetSessionDetails(currentuser).User.ID;

            var requestfromUser = _userRepository.GetSingleOrDefault(u => u.ID == UserID);
            if (requestfromUser != null)
            {
                if (requestfromUser.UserGlobalAdmin)
                {
                    var userlist = _userRepository.GetAllToList();
                    var finaluserlist = (from U in userlist
                                         select new UserModelAdminAccess
                                         {
                                             ID = U.ID,
                                             UserName = U.UserName,
                                             UserFullName = U.USerFullName,
                                             UserEmail = U.UserEmail,
                                             UserAdmin = U.UserAdmin,
                                             UserGlobalAdmin = U.UserGlobalAdmin,
                                             UserBlocked = U.UserBlocked == true ? true : false
                                         }).ToList();
                    return new UserListModel()
                    {
                        ErrorId = 0,
                        ErrorMessage = "",
                        UserModelAdminAccess = finaluserlist
                    };
                }
                else
                {
                    return new UserListModel()
                    {
                        ErrorId = 601,
                        ErrorMessage = "You are not authorize to grant Global Administrator access to other user.",
                        UserModelAdminAccess = new List<UserModelAdminAccess>()
                    };
                }
            }
            else
            {
                return new UserListModel()
                {
                    ErrorId = 601,
                    ErrorMessage = "You are not authorize to grant Global Administrator access to other user.",
                    UserModelAdminAccess = new List<UserModelAdminAccess>()
                };
            }
        }

        //Assign User Admin right 
        public string AssignUserAdminRight(IPrincipal assineeUser, int userToAssign)
        {
            try
            {
                //assignee must have global admin right
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isadmin = (adminUser.User.UserGlobalAdmin) ? true : false;
                if (isadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToAssign);
                    if (userdata != null)
                    {
                        if (!userdata.UserAdmin)
                        {
                            userdata.UserAdmin = true;
                            _userRepository.SaveChanges();
                            return "User Admin role granted successfully.";
                        }
                        else
                        {
                            return "User is already marked with User Admin.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to assign USER ADMIN rol. Please contact to your Project Admin or Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured. '" + ex.Message + "'";
            }
        }

        //Unassign User Admin right
        public string UnAssignUserAdminRight(IPrincipal assineeUser, int userToUnassign)
        {
            try
            {
                //assignee must have admin right or global admin right
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isadmin = (adminUser.User.UserGlobalAdmin) ? true : false;
                if (isadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToUnassign);
                    if (userdata != null)
                    {
                        if (userdata.UserAdmin)
                        {
                            userdata.UserAdmin = false;
                            _userRepository.SaveChanges();
                            return "User Admin role unassigned successfully.";
                        }
                        else
                        {
                            return "User is not marked with User Admin.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to assign USER ADMIN rol. Please contact to your Project Admin or Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured. '" + ex.Message + "'";
            }
        }

        //Assign Global Admin right 
        public string AssignGlobalAdminRight(IPrincipal assineeUser, int userToAssign)
        {
            //assignee must have global admin right
            try
            {
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isglobadmin = adminUser.User.UserGlobalAdmin ? true : false;
                if (isglobadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToAssign);
                    if (userdata != null)
                    {
                        if (!userdata.UserGlobalAdmin)
                        {
                            userdata.UserGlobalAdmin = true;
                            _userRepository.SaveChanges();

                            var accesslist = _accessRepository.GetAllToList(a => a.AttachedUser == userdata.ID);
                            var projectlist = _projectRepository.GetAllToList();
                            foreach (var pitem in projectlist)
                            {
                                bool isUserAlreadyHasAceess = (from al in accesslist
                                                               where al.AttachedUser == userdata.ID
                                                               && al.AttachedProject == pitem.ID
                                                               select al).Any();
                                if (!isUserAlreadyHasAceess)
                                {
                                    _accessRepository.Add(
                                        new Access()
                                        {
                                            AttachedProject = pitem.ID,
                                            AttachedUser = userdata.ID,
                                            DeliveryResult = true
                                        });
                                    _accessRepository.SaveChanges();
                                }
                            }

                            try
                            {
                                //send email about global admin access to the user
                                string ToAddress = userdata.UserEmail;
                                string[] toaddressarray = ToAddress.Split(',');
                                List<string> ToAddressList = new List<string>(toaddressarray);
                                string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi {USERNAME},<br /><br />You have been granted <b>Global Administration Access</b> to the Global Reporting System.<br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                                messagebody = messagebody.Replace("{USERNAME}", userdata.USerFullName);
                                messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                                _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Global Administration Access granted", messagebody);
                            }
                            catch { }
                            return "User Global Admin role assigned successfully.";
                        }
                        else
                        {
                            return "User is already marked with User Global Admin.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to assign USER GLOBAL ADMIN rol. Please contact to Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured. '" + ex.Message + "'";
            }

        }

        //Unassigned admin right to user
        public string UnAssignGlobalAdminRight(IPrincipal assineeUser, int userToUnassign)
        {
            //assignee must have global admin right
            try
            {
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isglobadmin = adminUser.User.UserGlobalAdmin ? true : false;
                if (isglobadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToUnassign);
                    if (userdata != null)
                    {
                        if (userdata.UserGlobalAdmin)
                        {
                            userdata.UserGlobalAdmin = false;
                            _userRepository.SaveChanges();

                            //Unassigned from user access and will have no access for project
                            var allAccessList = _accessRepository.GetAllToList(a => a.AttachedUser == userdata.ID);
                            foreach (var aitem in allAccessList)
                            {
                                var removeaccess = _accessRepository.GetSingleOrDefault(a => a.ID == aitem.ID);
                                _accessRepository.Delete(removeaccess);
                                _accessRepository.SaveChanges();
                            }

                            try
                            {
                                //Send email about removed form global admin 
                                string ToAddress = userdata.UserEmail;
                                string[] toaddressarray = ToAddress.Split(',');
                                List<string> ToAddressList = new List<string>(toaddressarray);
                                string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi {USERNAME},<br /><br />You have been unassigned from <b>Global Administration Access</b> to the Global Reporting System.<br /><br />Now, You do not have access for any project.<br /><br />To get the project access, Please contact to Administrator.<br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                                messagebody = messagebody.Replace("{USERNAME}", userdata.USerFullName);
                                messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                                _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Unassigned from Global Administration", messagebody);
                            }
                            catch
                            { }
                            return "User Global Admin role unassigned successfully.";
                        }
                        else
                        {
                            return "User is not marked with User Global Admin.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to unassign USER GLOBAL ADMIN rol. Please contact to Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured. '" + ex.Message + "'";
            }
        }

        public string UnBlockUser(IPrincipal assineeUser, int userToUnassign)
        {
            //assignee must have global admin right
            try
            {
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isglobadmin = adminUser.User.UserGlobalAdmin ? true : false;
                if (isglobadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToUnassign);
                    if (userdata != null)
                    {
                        bool isuserblocked = userdata.UserBlocked == true ? true : false;
                        if (isuserblocked)
                        {
                            userdata.UserBlocked = false;
                            _userRepository.SaveChanges();
                            return "User unblocked successfully.";
                        }
                        else
                        {
                            return "User is not blocked.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to unblock user. Please contact to Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured! '" + ex.Message + "'";
            }
        }

        //BlockUser
        public string BlockUser(IPrincipal assineeUser, int userToUnassign)
        {
            //assignee must have global admin right
            try
            {
                var adminUser = _sessionHelper.GetSessionDetails(assineeUser);
                bool isglobadmin = adminUser.User.UserGlobalAdmin ? true : false;
                if (isglobadmin)
                {
                    var userdata = _userRepository.GetSingleOrDefault(u => u.ID == userToUnassign);
                    if (userdata != null)
                    {
                        bool isuserblocked = userdata.UserBlocked == true ? true : false;
                        if (!isuserblocked)
                        {
                            userdata.UserBlocked = true;
                            _userRepository.SaveChanges();
                            return "User blocked successfully.";
                        }
                        else
                        {
                            return "User is not blocked.";
                        }
                    }
                    else
                    {
                        return "User not found.";
                    }
                }
                else
                {
                    return "You do not have previllage to unblock user. Please contact to Global Admin.";
                }
            }
            catch (Exception ex)
            {
                return "Exception occured! '" + ex.Message + "'";
            }
        }

        public bool CheckIsAdminUsingPage(IPrincipal currentUser)
        {
            try
            {
                var cSession = _sessionHelper.GetSessionDetails(currentUser);
                if (cSession != null)
                {
                    return cSession.User.UserGlobalAdmin == true ? true : false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<ProjectListModel> GetProjectListForDeleteOldRecord()
        {
            var plist = _projectRepository.GetAllToList();
            return (from x in plist
                    orderby x.DisplayName ascending
                    select new ProjectListModel
                    {
                        ID = x.ID,
                        DisplayName = x.DisplayName
                    }).ToList();
        }

        public ErrorControl AddNewUser(IPrincipal currentUser, string UserName, string UserFullName, string UserEmail, bool IsUserAdmin, bool IsUserGlobalAdmin, int ProjectID)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(currentUser);
            if (sessiondata.User.UserGlobalAdmin)
            {
                if ((!string.IsNullOrEmpty(UserName)) && (!string.IsNullOrEmpty(UserFullName)) && (!string.IsNullOrEmpty(UserEmail)) && (ProjectID > 0))
                {
                    Int64 usernamecount = _userRepository.Count(u => u.UserName == UserName);
                    if (usernamecount > 0)
                    {
                        return new ErrorControl() { ErrorId = 603, ErrorMessage = "Username already exist." };
                    }
                    else
                    {
                        Int64 useremailcount = _userRepository.Count(u => u.UserEmail == UserEmail);
                        if (useremailcount > 0)
                        {
                            return new ErrorControl() { ErrorId = 604, ErrorMessage = "User Email Address already exist." };
                        }
                        else
                        {
                            try
                            {
                                User addNewUser = new User()
                                {
                                    UserName = UserName,
                                    UserPassword = RandomString(),
                                    USerFullName = UserFullName,
                                    UserEmail = UserEmail,
                                    UserAdmin = IsUserAdmin ? true : false,
                                    UserGlobalAdmin = IsUserGlobalAdmin ? true : false
                                };
                                _userRepository.Add(addNewUser);
                                _userRepository.SaveChanges();
                                int newuserid = addNewUser.ID;

                                try
                                {
                                    string ToAddress = UserEmail;
                                    string[] toaddressarray = ToAddress.Split(',');
                                    List<string> ToAddressList = new List<string>(toaddressarray);
                                    string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi {USERNAME},<br /><br />Your account have been created on Global Reporting System. <br /><br />Please find your credential below.<br />Username: {LOGINUSERNAME}<br />Password: {PASSWORD}<br /><br />Note: If project is not assign to you, Please contact Global Administrator.<br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                                    messagebody = messagebody.Replace("{USERNAME}", addNewUser.USerFullName);
                                    messagebody = messagebody.Replace("{LOGINUSERNAME}", addNewUser.UserName);
                                    messagebody = messagebody.Replace("{PASSWORD}", addNewUser.UserPassword);
                                    messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                                    _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Global Administration Access granted", messagebody);
                                }
                                catch
                                {
                                }

                                if (newuserid > 0)
                                {
                                    if (addNewUser.UserGlobalAdmin)
                                    {
                                        var projectList = _projectRepository.GetAllToList().Select(s => s.ID);
                                        foreach (var item in projectList)
                                        {
                                            Access addNewAccess = new Access()
                                            {
                                                AttachedUser = newuserid,
                                                AttachedProject = item,
                                                DeliveryResult = true
                                            };
                                            _accessRepository.Add(addNewAccess);
                                            _accessRepository.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        Access addNewAccess = new Access()
                                        {
                                            AttachedUser = newuserid,
                                            AttachedProject = ProjectID,
                                            DeliveryResult = true
                                        };
                                        _accessRepository.Add(addNewAccess);
                                        _accessRepository.SaveChanges();
                                    }
                                    return new ErrorControl() { ErrorId = 0, ErrorMessage = "New user created successfully. Password has been sent to registered email address." };
                                }
                                else
                                {
                                    return new ErrorControl() { ErrorId = 0, ErrorMessage = "New User created successfully, but project assignment got failed. Please assign project to this user manully. Password has been sent to registered email address." };
                                }
                            }
                            catch (Exception ex)
                            {
                                return new ErrorControl() { ErrorId = 606, ErrorMessage = ex.Message };
                            }

                        }
                    }
                }
                else
                {
                    return new ErrorControl() { ErrorId = 605, ErrorMessage = "Validation Error" };
                }
            }
            else
            {
                return new ErrorControl() { ErrorId = 608, ErrorMessage = "You do not have permission to create new user." };
            }
        }

        private string RandomString()
        {
            Random random = new Random();
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < 8; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public ErrorControl AddNewProject(IPrincipal currentUser, AdminToolAddNewProject newProjectData)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(currentUser);
            if (sessiondata.User.UserGlobalAdmin)
            {
                try
                {
                    Int64 countProjectName = _projectRepository.Count(p => p.ProjectName == newProjectData.ProjectName);
                    if (countProjectName == 0)
                    {
                        Int64 displayNamecount = _projectRepository.Count(p => p.DisplayName == newProjectData.DisplayName);
                        if (displayNamecount == 0)
                        {
                            Project newProjectEnt = new Project();
                            newProjectEnt.ProjectName = newProjectData.ProjectName;
                            newProjectEnt.DisplayName = newProjectData.DisplayName;
                            newProjectEnt.ProjectTypeID = newProjectData.ProjectTypeID;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.QCLocation)))
                                newProjectEnt.QCLocation = newProjectData.QCLocation;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.QCServer)))
                                newProjectEnt.QCServer = newProjectData.QCServer;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.QcResultsPath)))
                                newProjectEnt.QcResultsPath = newProjectData.QcResultsPath;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.QCTestPlan)))
                                newProjectEnt.QCTestPlan = newProjectData.QCTestPlan;
                            //if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.AssignedBugTracker)))
                            //{
                            //    int bugTr = 0;
                            //    bool isconv = Int32.TryParse(Convert.ToString(newProjectData.AssignedBugTracker), out bugTr);
                            //    if (isconv)
                            //    {
                            //        newProjectEnt.AssignedBugTracker = bugTr;
                            //    }
                            //}
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.CustomStatuses)))
                                newProjectEnt.CustomStatuses = newProjectData.CustomStatuses;
                            newProjectEnt.Notification_ResultsDelivery = newProjectData.Notification_ResultsDelivery;
                            newProjectEnt.AutoAnalysis = newProjectData.AutoAnalysis;
                            newProjectEnt.isPublic = newProjectData.isPublic;
                            newProjectEnt.isGUI = newProjectData.isGUI;
                            newProjectEnt.IsGITDefault = newProjectData.IsGITDefault ? "Y" : "N";
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.SVNpath)))
                                newProjectEnt.SVNpath = newProjectData.SVNpath;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.SVNlogin)))
                                newProjectEnt.SVNlogin = newProjectData.SVNlogin;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.SVNpassword)))
                                newProjectEnt.SVNpassword = newProjectData.SVNpassword;
                            if (!string.IsNullOrEmpty(Convert.ToString(newProjectData.GITPath)))
                                newProjectEnt.GITPath = newProjectData.GITPath;
                            newProjectEnt.GITUsername = newProjectData.GITUSER;
                            newProjectEnt.GITPassword = newProjectData.GITPASSWORD;
                            _projectRepository.Add(newProjectEnt);
                            _projectRepository.SaveChanges();

                            int newprojid = newProjectEnt.ID;

                            if (newprojid > 0)
                            {
                                var AllAdminUser = _userRepository.GetAllToList(u => u.UserGlobalAdmin == true);
                                foreach (var uitem in AllAdminUser)
                                {
                                    Access newaccess = new Access()
                                    {
                                        AttachedUser = uitem.ID,
                                        AttachedProject = newprojid,
                                        DeliveryResult = true
                                    };
                                    _accessRepository.Add(newaccess);
                                    _accessRepository.SaveChanges();
                                }
                                return new ErrorControl() { ErrorId = 0, ErrorMessage = "Project created successfully. And assign to this project to the User Global Admin." };
                            }
                            else
                            {
                                return new ErrorControl() { ErrorId = 0, ErrorMessage = "Project created successfully. New project id not assigned to the User Global Admin. Please assign manually." };
                            }
                        }
                        else
                        {
                            return new ErrorControl() { ErrorId = 503, ErrorMessage = "Display Name already exist." };
                        }
                    }
                    else
                    {
                        return new ErrorControl() { ErrorId = 502, ErrorMessage = "Project name already exist." };
                    }
                }
                catch (Exception ex)
                {
                    return new ErrorControl() { ErrorId = 501, ErrorMessage = ex.Message };
                }
            }
            else
            {
                return new ErrorControl() { ErrorId = 504, ErrorMessage = "You do not have permission to create new project." };
            }
        }

        public string DeleteHarvestTempUploadedFile(IPrincipal currentUser)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(currentUser);
            if (sessiondata.User.UserGlobalAdmin)
            {
                // Delete files which is older than one month
                // Get All Directory 
                // for each directory => get all the files which is created before one month from today
                // delete those file and recheck directory => if no file left in that directory than delete directory as well
                try
                {
                    //string pathHarvestTemp = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Temp\";
                    string pathHarvestTemp = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Temp\";
                    DateTime oneMonthOldDateFromToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    var AllDir = Directory.GetDirectories(pathHarvestTemp);
                    foreach (var dir in AllDir)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(dir);
                        FileInfo[] dirFileInfo = dirInfo.GetFiles()
                            .OrderBy(f => f.CreationTime)
                            .Where(p => p.CreationTime < oneMonthOldDateFromToday).ToArray();
                        foreach (var filedel in dirFileInfo)
                        {
                            if (File.Exists(filedel.FullName))
                            {
                                File.Delete(filedel.FullName);
                            }
                        }
                        bool IsFileStillRemainInDir = dirInfo.GetFiles().Any();
                        if (!IsFileStillRemainInDir)
                        {
                            Directory.Delete(dir);
                        }
                    }
                    return "Temp files deleted successfully.";
                }
                catch (Exception ex)
                {
                    return "Exceprion occured!\n " + ex.Message;
                }
            }
            else
            {
                return "You have no access to delete temp files.";
            }
        }

        public string DeleteWEBUITempFiles(IPrincipal currentUser)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(currentUser);
            if (sessiondata.User.UserGlobalAdmin)
            {
                // Delete files which is older than one month
                // Get All the files which is created before one month from today
                try
                {
                    //string pathWebUITemp = @"E:\Jenkins\workspace\GRSNG\GlobalReportingSystem.Web.UI\Temp\";
                    string pathWebUITemp = @"D:\Jenkins\workspace\GRSNG\GlobalReportingSystem.Web.UI\Temp\";
                    DateTime oneMonthOldDateFromToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    DirectoryInfo dirInfo = new DirectoryInfo(pathWebUITemp);
                    FileInfo[] dirFileInfo = dirInfo.GetFiles()
                        .OrderBy(f => f.CreationTime)
                        .Where(p => p.CreationTime < oneMonthOldDateFromToday).ToArray();
                    foreach (var filedel in dirFileInfo)
                    {
                        if (File.Exists(filedel.FullName))
                        {
                            File.Delete(filedel.FullName);
                        }
                    }
                    return "Temp files deleted successfully.";
                }
                catch (Exception ex)
                {
                    return "Exception occured!\n " + ex.Message;
                }
            }
            else
            {
                return "You do not permission to delete temp files.";
            }
        }

        public string DeleteChartFiles(IPrincipal currentUser)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(currentUser);
            if (sessiondata.User.UserGlobalAdmin)
            {
                // Delete files which is older than one month
                // Get All the files which is created before one month from today
                try
                {
                    //string pathHarvestChart = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Charts\";
                    string pathHarvestChart = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Charts\";
                    DateTime oneMonthOldDateFromToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    DirectoryInfo dirInfo = new DirectoryInfo(pathHarvestChart);
                    FileInfo[] dirFileInfo = dirInfo.GetFiles()
                        .OrderBy(f => f.CreationTime)
                        .Where(p => p.CreationTime < oneMonthOldDateFromToday).ToArray();
                    foreach (var filedel in dirFileInfo)
                    {
                        if (File.Exists(filedel.FullName))
                        {
                            File.Delete(filedel.FullName);
                        }
                    }
                    return "Chart file deleted successfully.";
                }
                catch (Exception ex)
                {
                    return "Exception occured!\n " + ex.Message;
                }
            }
            else
            {
                return "You do not permission to delete temp files.";
            }
        }

        public ManageTestSuits GetTestSuitList(IPrincipal currentUser, int projectId, DateTime dateBefore)
        {
            int userId = _sessionHelper.GetSessionDetails(currentUser).User.ID;
            var userData = _userRepository.GetSingleOrDefault(u => u.ID == userId);
            if (userData.UserGlobalAdmin == true)
            {
                DateTime castBeforeDateTime = new DateTime(dateBefore.Year, dateBefore.Month, dateBefore.Day, 0, 0, 0);
                var tsList = _testSuitRepository.GetAllToList(t => t.ParentProject == projectId && t.TSStart < castBeforeDateTime);
                var testsuitList = (from x in tsList
                                    select new TestsuitManageModel
                                    {
                                        ID = x.ID,
                                        TestsuitName = x.TSName,
                                        StartTime = x.TSStart.ToString("dd/MM/yyyy hh:mm:ss tt"),
                                        TotalFailedTestcase = Convert.ToString(x.FailedTestCases ?? 0),
                                        TotalPassedTestcase = Convert.ToString(x.PassedTestCases ?? 0),
                                        TestcycleName = _testCycle.GetSingleOrDefault(tc => tc.ID == x.ParentTestCycle).CycleName,
                                        clientIP = _clientsInformation.GetSingleOrDefault(ci => ci.ID == x.ParentClientInfo).ClientIP,
                                        ClientEnvironment = _clientsInformation.GetSingleOrDefault(ci => ci.ID == x.ParentClientInfo).ClientEnvironment,
                                        clientUser = _clientsInformation.GetSingleOrDefault(ci => ci.ID == x.ParentClientInfo).ClientUser
                                    }).ToList();

                string projectdisplqayname = _projectRepository.GetSingleOrDefault(p => p.ID == projectId).DisplayName;
                return new ManageTestSuits() { ProjectDisplayName = projectdisplqayname, TestsuitManageModel = testsuitList };
            }
            else
            {
                return new ManageTestSuits() { ProjectDisplayName = "", TestsuitManageModel = new List<TestsuitManageModel>() };
            }
        }

        public string DeleteSuitSuitsByIds(IPrincipal currentUser, List<Int64> TSuitIds)
        {
            int userId = _sessionHelper.GetSessionDetails(currentUser).User.ID;
            var userData = _userRepository.GetSingleOrDefault(u => u.ID == userId);
            if (userData.UserGlobalAdmin == true)
            {
                List<AttachmentDeleteByProjectIdAndName> attachmentToDelete = new List<AttachmentDeleteByProjectIdAndName>();
                List<string> ScreenShotToDelete = new List<string>();

                foreach (var tsid in TSuitIds)
                {
                    //Get All TestCase
                    var tSuit = _testSuitRepository.GetSingleOrDefault(ts => ts.ID == tsid);
                    var testCaseList = _testCase.GetAllToList(TCa => TCa.ParentTestSuite == tSuit.ID);
                    foreach (var tCase in testCaseList)
                    {
                        //Get All TestStep
                        var testStepList = _testStep.GetAllToList(TSt => TSt.ParentTestCase == tCase.ID);
                        foreach (var tStep in testStepList)
                        {
                            //Get all subsetp
                            var subStepList = _subStepRepository.GetAllToList(SS => SS.ParentStep == tStep.ID);
                            foreach (var sStep in subStepList)
                            {
                                // add screen shot file name in list
                                if (!string.IsNullOrEmpty(sStep.SubStepScreenShot)) { ScreenShotToDelete.Add(sStep.SubStepScreenShot); }
                                // delete this sub step
                                var substepdel = _subStepRepository.GetSingleOrDefault(d => d.ID == sStep.ID);
                                _subStepRepository.Delete(substepdel);
                                _subStepRepository.SaveChanges();
                            }
                            //Add attachment file name in List
                            if (!string.IsNullOrEmpty(tStep.Attachments))
                            {
                                attachmentToDelete.Add(new AttachmentDeleteByProjectIdAndName() { ProjectId = tSuit.ParentProject ?? 0, AttachmentFileName = tStep.Attachments });
                            }
                            //Add screenshot to list
                            if (!string.IsNullOrEmpty(tStep.StepScreenshot)) { ScreenShotToDelete.Add(tStep.StepScreenshot); }
                            if (!string.IsNullOrEmpty(tStep.StepScreenshotDriver)) { ScreenShotToDelete.Add(tStep.StepScreenshotDriver); }
                            //delete this test step
                            var teststepdel = _testStep.GetSingleOrDefault(d => d.ID == tStep.ID);
                            _testStep.Delete(teststepdel);
                            _testStep.SaveChanges();
                        }
                        //add attachment file to delete list
                        if (!string.IsNullOrEmpty(tCase.TCAttachments))
                        {
                            attachmentToDelete.Add(new AttachmentDeleteByProjectIdAndName() { ProjectId = tSuit.ParentProject ?? 0, AttachmentFileName = tCase.TCAttachments });
                        }
                        // delete this test case
                        var testcasedel = _testCase.GetSingleOrDefault(d => d.ID == tCase.ID);
                        _testCase.Delete(testcasedel);
                        _testCase.SaveChanges();
                    }
                    //Delete Analysers
                    var analysersData = _analyser.GetAllToList(A => A.TestSet == tSuit.ID);
                    foreach (var anlritem in analysersData)
                    {
                        var deleteAnalyser = _analyser.GetSingleOrDefault(a => a.ID == anlritem.ID);
                        _analyser.Delete(deleteAnalyser);
                        _analyser.SaveChanges();
                    }
                    int clientinformationid = tSuit.ParentClientInfo ?? 0;
                    // delete this test suit
                    var testsuitdel = _testSuitRepository.GetSingleOrDefault(d => d.ID == tSuit.ID);
                    _testSuitRepository.Delete(testsuitdel);
                    _testSuitRepository.SaveChanges();
                    //delete ClientInformation
                    if (clientinformationid > 0)
                    {
                        Int64 isclientinfoidexist = _testSuitRepository.Count(c => c.ParentClientInfo.HasValue && c.ParentClientInfo == clientinformationid);
                        if (isclientinfoidexist <= 0)
                        {
                            var parentclientinfodel = _clientsInformation.GetSingleOrDefault(d => d.ID == clientinformationid);
                            _clientsInformation.Delete(parentclientinfodel);
                            _clientsInformation.SaveChanges();
                        }
                    }
                }

                // Delete attachment file

                foreach (var atfile in attachmentToDelete)
                {
                    //string ATTACHMENTPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Attachments\" + atfile.ProjectId + "\\";
                    string ATTACHMENTPATH = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Attachments\" + atfile.ProjectId + "\\";
                    if (File.Exists(ATTACHMENTPATH + atfile.AttachmentFileName))
                    {
                        File.Delete(ATTACHMENTPATH + atfile);
                    }
                }

                // Delete ScreenShots
                //string SCREENSHOTPATH = @"E:\Jenkins\workspace\GRSNG\GRSHarvest\Screenshots\";
                string SCREENSHOTPATH = @"D:\Jenkins\workspace\GRSNG\GRSHarvest\Screenshots\";
                foreach (var scsfile in ScreenShotToDelete)
                {
                    if (File.Exists(SCREENSHOTPATH + scsfile))
                    {
                        File.Delete(SCREENSHOTPATH + scsfile);
                    }
                }

                return "Selected testsuit(s) and related records/files deleted successfully.";
            }
            else
            {
                return "You are not authorized to delete Testsuit(s).";
            }
        }

        public List<TrackingExecutionsAndLogs> GetRelExecutionTrackingData(IPrincipal currentUser, string MachineIp)
        {
            int userId = _sessionHelper.GetSessionDetails(currentUser).User.ID;
            var userData = _userRepository.GetSingleOrDefault(u => u.ID == userId);
            if (userData.UserGlobalAdmin == true)
            {
                var reltrackdata = _relExecutionStatu.GetAllToList(r => r.MachineIP == MachineIp);
                var reltrackwithexecutiondata = (from x in reltrackdata
                                                 select new
                                                 {
                                                     RelExeID = x.ID,
                                                     executindata = _testsExecution.GetSingleOrDefault(te => te.ID == x.ExecutionId),
                                                     ExecutionStarted = x.ExecutionStarted.ToString("dd/MM/yyyy hh:mm:ss tt"),
                                                     LatestStatusCheckedAt = x.CurrentStatusCheckedAt.ToString("dd/MM/yyyy hh:mm:ss tt"),
                                                     CurrentStatus = x.CurrentStatus,
                                                     LastRowCount = x.LastRowCount,
                                                     LastRowId = x.LastRowId,
                                                     NewRowCount = x.NewRowCount,
                                                     NewRowId = x.NewRowId,
                                                     TestSuitIds = x.TestSuitIds
                                                 }).ToList();
                return (from y in reltrackwithexecutiondata
                        select new TrackingExecutionsAndLogs
                        {
                            RelExeID = y.RelExeID,
                            ProjectName = _projectRepository.GetSingleOrDefault(p => p.ID == y.executindata.BelongToProject).DisplayName,
                            CycleName = _testCycle.GetSingleOrDefault(tc => tc.ID == y.executindata.TargetTestCycle).CycleName,
                            Tests = y.executindata.Tests,
                            ExecutionStarted = y.ExecutionStarted,
                            LatestStatusCheckedAt = y.LatestStatusCheckedAt,
                            CurrentStatus = y.CurrentStatus,
                            LastRowCount = Convert.ToString(y.LastRowCount),
                            LastRowId = Convert.ToString(y.LastRowId),
                            NewRowCount = Convert.ToString(y.NewRowCount),
                            NewRowId = Convert.ToString(y.NewRowId),
                            TestSuitIds = y.TestSuitIds
                        }).ToList();
            }
            else
            {
                return new List<TrackingExecutionsAndLogs>();
            }
        }

        public bool DeleteRelExecutionTrackingDataAndLogs(IPrincipal currentUser, Int64[] relExIds)
        {
            int userId = _sessionHelper.GetSessionDetails(currentUser).User.ID;
            var userData = _userRepository.GetSingleOrDefault(u => u.ID == userId);
            if (userData.UserGlobalAdmin == true)
            {
                foreach (var item in relExIds)
                {
                    if (item > 0)
                    {
                        string machineIP = string.Empty;
                        var reltodel = _relExecutionStatu.GetSingleOrDefault(r => r.ID == item);
                        machineIP = reltodel.MachineIP;
                        _relExecutionStatu.Delete(reltodel);
                        _relExecutionStatu.SaveChanges();

                        _relExecutionStatusLog.Delete(t => t.MachineIP == machineIP);
                        _relExecutionStatusLog.SaveChanges();
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<ProjectType> GetProjectTypes()
        {
            var ptlist = _projectType.GetAllToList();
            return (from x in ptlist
                    orderby x.ProjectTypeName ascending
                    select new ProjectType
                    {
                        ID = x.ID,
                        ProjectTypeName = x.ProjectTypeName
                    }).ToList();
        }

        //==============================================================================================

        #region TEAM
        public TeamInfoModel GetTeams(IPrincipal user)
        {
            //var session = _sessionHelper.GetSessionDetails(user);
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;

            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return new TeamInfoModel()
            {
                TeamFromDb = HCP.GetTeamInformation(projectId, userId)
            };
        }

        public TeamInfo GetTeamById(Int64 teamid)
        {
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.GetTeamInformationById(teamid);
        }

        public string AddNewTeam(IPrincipal user, string teamName, string comment)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.AddNewTeam(teamName, comment, projectId, userId);
        }

        public string UpdateTeamInfo(IPrincipal user, Int64 teamId, string teamName, string comment)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.UpdateExistingTeam(teamId, teamName, comment, projectId, userId);
        }

        public string DeleteTeam(IPrincipal user, Int64 teamId)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.DeleteExistingTeam(teamId, projectId, userId);
        }
        #endregion TEAM

        #region RELEASE

        public ReleaseInfoModel GetReleases(IPrincipal user)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;

            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return new ReleaseInfoModel()
            {
                ReleaseFromDb = HCP.GetReleaseInformation(projectId, userId)
            };
        }

        public ReleaseInfo GetReleaseById(Int64 releaseid)
        {
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.GetReleaseInformationById(releaseid);
        }

        public string AddNewRelease(IPrincipal user, string releaseName, string releaseDate, string comment)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.AddNewRelease(releaseName, releaseDate, comment, projectId, userId);
        }

        public string UpdateReleaseInfo(IPrincipal user, Int64 releaseId, string releaseName, string releaseDate, string comment)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.UpdateExistingRelease(releaseId, releaseName, releaseDate, comment, projectId, userId);
        }

        public string DeleteRelease(IPrincipal user, Int64 releaseId)
        {
            int userId = _sessionHelper.GetSessionDetails(user).User.ID;
            int projectId = _sessionHelper.GetSessionDetails(user).Project.ID;
            HelperConfigurationProvider HCP = new HelperConfigurationProvider();
            return HCP.DeleteExistingRelease(releaseId, projectId, userId);
        }

        #endregion RELEASE

        //==============================================================================================

        #region Group
        public List<ExecutionGroupModel> GetExecutionGroupInfo(IPrincipal user)
        {
            var session = _sessionHelper.GetSessionDetails(user);
            var data = _executionGroup.GetAllToList(s => s.ProjectId == session.Project.ID).ToList();
            return (from x in data 
                    orderby x.GrupName ascending
                    select new ExecutionGroupModel 
                    {
                        Id = x.ID,
                        GroupName = x.GrupName
                    }).ToList();
        }
                
        public ResultReturnModel AddExecutionGroup(IPrincipal user, string groupName)
        {
            try
            {
                var session = _sessionHelper.GetSessionDetails(user);
                Int64 gcount = _executionGroup.Count(s => s.GrupName == groupName && s.ProjectId == session.Project.ID);
                if (gcount == 0)
                {
                    ExecutionGroup pgroup = new ExecutionGroup()
                    {
                        GrupName = groupName.Trim(),
                        ProjectId = session.Project.ID
                    };
                    _executionGroup.Add(pgroup);
                    _executionGroup.SaveChanges();
                    return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group added successfully." };
                }
                else 
                {
                    return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Group name already Exist!" };
                }
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }

        public ResultReturnModel UpdateExecutionGroup(IPrincipal user, Int64 id, string groupName)
        {
            try
            {
                var pgroup = _executionGroup.GetFirstOrDefault(s => s.ID == id);
                pgroup.GrupName = groupName.Trim();
                _executionGroup.SaveChanges();
                return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }

        public ResultReturnModel DeleteExecutionGroup(IPrincipal user, Int64 id)
        {
            try
            {
                var pgroup = _executionGroup.GetFirstOrDefault(s => s.ID == id);
                if (pgroup != null)
                {
                    Int64 isExecutionGroupReferred = _grpExecutionInfo.Count(s => s.GroupID == id);
                    if (isExecutionGroupReferred == 0)
                    {
                        _executionGroup.Delete(pgroup);
                        _executionGroup.SaveChanges();
                        return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group deleted successfully." };
                    }
                    else
                    {
                        return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Project group is referred for one or more group execution! Unable to delete Project group." };
                    }
                }
                else
                {
                    return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Project group not found!" };
                }
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }

        public List<ExecutionGroupClient_GetModel> GetExecutionGroup_Clients(IPrincipal user)
        {
            //var session = _sessionHelper.GetSessionDetails(user);
            //var data = _executionGroup.GetAllToList(s => s.ProjectId == session.Project.ID).ToList();
            //return (from x in data
            //        orderby x.GrupName ascending
            //        select new ExecutionGroupModel
            //        {
            //            Id = x.ID,
            //            GroupName = x.GrupName
            //        }).ToList();
            return new List<ExecutionGroupClient_GetModel>();
        }

        public ResultReturnModel AddExecutionGroup_Clients(IPrincipal user, string groupName)
        {
            try
            {
                var session = _sessionHelper.GetSessionDetails(user);
                Int64 gcount = _executionGroup.Count(s => s.GrupName == groupName && s.ProjectId == session.Project.ID);
                if (gcount == 0)
                {
                    ExecutionGroup pgroup = new ExecutionGroup()
                    {
                        GrupName = groupName.Trim(),
                        ProjectId = session.Project.ID
                    };
                    _executionGroup.Add(pgroup);
                    _executionGroup.SaveChanges();
                    return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group added successfully." };
                }
                else
                {
                    return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Group name already Exist!" };
                }
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }

        public ResultReturnModel UpdateExecutionGroup_Clients(IPrincipal user, Int64 id, string groupName)
        {
            try
            {
                var pgroup = _executionGroup.GetFirstOrDefault(s => s.ID == id);
                pgroup.GrupName = groupName.Trim();
                _executionGroup.SaveChanges();
                return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }

        public ResultReturnModel DeleteExecutionGroup_Clients(IPrincipal user, Int64 id)
        {
            try
            {
                var pgroup = _executionGroup.GetFirstOrDefault(s => s.ID == id);
                if (pgroup != null)
                {
                    Int64 isExecutionGroupReferred = _grpExecutionInfo.Count(s => s.GroupID == id);
                    if (isExecutionGroupReferred == 0)
                    {
                        _executionGroup.Delete(pgroup);
                        _executionGroup.SaveChanges();
                        return new ResultReturnModel() { ResultReturnId = 0, ResultReturnMessage = "Project group deleted successfully." };
                    }
                    else
                    {
                        return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Project group is referred for one or more group execution! Unable to delete Project group." };
                    }
                }
                else
                {
                    return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = "Project group not found!" };
                }
            }
            catch (Exception ex)
            {
                return new ResultReturnModel() { ResultReturnId = 1, ResultReturnMessage = ex.Message };
            }
        }
        #endregion Group


    }
}
