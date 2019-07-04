using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Principal;
using Castle.Core.Internal;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Enums;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.GRS.DB;
using GlobalReportingSystem.Core.Models.GRS.PublicAccess;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageTestCycleProvider : IManageTestCycleProvider
    {
        private readonly IRepository<TestCycle> _testCycleRepository;

        private readonly IRepository<TestSuit> _testSuitRepository;

        private readonly IRepository<TestCase> _testCaseRepository;

        private readonly IRepository<TestStep> _testStepRepository;

        private readonly IRepository<SubStep> _subStepRepository;

        private readonly IRepository<Project> _projectRepository;

        private readonly IRepository<Analysis> _analysisRepository;

        private readonly IRepository<AutoAnalysisCache> _autoAnalysisRepository;

        private readonly IRepository<Client> _clientRepository;

        private readonly IRepository<vw_FilesStorage> _vw_FilesStorageRepository;

        private readonly IRepository<HostsConfiguration> _hostsConfiguration;

        private readonly IRepository<AccountForTestRun> _accountForTestRun;

        private readonly IRepository<ExecutionConfiguration> _executionConfiguration;

        private readonly IRepository<TestsExecution> _testsExecutions;

        private readonly IRepository<User> _user;

        private readonly IRepository<Analyser> _analyser;

        private readonly ISessionHelper _sessionHelper;

        public ManageTestCycleProvider(IRepository<TestCycle> testCycleRepository,
            IRepository<TestSuit> testSuitRepository, IRepository<TestCase> testCaseRepository,
            IRepository<TestStep> testStepRepository, IRepository<SubStep> subStepRepository,
            IRepository<Project> projectRepository, IRepository<Analysis> analysisRepository,
            IRepository<Client> clientRepository, IRepository<vw_FilesStorage> vw_FilesStorageRepository,
            IRepository<HostsConfiguration> hostsConfiguration, IRepository<AccountForTestRun> accountForTestRun,
            IRepository<ExecutionConfiguration> executionConfiguration, IRepository<TestsExecution> testsExecutions,
            IRepository<User> user, IRepository<AutoAnalysisCache> autoAnalysisRepository, ISessionHelper sessionHelper,
            IRepository<Analyser> analyser)
        {
            _testCycleRepository = testCycleRepository;
            _testSuitRepository = testSuitRepository;
            _testCaseRepository = testCaseRepository;
            _testStepRepository = testStepRepository;
            _subStepRepository = subStepRepository;
            _projectRepository = projectRepository;
            _analysisRepository = analysisRepository;
            _clientRepository = clientRepository;
            _vw_FilesStorageRepository = vw_FilesStorageRepository;
            _hostsConfiguration = hostsConfiguration;
            _accountForTestRun = accountForTestRun;
            _executionConfiguration = executionConfiguration;
            _testsExecutions = testsExecutions;
            _user = user;
            _autoAnalysisRepository = autoAnalysisRepository;
            _sessionHelper = sessionHelper;
            _analyser = analyser;
        }

        public bool IsTestCycleExist(int id)
        {
            return _testCycleRepository.GetFirstOrDefault_Services(p => p.ID == id) != null;
        }

        public bool IsUserHasAccess(int userId, int projectId)
        {
            var user = _user.GetFirstOrDefault_Services(p => p.ID == userId);
            if (user.UserGlobalAdmin)
            {
                return true;
            }
            else if (user.Accesses.Count(p => p.AttachedProject.HasValue && p.AttachedProject == projectId) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SwitchProject(IPrincipal user, Project project)
        {
            _sessionHelper.GetSessionDetails(user).Project = project;
            _sessionHelper.SaveChanges();
        }
        public TestCycle GetTestCycle(int id)
        {
            return _testCycleRepository.GetIncluding(p => p.TestSuits).FirstOrDefault(p => p.ID == id);
        }

        //public List<ItemId> GetTestSuitsByKey(string key, int cycleId)
        //{
        //    var items = new List<ItemId>();
        //    var cycle = _testCycleRepository.GetFirstOrDefault(p => p.ID == cycleId);
        //    if (cycle != null)
        //    {
        //        var testSuits = cycle.TestSuits.Where(p => p.TSName == key).OrderBy(p => p.TSName);
        //        testSuits.GroupBy(p => p.ClientsInformation.ClientIP).ToList().ForEach(p => items.Add(new ItemId
        //        {
        //            Id = cycleId,
        //            Item = p.Key,
        //            Fail = p.Sum(h => h.FailedTestCases) ?? 0,
        //            Pass = p.Sum(h => h.PassedTestCases) ?? 0
        //        }));
        //    }
        //    return items;
        //}

        public List<ItemId> GetTestSuitsByKey(string key, int cycleId)
        {
            var items = new List<ItemId>();
            var cycle = _testCycleRepository.GetFirstOrDefault(p => p.ID == cycleId);
            if (cycle != null)
            {
                var testSuits = cycle.TestSuits.Where(p => p.TSName == key).OrderBy(p => p.TSName);
                testSuits.GroupBy(p => p.ClientsInformation.ClientIP).ToList().ForEach(p => items.Add(new ItemId
                {
                    Id = cycleId,
                    Item = p.Key,
                    Fail = p.Sum(h => h.FailedTestCases) ?? 0,
                    Pass = p.Sum(h => h.PassedTestCases) ?? 0
                }));
            }
            return items;
        }

        public List<ItemId> GetTestSuitsByKeyAndIp(string ip, int cycleId, string testSuitName)
        {
            var items = new List<ItemId>();
            var cycle = _testCycleRepository.GetFirstOrDefault(p => p.ID == cycleId);
            if (cycle != null)
            {
                //var da = cycle.TestSuits.Where(p => p.TSName == testSuitName).OrderBy(p => p.TSName)
                //.Where(p => p.ClientsInformation.ClientIP == ip.Trim()).ToList();
                cycle.TestSuits.Where(p => p.TSName == testSuitName).OrderBy(p => p.TSStart)
                .Where(p => p.ClientsInformation.ClientIP == ip.Trim())
                    .ForEach(p =>
                    {
                        var timeS = new TimeSpan();
                        p.TestCases.ToList().ForEach(d => timeS += (d.TCEndTime - d.TCStartTime));

                        items.Add(new ItemId
                        {
                            Id = p.ID,
                            Fail = p.FailedTestCases ?? 0,
                            Pass = p.PassedTestCases ?? 0,
                            Item = string.Join(";", new List<string>
                                {
                                    p.DeliveryTime == null ? p.TSStart.ToString(CultureInfo.InvariantCulture) : p.DeliveryTime.Value.ToString(CultureInfo.InvariantCulture),
                                    p.ClientsInformation.ClientURL,
                                    p.ClientsInformation.ClientIP,
                                    p.ClientsInformation.ClientBrowser,
                                    p.ClientsInformation.ClientOS,
                                    p.ClientsInformation.ClientIP,
                                    "Duration: " + timeS.ToString(@"dd\.hh\:mm\:ss")
                                })
                        });
                    });
            }
            return items;
        }
        public ItemId GetTestSuitDetails(string id)
        {
            var tsId = Int32.Parse(id);
            var ts = _testSuitRepository.GetFirstOrDefault_Services(p => p.ID == tsId);
            var item = new ItemId();
            if (ts != null)
            {
                var timeS = new TimeSpan();
                ts.TestCases.ToList().ForEach(d => timeS += (d.TCEndTime - d.TCStartTime));
                item = new ItemId
                        {
                            Id = ts.ID,
                            Fail = ts.FailedTestCases ?? 0,
                            Pass = ts.PassedTestCases ?? 0,
                            Item = string.Join(";", new List<string>
                                {
                                    ts.DeliveryTime == null ? ts.TSStart.ToString(CultureInfo.InvariantCulture) : ts.DeliveryTime.Value.ToString(CultureInfo.InvariantCulture),
                                    ts.ClientsInformation.ClientURL,
                                    ts.ClientsInformation.ClientIP,
                                    ts.ClientsInformation.ClientBrowser,
                                    ts.ClientsInformation.ClientOS,
                                    ts.ClientsInformation.ClientIP,
                                    "Duration: " + timeS.ToString(@"dd\.hh\:mm\:ss")
                                })
                        };
            }
            return item;
        }

        public TestCasesAndStepsModel GetTsLegend(int id, TestCasesAndStepsModel model = null)
        {
            var testCases = new TestCasesAndStepsModel();

            if (model != null)
                testCases = model;

            var ts = _testSuitRepository.GetSingleOrDefault_Services(p => p.ID == id);

            var testCasesCasche = _testCaseRepository.GetAllToList(p => p.ParentTestSuite == id);

            testCases.TSuitName = ts.TSName;
            testCases.TestSuiteID = ts.ID;
            testCases.TotalCount = testCasesCasche.Count();
            testCases.PassCount = testCasesCasche.Count(p => p.TCState == "pass");
            testCases.FailCount = testCasesCasche.Count(p => p.TCState == "fail");
            testCases.NotCompetedCount = testCasesCasche.Count(p => p.TCState == "notcompleted");
            testCases.NoRunCount = testCasesCasche.Count(p => p.TCState == "norun");
            testCases.NotAnalyzedCount = testCasesCasche.Count(p => !p.isAnalyzed && p.TCState != "pass");
            testCases.AnalyzedCount = testCasesCasche.Count(p => p.isAnalyzed);
            return testCases;
        }

        public TestCasesAndStepsModel GetTestSuite(int id, string filter)
        {
            var testCases = new TestCasesAndStepsModel();
            var ts = _testSuitRepository.GetSingleOrDefault_Services(p => p.ID == id);
            //made by dmitry bakaev on 3/4/2015 performance improvment
            //var dedicatedTs = _testSuitRepository.GetAll(p => p.DedicatedLinearExecution == ts.DedicatedLinearExecution).ToList();
            var tcList = new List<TestCasesModel>();

            var testCasesCasche = _testCaseRepository.GetIncludingWithFiltering_Services(p => p.ParentTestSuite == id,
                p => p.TestSteps);
            var selection = new List<TestCase>();

        FillSelection:
            try
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    var curStType = (CurrentStatusType)Enum.Parse(typeof(CurrentStatusType), filter, true);
                    switch (curStType)
                    {
                        case CurrentStatusType.Passed:
                            selection = testCasesCasche.Where(p => p.TCState == "pass").ToList();
                            break;
                        case CurrentStatusType.Failed:
                            selection = testCasesCasche.Where(p => p.TCState == "fail").ToList();
                            break;
                        case CurrentStatusType.Notcompleted:
                            selection = testCasesCasche.Where(p => p.TCState == "notcompleted").ToList();
                            break;
                        case CurrentStatusType.Norun:
                            selection = testCasesCasche.Where(p => p.TCState == "norun").ToList();
                            break;
                        case CurrentStatusType.Notanalyzed:
                            selection = testCasesCasche.Where(p => !p.isAnalyzed && p.TCState != "pass").ToList();
                            break;
                        case CurrentStatusType.Analyzed:
                            selection = testCasesCasche.Where(p => p.isAnalyzed).ToList();
                            break;
                    }
                }
                else
                {
                    selection = testCasesCasche.ToList();
                }
            }
            catch (System.Data.EntityCommandExecutionException)
            {
                Thread.Sleep(90000);
                goto FillSelection;
            }
            selection.ForEach(p =>
                tcList.Add(new TestCasesModel
                {
                    TCDescription = p.TCDescription,
                    TCEndTime = p.TCEndTime,
                    ID = p.ID,
                    TCName = p.TCName,
                    TCStartTime = p.TCStartTime,
                    TCState = p.TCState,
                    isAnalyzed = p.isAnalyzed,
                    qc_id = p.qc_id
                })
                );

            testCases.TestSuiteID = ts.ID;
            testCases.TSuitName = ts.TSName;
            testCases.TSuitStart = ts.TSStart;
            testCases.TestCases = tcList;
            testCases.ProjectID = ts.ParentProject ?? 0;
            // testCases.DedicatedSuits = dedicatedTs;
            testCases.DedicatedLinearExecution = ts.DedicatedLinearExecution;
            testCases.DedicatedLoadBalancedExecution = ts.DedicatedLbExecution;
            return testCases;
        }

        public void AddCommentToTestSuite(int id, string value)
        {
            var testSuite = _testSuitRepository.GetFirstOrDefault(p => p.ID == id);
            testSuite.Comments = String.IsNullOrWhiteSpace(value) ? null : value;
            _testSuitRepository.SaveChanges();
        }

        public TestCasesAndStepsModel GetTestSuiteAndLegend(int id, string filter)
        {
            var testCases = GetTestSuite(id, filter);
            return GetTsLegend(id, testCases);
        }

        public TestStepAndProject GetSubSteps(int id)
        {
            var testStep = new TestStepAndProject();

            testStep.TestStep = _testStepRepository.GetIncluding(p => p.SubSteps).SingleOrDefault(p => p.ID == id);
            var projId = testStep.TestStep.TestCase.TestSuit.ParentProject ?? 0;
            var customStatuses = new CustomStatuses.CustomStatuses(projId, _projectRepository);

            testStep.TestStepAndCustomStatus = new List<SubStepAndCustomStatus>();
            _subStepRepository
                .GetAllToList(p => p.ParentStep == id)
                .ForEach(p => testStep.TestStepAndCustomStatus.Add
                    (new SubStepAndCustomStatus
                    {
                        SubStep = p,
                        CustomStatus = ((p.AnalyzedStatus == null) ? null :
                            ((customStatuses)
                            .getStatus(p.AnalyzedStatus.Value).StatusName))
                    }));
            testStep.ProjectId = projId;
            try
            {
                testStep.Bugtracker = testStep.TestStep.TestCase.TestSuit.Project.Bugracker.Url;
            }
            catch { }
            testStep.CustomStatusesList = customStatuses.Statuses.Select(p => p.StatusName).ToList();

            //testStep.TestStepAndCustomStatus = testStep.TestStepAndCustomStatus.OrderBy(p => p.SubStep.SubStepTime).ToList();

            return testStep;
        }

        public TestCasesAndProject GetTestCases(int id)
        {
            var testCaseModel = new TestCasesAndProject();
            var testCase = _testCaseRepository.GetSingleOrDefault_Services(p => p.ID == id);
            var projId = testCase.TestSuit.ParentProject ?? 0;
            var customStatuses = new CustomStatuses.CustomStatuses(projId, _projectRepository);

            testCaseModel.TestCaseAndAnalysis = new TestCaseAndAnalysis();
            testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis = new List<TestStepAndAnalysis>();
            testCaseModel.TestCaseAndAnalysis.TestCase = testCase;
            //var testCase = _testCaseRepository//.GetIncluding(p => p.TestSteps)
            //.GetSingleOrDefault(p => p.ID == id);
            var testSteps = testCase.TestSteps;//.OrderBy(p => p.StepStartTime);

            foreach (var step in testSteps)
            {
                var status = GetGlobalState(step.ID);
                if (status != null)
                {
                    try
                    {
                        var custStat = customStatuses.getStatus((Guid.Parse(status.StepState)));
                        testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis.Add(
                            new TestStepAndAnalysis
                            {
                                TestStep = step,
                                Status =
                                    custStat == null ? "" : custStat.StatusName,
                                CurrentDefects = status.CurrentDefects ?? "",
                                OldDefects = status.OldDefects ?? "",
                                SortOrder = !step.SortOrder.HasValue ? 0 : (int)step.SortOrder
                            });
                    }
                    catch (FormatException)
                    {
                        testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis.Add(
                            new TestStepAndAnalysis
                            {
                                TestStep = step,
                                Status = "",
                                CurrentDefects = status.CurrentDefects ?? "",
                                OldDefects = status.OldDefects ?? ""
                            });
                    }
                }
                else
                {
                    testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis.Add(
                        new TestStepAndAnalysis
                        {
                            TestStep = step,
                            Status = "",
                            CurrentDefects = "",
                            OldDefects = ""
                        });
                }
            }

            //testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis = testCaseModel.TestCaseAndAnalysis.TestStepAndAnalysis.OrderBy(p => p.TestStep.StepStartTime).ToList();

            testCaseModel.ProjectID = testCase.TestSuit.ParentProject ?? 0;
            //try on bug tracker - added by sundram
            try
            {
                testCaseModel.Bugtracker = testCase.TestSuit.Project.Bugracker.Url;
            }
            catch { }
            return testCaseModel;
        }

        //delete testcase by id
        public void DeleteTestCase(int id)
        {
            List<string> attachmentsToDelete = new List<string>();
            List<string> screenShotsToDelete = new List<string>();
            var testCase = _testCaseRepository.GetSingleOrDefault(p => p.ID == id);
            //getting attachments need to be deleted
            _testStepRepository.GetAll(p => p.ParentTestCase == testCase.ID)
                        .Where(p => p.Attachments != null)
                        .ForEach(p => attachmentsToDelete.AddRange(p.Attachments.Split(';').ToList()));
            testCase.TestSteps.Where(p => p.StepScreenshotDriver != null).ForEach(p => screenShotsToDelete.AddRange(p.StepScreenshotDriver.Split(';').ToList()));
            //decrease count of failed\passed testcases in testsuit
            if (testCase.TCState.ToLower() == "pass")
            {
                testCase.TestSuit.PassedTestCases -= 1;
            }
            else
            {
                testCase.TestSuit.FailedTestCases -= 1;
            }
            //deleting the testcase
            _testCaseRepository.Delete(p => p.ID == id);
            //deleting attachments and screenshots
            string screenshotFolder =
                     (HttpContext.Current.Server.MapPath("~").Replace("GlobalReportingSystem.Web.UI", "GRSHarvest") +
                      @"\Screenshots\").Replace(@"\\", @"\");
            DeleteFiles(screenShotsToDelete.Where(screenShot => File.Exists(
                screenshotFolder + screenShot)).Select(screenshot => screenshotFolder + screenshot));
            string attachmentFolder =
                (HttpContext.Current.Server.MapPath("~").Replace("GlobalReportingSystem.Web.UI", "GRSHarvest") +
                 @"\Attachments\" + testCase.TestSuit.TestCycle.Project.ID.ToString() + @"\").Replace(@"\\", @"\");
            DeleteFiles(attachmentsToDelete.Where(attachment => File.Exists(
                attachmentFolder + attachment)).Select(attachment => attachmentFolder + attachment));
            _testSuitRepository.SaveChanges();
        }

        public void UpdateSubStep(int id, string state, string analyzedStatus, string defects, int userId = 0)
        {
            var subStep = _subStepRepository.GetFirstOrDefault(p => p.ID == id);
            var step = subStep.TestStep;
            var tCase = step.TestCase;

            var custStat =
                (new CustomStatuses.CustomStatuses(subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0,
                    _projectRepository));
            if (!string.IsNullOrEmpty(analyzedStatus))
            {
                var firstOrDefault = custStat.Statuses// CustomStatuses(subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0)).Statuses
                    .FirstOrDefault(p => p.StatusName == analyzedStatus);
                if (firstOrDefault != null)
                {
                    subStep.AnalyzedStatus =
                        firstOrDefault.UniqueID;
                    step.UI = firstOrDefault.UniqueID;
                }
            }

            if (state != analyzedStatus)
            {
                var newStatus = new Status();
                if (!string.IsNullOrEmpty(analyzedStatus) && analyzedStatus != "Pass" && analyzedStatus != "Fail")
                {
                    newStatus = custStat.Statuses// CustomStatuses(subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0)).Statuses
                        .FirstOrDefault(p => p.StatusName == analyzedStatus);
                }

                if (newStatus != null)
                {
                    subStep.SubStepValid = newStatus.CountAsPass;

                    if (!subStep.SubStepValid && step.StepType == "pass")
                    {
                        step.StepType = "fail";
                        if (tCase.TCState == "pass")
                        {
                            tCase.TCState = "fail";
                        }
                    }
                    else if (subStep.SubStepValid && step.StepType == "fail")
                    {
                        //var sSteps = step.SubSteps;
                        if (step.SubSteps.Count(p => !p.SubStepValid) == 0)
                        {
                            step.StepType = "pass";
                        }
                        var sSteps = step.TestCase.TestSteps.SelectMany(p => p.SubSteps.Select(s => s));
                        if (sSteps.Count(p => !p.SubStepValid) == 0 )
                        {
                            if (tCase.TCState == "fail")
                            {
                                tCase.TCState = "pass";
                            }
                        }
                    }

                }
            }

            subStep.Defects = defects;

            if (step.SubSteps.Count(p => !p.SubStepValid && p.AnalyzedStatus == null) == 0)
            {
                var topStat = custStat.getTopPriorityStatus(step.SubSteps.Where(p => p.AnalyzedStatus != null).Select(p => p.AnalyzedStatus.Value).ToList(),
                    custStat).StatusName;
                var cacheDefects = step.SubSteps.Where(p => p.AnalyzedStatus != null).Select(p => p.Defects).ToList();
                AddAnalysisCache(step.ID, topStat, cacheDefects);
                
            }

            if (
                tCase.TestSteps.Select(d => d.SubSteps.Count(p => !p.SubStepValid && p.AnalyzedStatus == null))
                    .Sum() == 0)
            {
                tCase.isAnalyzed = true;
                if (userId != 0)
                {
                    tCase.AnalyzedBy = _user.GetFirstOrDefault_Services(p => p.ID == userId).ID;
                    tCase.AnalyzedDate = DateTime.Now;
                }
                tCase.TestSuit.FailedTestCases = tCase.TestSuit.TestCases.Count(p => p.TCState != "pass");
                tCase.TestSuit.PassedTestCases = tCase.TestSuit.TestCases.Count(p => p.TCState == "pass");
            }

            _subStepRepository.SaveChanges();

            try
            {
                var status = custStat.Statuses.FirstOrDefault(p => p.StatusName == analyzedStatus);
                if (status != null && !status.NotAddToCache)
                {
                    AddAutoAnalyzeCache(subStep);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat("Substep status was updated. Got error during updating auto analyze cache: ", ex.Message));
            }
        }

        private void AddAutoAnalyzeCache(SubStep subStep)
        {
            var parentTestStep = subStep.ParentStep;
            var testCaseName = subStep.TestStep.TestCase.TCName;
            var testSuiteName = subStep.TestStep.TestCase.TestSuit.TSName;
            var projectID = subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0;

            string subStepMessage = UpdateByRegEx(subStep.SubStepMessage);

            var cacheToFind = _autoAnalysisRepository.GetFirstOrDefault(p => p.ProjectID == projectID && p.TestSuiteName == testSuiteName && p.TestCaseName == testCaseName && p.ErrorMessage == subStepMessage);
            if (cacheToFind == null)
            {
                var cache = new AutoAnalysisCache()
                {
                    DefectID = subStep.Defects,
                    ErrorMessage = subStepMessage,
                    ProjectID = projectID,
                    Status = subStep.AnalyzedStatus,
                    TestCaseName = testCaseName,
                    TestSuiteName = testSuiteName,
                    LastTimeUsed = DateTime.Now
                };
                _autoAnalysisRepository.Add(cache);
            }
            else
            {
                cacheToFind.DefectID = subStep.Defects;
                cacheToFind.Status = subStep.AnalyzedStatus;
                cacheToFind.LastTimeUsed = DateTime.Now;
            }
            _autoAnalysisRepository.SaveChanges();
        }

        public string UpdateByRegEx(string message)
        {
            message = Regex.Replace(message, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", "");
            message = Regex.Replace(message, @"\d+\.\d+", "");
            message = Regex.Replace(message, @":\d+", "");
            return message;
        }

        private Analysis GetGlobalState(int stepId)
        {
            var ParentStepId = _testStepRepository.GetSingleOrDefault(p => p.ID == stepId);
            string TS_Name = ParentStepId.TestCase.TCName;
            var StepNumber = ParentStepId.TestCase.TestSteps.OrderBy(p => p.ID).ToList().IndexOf(ParentStepId) + 1;
            var StepDetails = _analysisRepository.GetLastOrDefault(p => p.GlobalPositionTCName == TS_Name && p.GlobalPositionStepNumber == StepNumber);
            return StepDetails;
        }

        private void AddAnalysisCache(int stepId, string globalState, List<string> currentDefects)
        {
            var Step = _testStepRepository.GetSingleOrDefault(p => p.ID == stepId);
            string TestName = Step.TestCase.TCName;
            var steps = Step.TestCase.TestSteps.OrderBy(p => p.ID).ToList();
            int StepNum = steps.FindIndex(p => p.ID == stepId) + 1;
            //check if we have the same data been uploaded
            Analysis _Analysis;
            Analysis _ExistingAnalysis = null;
            try
            {
                _ExistingAnalysis = _analysisRepository.GetSingleOrDefault(
                     p => p.GlobalPositionTCName == TestName && p.GlobalPositionStepNumber == StepNum);
            }
            catch (InvalidOperationException) //if table contains more then one row for p.GlobalPositionTCName == TestName && p.GlobalPositionStepNumber == StepNum
            {
                var duplicates = _analysisRepository.GetAllToList(p => p.GlobalPositionTCName == TestName && p.GlobalPositionStepNumber == StepNum);
                if (duplicates.All(p => String.IsNullOrEmpty(p.CurrentDefects)) || duplicates.All(p => !String.IsNullOrEmpty(p.CurrentDefects)))
                {
                    var del = duplicates.OrderBy(p => p.RaisedDate).First().ID;
                    _analysisRepository.Delete(d => d.ID == del);
                }
                else
                {
                    var del = duplicates.Single(p => !String.IsNullOrEmpty(p.CurrentDefects)).ID;
                    _analysisRepository.Delete(d => d.ID == del);
                }
                // _analysisRepository.SaveChanges();
                _ExistingAnalysis = _analysisRepository.GetSingleOrDefault(
                     p => p.GlobalPositionTCName == TestName && p.GlobalPositionStepNumber == StepNum);
            }
            if (_ExistingAnalysis != null)
            {
                _Analysis = _ExistingAnalysis;
            }
            else
            {
                _Analysis = new Analysis();
            }

            _Analysis.BelongToProject = Step.TestCase.TestSuit.Project.ID;
            //_Analysis.Comments = Comments;
            if (Step.TestCase.TestSuit.TestCycle != null)
                _Analysis.ParentTestCycle = Step.TestCase.TestSuit.TestCycle.ID;

            if (!string.IsNullOrEmpty(_Analysis.CurrentDefects))
            {
                if (!string.IsNullOrEmpty(_Analysis.OldDefects))
                {
                    _Analysis.OldDefects =
                        string.Join(",", (_Analysis.CurrentDefects + "," + _Analysis.OldDefects).Split(',').Distinct().Except(currentDefects));
                }
                else
                {
                    _Analysis.OldDefects = string.Join(",", _Analysis.CurrentDefects.Split(',').Distinct().Except(currentDefects));
                }
            }
            else if (!string.IsNullOrEmpty(_Analysis.OldDefects))
            {
                _Analysis.OldDefects = string.Join(",", _Analysis.OldDefects.Split(',').Distinct().Except(currentDefects));
            }

            _Analysis.RaisedDate = DateTime.Now;
            _Analysis.CurrentDefects = string.Join(",", currentDefects.Distinct());

            var status = (new CustomStatuses.CustomStatuses(_Analysis.BelongToProject ?? 0,
                _projectRepository)).Statuses.FirstOrDefault(p => p.StatusName == globalState);
            if (status != null)
                _Analysis.StepState = status.UniqueID.ToString();
            _Analysis.GlobalPositionStepNumber = StepNum;
            _Analysis.GlobalPositionTCName = TestName;
            _Analysis.ParentStep = stepId;

            if (_Analysis.ID == 0)
                _analysisRepository.Add(_Analysis);
            _analysisRepository.SaveChanges();
        }

        public List<string> GetTestSuitesIdsByNames(int cycleId, List<string> tsNames)
        {


          var ids = new List<string>();
            foreach(var name in tsNames)
            {
                ids.AddRange(_testSuitRepository.GetAll(p => p.ParentTestCycle == cycleId && p.TSName == name).Select(p => p.ID.ToString()));
            }
            //DeleteTestSuites(ids);
            return ids;

              
            
        }
        public List<string> GetTestSuitesIdsByIP(int cycleId, List<string> tsNamesAndIps)
        {
            var ids = new List<string>();
            foreach (var item in tsNamesAndIps)
            {
                var tsName = item.Split('/')[0];
                var tsIp = item.Split('/')[1];

                ids.AddRange(GetTestSuitsByKeyAndIp(tsIp, cycleId, tsName).Select(p => p.Id.ToString()));
            }
            //DeleteTestSuites(ids);
            return ids;
        }

        public void DeleteTestSuites(List<string> tsIds)
        {
            if (tsIds.Count > 0)
            {
                int tsIdfirst = Convert.ToInt32(tsIds.First());
                var testCycleId = _testSuitRepository.GetFirstOrDefault(t => t.ID == tsIdfirst).ParentTestCycle;
                var testCycle = _testCycleRepository.GetSingleOrDefault(p => p.ID == testCycleId);
                List<string> attachmentsToDelete = new List<string>();
                List<string> screenShotsToDelete = new List<string>();
                foreach (var id in tsIds)
                {
                    var item = _testSuitRepository.GetFirstOrDefault(p => p.ID.ToString() == id);
                    foreach (var testCase in item.TestCases)
                    {
                        _testStepRepository.GetAll(p => p.ParentTestCase == testCase.ID)
                            .Where(p => p.Attachments != null)
                            .ForEach(p => attachmentsToDelete.AddRange(p.Attachments.Split(';').ToList()));
                        foreach (var testStep in testCase.TestSteps)
                        {
                            _subStepRepository.GetAll(p => p.ParentStep == testStep.ID)
                                .Where(p => p.SubStepScreenShotDriver != null)
                                .ForEach(p => screenShotsToDelete.AddRange(p.SubStepScreenShotDriver.Split(';').ToList()));
                        }
                    }
                    _testSuitRepository.Delete(p => p.ID.ToString() == id);
                }
                string screenshotFolder =
                        (HttpContext.Current.Server.MapPath("~").Replace("GlobalReportingSystem.Web.UI", "GRSHarvest") +
                         @"\Screenshots\").Replace(@"\\", @"\");
                DeleteFiles(screenShotsToDelete.Where(screenShot => File.Exists(
                    screenshotFolder + screenShot)).Select(screenshot => screenshotFolder + screenshot));

                string attachmentFolder =
                    (HttpContext.Current.Server.MapPath("~").Replace("GlobalReportingSystem.Web.UI", "GRSHarvest") +
                     @"\Attachments\" + testCycle.Project.ID.ToString() + @"\").Replace(@"\\", @"\");
                DeleteFiles(attachmentsToDelete.Where(attachment => File.Exists(
                    attachmentFolder + attachment)).Select(attachment => attachmentFolder + attachment));
            }
        }

        private void DeleteFiles(IEnumerable<string> toRemoveList)
        {
            foreach (var attachment in toRemoveList)
            {
                File.Delete(attachment);
            }
        }

        public List<int> GetSelectedTestSuitIds(int cycleId, List<string> tsInclude, List<string> ipInclude, List<string> fnInclude, bool selectAll,
            List<string> tsExclude, List<string> ipExclude, List<string> fnExclude)
        {
            var tsIdsInclude = tsInclude == null ? new List<int>() : tsInclude.Select(p => Int32.Parse(p.Trim())).ToList();
            var ipIdsInclude = ipInclude == null ? new List<string>() : ipInclude.Select(p => p.Trim()).ToList();
            var fnIdsInclude = fnInclude == null ? new List<string>() : fnInclude.Select(p => p.Trim()).ToList();
            var tsIdsExclude = tsExclude == null ? new List<int>() : tsExclude.Select(p => Int32.Parse(p.Trim())).ToList();
            var ipIdsExclude = ipExclude == null ? new List<string>() : ipExclude.Select(p => p.Trim()).ToList();
            var fnIdsExclude = fnExclude == null ? new List<string>() : fnExclude.Select(p => p.Trim()).ToList();

            if (selectAll)
            {
                tsIdsInclude.AddRange(_testSuitRepository.GetAll(p => p.ParentTestCycle == cycleId).Select(p => p.ID));

                if (tsIdsExclude.Count() != 0)
                {
                    tsIdsInclude = tsIdsInclude.Except(tsExclude.Select(p => Int32.Parse(p))).ToList();
                }

                if (ipIdsExclude.Count() != 0)
                    foreach (var ip in ipIdsExclude)
                    {
                        var ipts = GetTestSuitsByKeyAndIp(ip.Split('_')[1], cycleId, ip.Split('_')[0]).Select(p => p.Id).ToList();
                        if (!tsIdsInclude.Intersect(ipts).Any())
                            tsIdsInclude = tsIdsInclude.Except(ipts).ToList();
                    }

                if (fnIdsExclude.Count() != 0)
                    foreach (var fn in fnIdsExclude)
                    {
                        var ts = _testSuitRepository.GetAll(p => p.ParentTestCycle == cycleId && p.TSName == fn).Select(p => p.ID);
                        if (!tsIdsInclude.Intersect(ts).Any())
                            tsIdsInclude = tsIdsInclude.Except(ts).ToList();
                    }
            }
            else
            {
                if (fnIdsInclude.Count() != 0)
                    foreach (var fn in fnIdsInclude)
                    {
                        var ts = _testSuitRepository.GetAll(p => p.ParentTestCycle == cycleId && p.TSName == fn).Select(p => p.ID);
                        if (!tsIdsInclude.Intersect(ts).Any())
                            tsIdsInclude.AddRange(ts);
                    }

                if (ipIdsInclude.Count() != 0)
                    foreach (var ip in ipIdsInclude)
                    {
                        var ipValue = Regex.Match(ip, @"[\d]+[.][\d]+[.][\d]+[.][\d]+").Value;
                        var tsName = ip.Replace(String.Concat("_", ipValue), String.Empty);
                        //var ipts = GetTestSuitsByKeyAndIp(ip.Split('_')[1], cycleId, ip.Split('_')[0]).Select(p => p.Id).ToList();
                        var ipts = GetTestSuitsByKeyAndIp(ipValue, cycleId, tsName).Select(p => p.Id).ToList();
                        if (!tsIdsInclude.Intersect(ipts).Any())
                            tsIdsInclude.AddRange(ipts);
                    }
            }
            tsIdsInclude = tsIdsInclude.Distinct().ToList();

            return tsIdsInclude;
        }
        public void DeleteOldDefects(int stepId, string defectId)
        {
            var Step = _testStepRepository.GetSingleOrDefault(p => p.ID == stepId);
            string TestName = Step.TestCase.TCName;
            var steps = Step.TestCase.TestSteps.OrderBy(p => p.ID).ToList();
            int StepNum = steps.FindIndex(p => p.ID == stepId) + 1;
            Analysis _ExistingAnalysis =
    _analysisRepository.GetSingleOrDefault(
        p => p.GlobalPositionTCName == TestName && p.GlobalPositionStepNumber == StepNum);
            if (_ExistingAnalysis != null)
            {
                //_ExistingAnalysis.OldDefects = string.Join(",",
                //    _ExistingAnalysis.OldDefects.Split(',').Distinct().Except(new List<string> { defectId }));
                var oldDefects = _ExistingAnalysis.OldDefects.Split(',').ToList();
                if (oldDefects.Remove(defectId))
                {
                    _ExistingAnalysis.OldDefects = string.Join(",", oldDefects);
                }
            }
            _analysisRepository.SaveChanges();
        }

        public string GetTestSuiteName(int id)
        {
            var ts = _testSuitRepository.GetSingleOrDefault(p => p.ID == id);
            if (ts != null)
                return "Simple View for : " + ts.TSName + " " + ts.TSStart;
            return "";
        }

        public List<Project> GetPublicAcessProjects()
        {
            return _projectRepository.GetAll(p => p.isPublic
                //    , new Expression<Func<Project, object>>[]
                //{p => p.Clients, p => p.AccountForTestRuns, p => p.HostsConfigurations}
            ).OrderBy(p => p.DisplayName).ToList();
        }

        public List<Client> GetAppropriateMachines(int projId)
        {
            return _clientRepository.GetAll(p => p.BelongToProject == projId).OrderBy(p => p.RemoteMachineIP).ToList();
        }

        public int GetAppropriateTests(int projId)
        {
            var filesStorage = _vw_FilesStorageRepository.GetSingleOrDefault(
                p => p.FilesStorage_BelongToProject == projId && p.FilesStorage_Name == "From_SVN");

            if (filesStorage != null)
                return filesStorage.FilesStorage_ID;
            return 0;
        }

        public List<HostsConfiguration> GetAppropriateEnvironments(int projId)
        {
            return _hostsConfiguration.GetAll(p => p.BelongToProject == projId).OrderBy(p => p.EnvironmentName).ToList();
        }

        public List<AccountForTestRun> GetAppropriateAccounts(int projId)
        {
            return _accountForTestRun.GetAll(p => p.BelongToProject == projId).OrderBy(p => p.AccountName).ToList();
        }

        public List<User> GetAppropriateUsers(int projId)
        {
            var userList = new List<User>();
            _user.GetAll(p => p != null)
                .ForEach(o => userList.AddRange(o.Accesses.Where(e => e.AttachedProject == projId).Select(e => e.User)));
            return userList.OrderBy(p => p.UserName).ToList();
        }

        public FullConfigurationModel GetFullConfigurationModel(int projId)
        {
            var proj = _projectRepository.GetFirstOrDefault(p => p.ID == projId);
            var model = new FullConfigurationModel();
            model.AccountForTestRuns = GetAppropriateAccounts(projId);

            model.BrowserList = new Dictionary<string, string>();

            if (proj.isGUI)
            {
                model.BrowserList.Add("IE", "Internet Explorer");
                model.BrowserList.Add("FF", "Mozilla Firefox");
                model.BrowserList.Add("CH", "Google Chrome");
                model.BrowserList.Add("", "No browser");
            }

            model.ExecutionConfigurations = GetAppropriateConfigurations(projId);
            model.HostsConfigurations = GetAppropriateEnvironments(projId);

            return model;
        }

        public List<ExecutionConfiguration> GetAppropriateConfigurations(int projId)
        {
            var proj = _projectRepository.GetFirstOrDefault(p => p.ID == projId).ProjectName;
            XDocument xmlDoc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "/Content/ConfigurationConformity.xml");
            var configurations = xmlDoc.XPathSelectElements("//project[@name='" + proj + "']/grsname").Select(p => p.Attribute("frameName").Value.Trim()).ToArray();
            var executions = _executionConfiguration.GetAll(p => p.BelongToProject == projId && configurations.Contains(p.Name.ToLower())).OrderBy(p => p.Name).ToList();

            foreach (var execution in executions)
            {
                execution.Name =
                    xmlDoc.XPathSelectElements("//project[@name='" + proj + "']/grsname[@frameName='" + execution.Name.ToLower() + "']").First().Value.Trim();
            }

            return executions;
        }

        public TestsExecution GetAppropriateExecution(int execId)
        {
            return _testsExecutions.GetSingleOrDefault_Services(p => p.ID == execId);
        }

        public List<TestsExecution> GetExecutionHistory(string user)
        {
            return _testsExecutions.GetIncludingWithFiltering_Services(p => p.AddedBy.ToLower() == user.ToLower(),
                new Expression<Func<TestsExecution, object>>[]
                    {
                        p => p.Project, p => p.TestSuits
                    }).OrderBy(p => p.ID).ToList();
        }

        public object[] GetExecutionData(int execId)
        {
            var results = new object[9];

            var exec = _testsExecutions.GetFirstOrDefault(p => p.ID == execId);
            results[0] = exec.BelongToProject;
            results[1] = exec.Tests;
            results[2] = exec.Subscribers;
            results[3] = exec.Client;
            results[4] = exec.Browser;
            results[5] = exec.Account;
            results[6] = exec.ProfileHost;
            results[7] = exec.Configuration;
            results[8] = exec.FrameworkVersion;

            return results;
        }

        public void RemoveAllDuplicates(int id)
        {
            var ts = _testSuitRepository.GetFirstOrDefault(p => p.ID == id);
            if (ts == null) return;
            var duplicates = ts.TestCases.GroupBy(p => p.TCName).Where(p => p.Count() >= 2).ToList();
            foreach (var duplicate in duplicates)
            {
                foreach (var dup in duplicate.OrderByDescending(p => p.ID).Skip(1))
                {
                    _testCaseRepository.Delete(dup);
                }
            }
            _testCaseRepository.SaveChanges();
        }



        public void RemoveDuplicate(int id, string name)
        {
            var ts = _testSuitRepository.GetFirstOrDefault(p => p.ID == id);
            if (ts == null) return;
            var duplicates = ts.TestCases.Where(p => p.TCName == name).OrderByDescending(p => p.ID).Skip(1).ToList();
            foreach (var duplicate in duplicates)
            {
                _testCaseRepository.Delete(duplicate);
            }
            _testCaseRepository.SaveChanges();
        }

        public void RemoveAllDuplicatesForTestCycle(int id)
        {
            var testCycle = _testCycleRepository.GetFirstOrDefault(p => p.ID == id);
            if (testCycle == null) return;
            var allTestSuites =
                testCycle.TestSuits.Where(p => p.TestCases.GroupBy(z => z.TCName).Where(q => q.Count() >= 2).Count() != 0).ToList();
            foreach (var testSuite in allTestSuites)
            {
                if (testSuite.TestCases.GroupBy(p => p.TCName).Any(p => p.Count() >= 2))
                    RemoveAllDuplicates(testSuite.ID);
            }
        }

        public void MigrateTestSuite(List<int> testSuitesIds, int targetTestCycle)
        {
            foreach (var id in testSuitesIds)
            {
                var testSuite = _testSuitRepository.GetFirstOrDefault(p => p.ID == id);
                testSuite.ParentTestCycle = targetTestCycle;
            }
            _testSuitRepository.SaveChanges();
        }

        public void MergeTestSuite(List<int> testSuitesIdsToMerge)
        {
            var testCases = new List<string>();
            foreach (var id in testSuitesIdsToMerge)
            {
                var testSuite = _testSuitRepository.GetFirstOrDefault_Services(p => p.ID == id);
                if (testSuite == null)
                {
                    throw new Exception("Cannot find requested Test Set at the Database. Possibly it was removed by another user");
                }
                testSuite.TestCases.ToList().ForEach(p => testCases.Add(p.TCName));
                var duplicates = testCases.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Count != 0)
                    throw new Exception(
                        string.Format("Duplicates were found ({0}) between selected Tests Sets.", string.Join("; ", duplicates.ToArray()))
                        );
            }
            var targetTestSuiteId = testSuitesIdsToMerge.LastOrDefault();
            testSuitesIdsToMerge.Remove(targetTestSuiteId);
            var targetTestSuite = _testSuitRepository.GetFirstOrDefault(p => p.ID == targetTestSuiteId);
            foreach (var id in testSuitesIdsToMerge)
            {
                var originalTestSuite = _testSuitRepository.GetFirstOrDefault(p => p.ID == id);
                if (originalTestSuite == null)
                    throw new Exception(
                        "Cannot find requested Test Set at the Database. Possibly it was removed by another user");
                originalTestSuite.TestCases.ForEach(p => p.ParentTestSuite = targetTestSuiteId);
                if (targetTestSuite.PassedTestCases != null && originalTestSuite.PassedTestCases != null)
                {
                    targetTestSuite.PassedTestCases += originalTestSuite.PassedTestCases;
                }
                if (targetTestSuite.FailedTestCases != null && originalTestSuite.FailedTestCases != null)
                {
                    targetTestSuite.FailedTestCases += originalTestSuite.FailedTestCases;
                }
                //if (_testSuitRepository.GetFirstOrDefault_Services(p => p.ID == originalTestSuite.ID).TestCases.Count == 0)
                //{
                    _testSuitRepository.Delete(originalTestSuite);
                //}
            }

            _testSuitRepository.SaveChanges();
        }

        public void UpdateSubStepsWithSameError(int subStepId, bool allSubsteps, bool checkedStep)
        {
            var subStep = _subStepRepository.GetFirstOrDefault_Services(p => p.ID == subStepId);
            var necessaryElements = new List<SubStep>();
            if (checkedStep)
            {
                necessaryElements.AddRange(_subStepRepository.GetAll(p => p.ParentStep == subStep.ParentStep && p.SubStepValid == false && p.AnalyzedStatus == null));
            }
            if (allSubsteps)
            {
                necessaryElements.AddRange(
                    _subStepRepository.GetAll(
                        p => p.SubStepValid == false &&
                            p.AnalyzedStatus == null &&
                            p.SubStepMessage == subStep.SubStepMessage &&
                            p.TestStep.TestCase.TestSuit.ID == subStep.TestStep.TestCase.TestSuit.ID
                            ).ToList());
            }
            necessaryElements = necessaryElements.Distinct().ToList();
            foreach (var ss in necessaryElements)
            {
                var custStat =
                (new CustomStatuses.CustomStatuses(subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0,
                    _projectRepository));
                var analyzedStatusName = custStat.Statuses// CustomStatuses(subStep.TestStep.TestCase.TestSuit.ParentProject ?? 0)).Statuses
                        .FirstOrDefault(p => p.UniqueID == subStep.AnalyzedStatus).StatusName;
                var actualStatusName = "Fail";
                UpdateSubStep(ss.ID, actualStatusName, analyzedStatusName, subStep.Defects);
            }
        }

        public void UpdateTestSetsWithUser(List<int> testSets, int user)
        {
            foreach (var set in testSets)
            {
                var ts = _testSuitRepository.GetFirstOrDefault(p => p.ID == set);
                ts.Tester = user;
            }
            _testSuitRepository.SaveChanges();
        }

        public void UpdateTestSetWithUserByStep(int testStep, int user)
        {
            var ts = _subStepRepository.GetFirstOrDefault(p => p.ID == testStep).TestStep.TestCase.TestSuit.ID;
            var record = _analyser.GetFirstOrDefault(p => p.TestSet == ts && p.User == user);
            if (record == null)
            {
                _analyser.Add(new Analyser {TestSet = ts, User = user});
                _analyser.SaveChanges();
            }
        }

        public bool CheckIfTesetsBelongsSameSet(List<int> ids)
        {
            var tc = _testSuitRepository.GetAll(p => ids.Contains(p.ID)).Select(p => p.ParentTestCycle).Distinct().Count();
            return tc == 1;
        }
    }
}