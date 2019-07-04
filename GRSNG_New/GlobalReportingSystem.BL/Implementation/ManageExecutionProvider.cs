using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using GlobalReportingSystem.BL.Helper;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Enums;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.Executor;
using GlobalReportingSystem.Core.Models.GRS;
using System.Globalization;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageExecutionProvider : IManageExecutionProvider
    {
        private GlobalReportingSystem.Core.Models.GRS.DB.Node _theNode;

        private readonly ISessionHelper _sessionHelper;

        private readonly IRepository<TestsExecution> _testExecutionRepository;

        private readonly IRepository<vw_FilesStorage> _filesStorageRepository;

        private readonly IRepository<Project> _projectRepository;

        private readonly IRepository<NewLoadBalanced> _newLoadBalancedRepository;

        private readonly IRepository<LoadBalancedMachine> _loadBalancedMachineRepository;

        private readonly IRepository<LoadBalancedTest> _loadBalancedTestRepository;

        private readonly IRepository<vw_FilesStorage> _vw_FilesStorage;

        private readonly IRepository<HostsConfiguration> _hostsConfiguration;

        private readonly IRepository<AccountForTestRun> _accountForTestRun;

        private readonly IRepository<Client> _client;

        public ManageExecutionProvider(ISessionHelper sessionHelper, IRepository<TestsExecution> testExecutionRepository
            , IRepository<vw_FilesStorage> filesStorageRepository, IRepository<Project> projectRepository
            , IRepository<NewLoadBalanced> newLoadBalancedRepository, IRepository<LoadBalancedMachine> loadBalancedMachineRepository
            , IRepository<LoadBalancedTest> loadBalancedTestRepository, IRepository<vw_FilesStorage> vw_FilesStorage,
            IRepository<HostsConfiguration> hostsConfiguration, IRepository<AccountForTestRun> accountForTestRun,
            IRepository<Client> client)
        {
            _sessionHelper = sessionHelper;
            _testExecutionRepository = testExecutionRepository;
            _filesStorageRepository = filesStorageRepository;
            _projectRepository = projectRepository;
            _newLoadBalancedRepository = newLoadBalancedRepository;
            _loadBalancedMachineRepository = loadBalancedMachineRepository;
            _loadBalancedTestRepository = loadBalancedTestRepository;
            _vw_FilesStorage = vw_FilesStorage;
            _hostsConfiguration = hostsConfiguration;
            _accountForTestRun = accountForTestRun;
            _client = client;
        }

        public LinearExecutionViewModel GetLinearExecutions(IPrincipal user)
        {
            var model = new LinearExecutionViewModel();
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var linearElements = new List<TestsExecutions>();

            _testExecutionRepository.GetIncludingWithFiltering(
                p => p.Project.ID == pId && p.FilesStorage != null && p.ExecutionType == "linear",
                new Expression<Func<TestsExecution, object>>[]
                    {
                        p => p.AccountForTestRun, p => p.HostsConfiguration, p => p.Client1, p => p.TestCycle,
                        p => p.ExecutionConfiguration
                    })
            .ToList().ForEach(p => linearElements.Add(new TestsExecutions
            {
                TestsExecution = p,
                FilesStorage = _filesStorageRepository.GetSingleOrDefault(f => p.FrameworkVersion == f.FilesStorage_ID)
            }));

            model.TestsExecutions = linearElements;
            var project = _projectRepository.GetIncludingWithFiltering(p => p.ID == pId, new Expression<Func<Project, object>>[]
            {
                p => p.AccountForTestRuns, p => p.HostsConfigurations, p => p.ExecutionConfigurations,
                p => p.Clients, p => p.TestCycles
            }).First();

            model.ProjectInfo = new ProjectInfo();
            model.ProjectInfo.Project = project;
            model.ProjectInfo.FilesStorages =
                _filesStorageRepository.GetAll(p => p.FilesStorage_BelongToProject == pId).ToList();
            model.ProjectInfo.Users = project.Accesses.Select(p => p.User.UserEmail).OrderBy(p => p).ToList();

            return model;
        }

        public LoadBalancedViewModel GetLoadBalancedExecutions(IPrincipal user, int? id)
        {
            var model = new LoadBalancedViewModel();
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var items = _newLoadBalancedRepository.GetIncludingWithFiltering_Services(p => p.BelongToProject == pId,
                new Expression<Func<NewLoadBalanced, object>>[]
            {
                p => p.LoadBalancedMachines,
                p => p.LoadBalancedTests
            })
                .ToList();

            var lbItems = new List<NewLoadBalancedItem>();
            var project = _projectRepository.GetIncludingWithFiltering_Services(p => p.ID == pId, new Expression<Func<Project, object>>[]
            {
                p => p.AccountForTestRuns, p => p.HostsConfigurations, p => p.ExecutionConfigurations,
                p => p.Clients, p => p.TestCycles, p => p.FilesStorages
            }).First();

            model.ProjectInfo = project;
            if (id.HasValue)
            {
                items.Where(p => p.ID == id.Value).ToList()
                    .ForEach(p => lbItems.Add(new NewLoadBalancedItem { NewLoadBalanced = p, Single = true }));
                model.Single = true;
            }
            else
            {
                items.ForEach(p => lbItems.Add(new NewLoadBalancedItem { NewLoadBalanced = p, Single = false }));
                model.Single = false;
            }
            model.NewLoadBalancedList = lbItems;
            return model;
        }

        public NewLoadBalancedItem AddLoadBalancedExecution(IPrincipal user, int? frameworkId)
        {
            var guid = Guid.NewGuid();
            _sessionHelper.GetSessionDetails(user).Project.NewLoadBalanceds.Add(new NewLoadBalanced { FrameworkId = frameworkId, RemoteExecutionLink = guid });
            _sessionHelper.SaveChanges();
            //return new NewLoadBalancedItem
            //    {
            //        NewLoadBalanced = _newLoadBalancedRepository.GetIncluding(p => p.FilesStorage).ToList().Last(),
            //        Single = false
            //    };
            return new NewLoadBalancedItem
            {
                NewLoadBalanced = _newLoadBalancedRepository.GetFirstOrDefault_Services(p => p.RemoteExecutionLink == guid),
                Single = false
            };
        }

        public void DeleteLoadBalancedSection(IPrincipal user, int id)
        {
            var currentSession = _sessionHelper.GetSessionDetails(user);
            var newLoadBalanced = currentSession.Project.NewLoadBalanceds;
            var item = newLoadBalanced.SingleOrDefault(p => p.ID == id);
            if (item != null)
            {
                item.LoadBalancedMachines.ToList().ForEach(p => item.LoadBalancedMachines.Remove(p));
                item.LoadBalancedTests.ToList().ForEach(p => item.LoadBalancedTests.Remove(p));
                newLoadBalanced.Remove(item);
                _sessionHelper.SaveChanges();

            }
        }

        public LoadBalancedMachine AddLoadBalancedMachine(IPrincipal user, int sectionId)
        {
            var lb = new LoadBalancedMachine { NewLoadBalancedId = sectionId };
            _loadBalancedMachineRepository.Add(lb);
            _loadBalancedMachineRepository.SaveChanges();

            //return _loadBalancedMachineRepository.GetIncluding(p => p.NewLoadBalanced, p => p.TestCycle)
            //    .ToList()
            //    .Last();
            return _loadBalancedMachineRepository.GetFirstOrDefault_Services(p => p.ID == lb.ID);
        }

        public void DeleteLoadBalancedMachine(IPrincipal user, int id)
        {
            _loadBalancedMachineRepository.Delete(p => p.ID == id);
            _loadBalancedMachineRepository.SaveChanges();
        }

        public void AddTestsExecution(IPrincipal user, int frameworkId)
        {
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            _testExecutionRepository.Add(new TestsExecution
            {
                BelongToProject = pId,
                ExecutionType = "linear",
                RemoteExecutionLink = Guid.NewGuid(),
                Tests = "",
                Browser = "",
                Priroty = "",
                CategoriesExclude = "",
                CategoriesInclude = "",
                FrameworkVersion = frameworkId
            });
            _testExecutionRepository.SaveChanges();
        }

        public int AddFullExecution(string user, int projId, string tests, int frameworkId, string browser, int? client, int? account,
            int? hostConf, int? execConf, string subscribers)
        {
            var tempTests = tests.Split(',').Aggregate("", (current, test) => current + (string.Join("|",
                    GetTestsList(test, "AddAll",
                    _vw_FilesStorage.GetSingleOrDefault(p => p.FilesStorage_ID == frameworkId).FilesStorage_Comments))));

            if (!tempTests.EndsWith("|"))
                tempTests += "|";

            var activeCycle =
                _projectRepository.GetFirstOrDefault(p => p.ID == projId)
                    .TestCycles.FirstOrDefault(p => p.CycleIsCurrent == true);

            var exec = _testExecutionRepository.GetSingleOrDefault_Services(p => p.Subscribers == subscribers &&
                                                                                 p.AddedBy == user &&
                                                                                 p.BelongToProject == projId &&
                                                                                 p.Tests == tempTests &&
                                                                                 p.FrameworkVersion == frameworkId &&
                                                                                 p.Client == client &&
                                                                                 p.Account == account &&
                                                                                 p.ProfileHost == hostConf &&
                                                                                 p.Configuration == execConf &&
                                                                                 p.TargetTestCycle == activeCycle.ID &&
                                                                                 p.Browser == browser &&
                                                                                 p.ExecutionType == "linear" &&
                                                                                 p.Priroty == "" &&
                                                                                 p.CategoriesExclude == "" &&
                                                                                 p.CategoriesInclude == "");

            if (exec == null)
            {
                _testExecutionRepository.Add(new TestsExecution
                {
                    BelongToProject = projId,
                    ExecutionType = "linear",
                    RemoteExecutionLink = Guid.NewGuid(),
                    Tests = tempTests,
                    Browser = browser,
                    Priroty = "",
                    CategoriesExclude = "",
                    CategoriesInclude = "",
                    FrameworkVersion = frameworkId,
                    Client = client,
                    Account = account,
                    ProfileHost = hostConf,
                    Configuration = execConf,
                    TargetTestCycle = activeCycle == null ? 0 : activeCycle.ID,
                    AddedBy = user,
                    Subscribers = subscribers
                });
                _testExecutionRepository.SaveChanges();
                return _testExecutionRepository.GetAllToList(p => p.ID != null).Last().ID;
            }
            return exec.ID;
        }

        public void SetLoadBalancedMachine(int machineId, int clientId)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineId);
            if (machine != null) machine.MachineId = clientId;
            _loadBalancedMachineRepository.SaveChanges();
        }

        public LoadBalancedMachine GetLoadBalancedMachines(int machineId)
        {
            return _loadBalancedMachineRepository.GetIncluding(new Expression<Func<LoadBalancedMachine, object>>[]
            {
                p => p.Client, p => p.NewLoadBalanced, p => p.HostsConfiguration, p => p.AccountForTestRun,
                p => p.TestCycle
            })
                .ToList()
                .FirstOrDefault(p => p.ID == machineId);
        }

        public LoadBalancedMachine SetLoadBalancedAccount(int accountid, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null) machine.AccountId = accountid;
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == machineid,
                new Expression<Func<LoadBalancedMachine, object>>[]
                {
                    p => p.Client, p => p.NewLoadBalanced,
                    p => p.HostsConfiguration, p => p.AccountForTestRun,
                    p => p.TestCycle
                }).First();
        }

        public LoadBalancedMachine SetLoadBalancedEnvironment(int environmentid, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null) machine.EnvironmentId = environmentid;
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == machineid,
                new Expression<Func<LoadBalancedMachine, object>>[]
                {
                    p => p.Client, p => p.AccountForTestRun,
                    p => p.NewLoadBalanced, p => p.HostsConfiguration,
                    p => p.TestCycle
                }).First();
        }

        public LoadBalancedMachine SetLoadBalancedBrowser(string browser, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null) machine.Browser = browser;
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == machineid,
                new Expression<Func<LoadBalancedMachine, object>>[]
                    {
                        p => p.Client, p => p.NewLoadBalanced,
                        p => p.HostsConfiguration, p => p.AccountForTestRun,
                        p => p.TestCycle
                    }).First();
        }

        public LoadBalancedMachine SetLoadBalancedExecutionConfig(int confid, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null) machine.ExecutionConfigurationId = confid;
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == machineid,
                new Expression<Func<LoadBalancedMachine, object>>[]
                    {
                        p => p.Client, p => p.NewLoadBalanced,
                        p => p.HostsConfiguration, p => p.AccountForTestRun,
                        p => p.TestCycle
                    }).First();
        }

        public LoadBalancedMachine SetLoadBalancedTargetTc(int cycleid, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null)
            {
                if (cycleid == 0)
                    machine.TargetTestCycle = null;
                else
                {
                    machine.TargetTestCycle = cycleid;
                }
            }
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == machineid,
                new Expression<Func<LoadBalancedMachine, object>>[]
                    {
                        p => p.Client, p => p.NewLoadBalanced,
                        p => p.HostsConfiguration, p => p.AccountForTestRun,
                        p => p.TestCycle
                    }).First();
        }

        public void SetLoadBalancedPriority(string priority, int machineid)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == machineid);
            if (machine != null)
            {
                if (string.IsNullOrEmpty(priority))
                    machine.Priority = null;
                else
                    machine.Priority = priority;
            }
            _loadBalancedMachineRepository.SaveChanges();
        }

        public void AddLoadBalancedTests(int sectionId, string tests, string key)
        {
            var section = _newLoadBalancedRepository.GetFirstOrDefault(p => p.ID == sectionId);
            foreach (var test in tests.Split(','))
            {
                if (section != null)
                {
                    var pathes = GetTestsList(test, key, section.FilesStorage.Comments);
                    foreach (var path in pathes)
                    {
                        section.LoadBalancedTests.Add(new LoadBalancedTest
                        {
                            Status = "Ready",
                            TestName = path
                        });
                    }
                }
            }
            _newLoadBalancedRepository.SaveChanges();
        }

        private List<string> GetTestsList(string path, string key, string fs)
        {
            path = path.Replace("__", ".");

            var manyTests = new List<string>();
            var outTests = new List<string>();
            var tcAddType = (TestsToExecute)Enum.Parse(typeof(TestsToExecute), key, true);
            var tests = new NunitTests();
            var arrayOfTests = new List<string>();

            switch (tcAddType)
            {
                case TestsToExecute.AddAll:
                    return new List<string> { path };
                case TestsToExecute.AddAllTestCases:
                    if (fs != null) tests = tests.Deserialize(fs);
                    arrayOfTests =
                        GetArrayOfTests(tests).OrderBy(p => p).Where(z => z.Contains(path + ".")).ToList();
                    outTests.AddRange(arrayOfTests.Distinct());
                    return outTests.Distinct().ToList();
                case TestsToExecute.AddAllTestSets:
                    if (fs != null) tests = tests.Deserialize(fs);
                    var tempList = new List<string>();
                    arrayOfTests =
                        GetArrayOfTests(tests).OrderBy(p => p).Where(z => z.Contains(path + ".")).ToList();
                    arrayOfTests.ForEach(tempList.Add);
                    foreach (var arrayOfTest in arrayOfTests)
                    {
                        manyTests.Add(arrayOfTest.Replace("." + arrayOfTest.Split('.').Last(), ""));
                    }
                    outTests.AddRange(manyTests.Distinct());
                    return outTests.Distinct().ToList();
                default:
                    return new List<string>();
            }
        }

        public List<string> StopExecution(int executionId, string type)
        {
            var execution = new List<string>();
            switch (type)
            {
                case "loadballanced":
                    execution.AddRange(_newLoadBalancedRepository.GetSingleOrDefault(p => p.ID == executionId)
                            .LoadBalancedMachines.Select(p => p.Client.RemoteMachineIP).ToList());
                    ;
                    break;
                case "linear":
                    var client1 = _testExecutionRepository.GetSingleOrDefault(p => p.ID == executionId)
                        .Client1;
                    if (client1 == null)
                    {
                        throw new Exception(Messages.AddMachineIpMessage);
                    }
                    execution.Add(client1.RemoteMachineIP);
                    break;
            }
            return execution;
        }

        public void DeleteLinearElement(int id)
        {
            var exe = _testExecutionRepository.GetFirstOrDefault(p => p.ID == id);
            _testExecutionRepository.Delete(exe);
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearMachine(int clientid, int executionid)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.Client = clientid;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearBrowser(string browser, int executionid)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.Browser = browser;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearExecutionConfiguration(int confid, int executionid)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.Configuration = confid;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearEnvironment(int environmentid, int executionid)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.ProfileHost = environmentid;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearTestCycle(int cycleid, int executionid)
        {
            //var emptyCycles = _testExecutionRepository.GetAll(p => p.ID != null).ToList();

            //foreach (var cycle in emptyCycles)
            //{
            //    var tcycle =
            //        cycle.TestCycle;//.Project.TestCycles.SingleOrDefault(p => p.CycleIsCurrent.HasValue && p.CycleIsCurrent.Value);
            //    if (tcycle == null && cycle.TargetTestCycle != null)
            //    {
            //        var id = cycle.Project.TestCycles.SingleOrDefault(p => p.CycleIsCurrent.HasValue && p.CycleIsCurrent.Value);
            //        if(id!=null)
            //        cycle.TargetTestCycle = id.ID;
            //    }
            //}

            //_testExecutionRepository.SaveChanges();

            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.TargetTestCycle = cycleid;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetLinearAccount(int accountid, int executionid)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                execution.Account = accountid;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void AddLiearTests(int executionId, string tests, string key)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionId);
            if (execution != null)
            {
                foreach (var test in tests.Split(','))
                {
                    execution.Tests += string.Join("|", GetTestsList(test, key, execution.FilesStorage.Comments)) + "|";
                }
            }
            _testExecutionRepository.SaveChanges();
        }

        public void DeleteLinearTest(int executionid, string testname)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionid);
            if (execution != null)
            {
                var tests = execution.Tests;
                int pos = tests.IndexOf(testname);
                if (pos >= 0)
                {
                    execution.Tests =
                        (tests.Substring(0, pos) + tests.Substring(pos + testname.Length))
                            .Replace("||", "|").Trim('|');

                    //execution.Tests = execution.Tests.Replace("|" + testname, "").Replace(testname + "|", "").Replace(testname, "");
                }
            }
            _testExecutionRepository.SaveChanges();
        }

        public string[] GetCategoriesForFramework(int id)
        {
            var categoriesDistinct = new List<string>();
            var framework = _filesStorageRepository.GetFirstOrDefault(p => p.FilesStorage_ID == id);
            if (framework != null)
            {
                var tests = new NunitTests();
                tests = tests.Deserialize(framework.FilesStorage_Comments); var categories = new List<string>();
                tests.TestFixtures.Select(p => p.Categories).ToList().ForEach(z => z.ForEach(categories.Add));
                categoriesDistinct.AddRange(categories.Distinct());
            }
            return categoriesDistinct.ToArray();
        }

        public LoadBalancedMachine SetCategoriesForMachine(int id, string categories, bool include)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == id);
            if (machine != null)
            {
                if (string.IsNullOrEmpty(categories)) categories = null;
                if (include)
                {
                    machine.Include = categories;
                }
                else
                {
                    machine.Exclude = categories;
                }
            }
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == id,
                new Expression<Func<LoadBalancedMachine, object>>[]
                    {
                        p => p.Client, p => p.NewLoadBalanced,
                        p => p.HostsConfiguration, p => p.AccountForTestRun,
                        p => p.TestCycle
                    }).First();
        }

        public List<string> GetCategoryForMachine(int id, bool include)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault_Services(p => p.ID == id);
            var selectedCategories = new List<string>();
            if (machine != null)
            {
                if (include)
                {
                    selectedCategories.AddRange(machine.Include.Split(','));
                }
                else
                {
                    selectedCategories.AddRange(machine.Exclude.Split(','));
                }
            }
            return selectedCategories;
        }

        public LoadBalancedMachine SetPriorityForMachine(int id, string priorities)
        {
            var machine = _loadBalancedMachineRepository.GetFirstOrDefault(p => p.ID == id);
            if (machine != null)
            {
                if (string.IsNullOrEmpty(priorities)) priorities = null;
                machine.Priority = priorities;
            }
            _loadBalancedMachineRepository.SaveChanges();

            return _loadBalancedMachineRepository.GetIncludingWithFiltering(p => p.ID == id,
                new Expression<Func<LoadBalancedMachine, object>>[]
                    {
                        p => p.Client, p => p.NewLoadBalanced,
                        p => p.HostsConfiguration, p => p.AccountForTestRun,
                        p => p.TestCycle
                    }).First();
        }

        public void SetPriorityForExecution(int id, string priorities)
        {
            var machine = _testExecutionRepository.GetFirstOrDefault(p => p.ID == id);
            if (machine != null)
            {
                machine.Priroty = priorities;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetSubscriberForExecution(int id, string subscribers)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == id);
            if (execution != null)
            {
                execution.Subscribers = subscribers;
            }
            _testExecutionRepository.SaveChanges();
        }

        public void SetCategotyForExecution(int id, string categories, bool include)
        {
            var machine = _testExecutionRepository.GetFirstOrDefault(p => p.ID == id);
            if (machine != null)
            {
                if (include)
                {
                    machine.CategoriesInclude = categories;
                }
                else
                {
                    machine.CategoriesExclude = categories;
                }
            }
            _testExecutionRepository.SaveChanges();
        }
        public List<string> GetCategoryForExecution(int id, bool include)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault_Services(p => p.ID == id);
            var selectedCategories = new List<string>();
            if (execution != null)
            {
                if (include)
                {
                    selectedCategories.AddRange(execution.CategoriesInclude.Split(','));
                }
                else
                {
                    selectedCategories.AddRange(execution.CategoriesExclude.Split(','));
                }
            }
            return selectedCategories;
        }

        public NodesAndId GetTestsForFramework(int id)
        {
            var nodeAndId = new NodesAndId();

            var fw = _filesStorageRepository.GetSingleOrDefault(p => p.FilesStorage_ID == id);
            if (fw != null)
            {
                nodeAndId.id = id;
                var tests = new NunitTests();
                tests = tests.Deserialize(fw.FilesStorage_Comments);
                var arrayOfTests = GetArrayOfTests(tests).OrderBy(p => p);
                nodeAndId.Node = TreeBuilder.BuildTree(arrayOfTests, new[] { '.' }).Children.First();
            }
            else
            {
                nodeAndId = null;
            }
            return nodeAndId;
        }

        private static IEnumerable<string> GetArrayOfTests(NunitTests tests)
        {
            return (from testFix in tests.TestFixtures from testcase in testFix.Tests select testFix.Namespace + "." + testFix.Name + "." + testcase).ToList();
        }

        public NodesAndId GetChildren(int id, string parent)
        {
            var nodeAndId = new NodesAndId();
            var fw = _filesStorageRepository.GetSingleOrDefault(p => p.FilesStorage_ID == id);
            if (fw != null)
            {
                nodeAndId.id = id;
                var tests = new NunitTests();
                tests = tests.Deserialize(fw.FilesStorage_Comments);
                var arrayOfTests = new List<string>();
                foreach (var testFix in tests.TestFixtures.OrderBy(p => p.Namespace))
                {
                    foreach (var testcase in testFix.Tests.OrderBy(p => p))
                    {
                        arrayOfTests.Add(testFix.Namespace + "." + testFix.Name + "." + testcase);
                    }
                }
                arrayOfTests = arrayOfTests.OrderBy(p => p).ToList();
                var nodes = TreeBuilder.BuildTree(arrayOfTests, new[] { '.' }).Children.First();
                var searchterm = parent.Split(new string[] { "__" }, StringSplitOptions.None);
                _theNode = null;
                foreach (var level in searchterm.Skip(1))
                {
                    SearchIterative(nodes, level);
                    nodes = _theNode;
                }

                nodeAndId.Node = nodes;
            }
            else
            {
                nodeAndId = null;
            }
            return nodeAndId;
        }

        private void SearchIterative(Core.Models.GRS.DB.Node tN, string term)
        {
            foreach (var n in tN.Children)
            {
                if (n.Value == term)
                {
                    _theNode = n;
                    return;
                }
                SearchIterative(n, term);
            }
        }

        public int GetLinearIdByRel(string rel, string vm, string env, string account = "")
        {
            try
            {
                Guid relGuid = Guid.Parse(rel);
                var execution = _testExecutionRepository.GetSingleOrDefault(p => p.RemoteExecutionLink == relGuid);
                if (execution != null)
                {
                    if (!string.IsNullOrEmpty(env) && execution.HostsConfiguration.EnvironmentName.ToLower() != env.ToLower())
                    {
                        execution.ProfileHost =
                            _hostsConfiguration.GetSingleOrDefault(p => p.BelongToProject == execution.BelongToProject
                                                                        && p.EnvironmentName.ToLower() == env.ToLower())
                                .ID;
                    }
                    if (!string.IsNullOrEmpty(account) && execution.AccountForTestRun.AccountName.ToLower() != account.ToLower())
                    {
                        execution.Account =
                            _accountForTestRun.GetSingleOrDefault(p => p.BelongToProject == execution.BelongToProject
                                                                        && p.AccountName.ToLower() == account.ToLower())
                                .ID;
                    }
                    if (!string.IsNullOrEmpty(vm) && execution.Client1.RemoteMachineIP.ToLower() != vm.ToLower())
                    {
                        execution.Client =
                            _client.GetSingleOrDefault(p => p.BelongToProject == execution.BelongToProject
                                                                        && p.RemoteMachineIP.ToLower() == vm.ToLower())
                                .ID;
                    }
                    _testExecutionRepository.SaveChanges();
                    return execution.ID;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int GetLoadBalanceIdByRel(string rel)
        {
            try
            {
                Guid relGuid = Guid.Parse(rel);
                var execution = _newLoadBalancedRepository.GetSingleOrDefault(p => p.RemoteExecutionLink == relGuid);
                if (execution != null)
                    return execution.ID;
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public void SubmitResponse(string message, bool error)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.StatusCode = (error ? 500 : 200);
            HttpContext.Current.Response.AddHeader("Content-Type", "text/plain");
            HttpContext.Current.Response.Write(message);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        public void DeleteLbTests(List<string> testsList)
        {
            var testsListToInt = new List<int>();
            testsList.ForEach(p => testsListToInt.Add(int.Parse(p)));
            _loadBalancedTestRepository.Delete(p => testsListToInt.Contains(p.ID));
            _loadBalancedTestRepository.SaveChanges();
        }

        public void ResetLbTests(List<string> testsList)
        {
            var testsListToInt = new List<int>();
            testsList.ForEach(p => testsListToInt.Add(int.Parse(p)));
            foreach (var test in testsListToInt)
            {
                var lbTest = _loadBalancedTestRepository.GetFirstOrDefault(p => p.ID == test);
                lbTest.StartedAt = null;
                lbTest.Status = "Ready";
                lbTest.FinishedAt = null;
                lbTest.MachineIP = null;

                _loadBalancedTestRepository.SaveChanges();
            }
        }

        public string[] GetLoadBalancedMashines(int executionId)
        {
            return _newLoadBalancedRepository.GetSingleOrDefault(p => p.ID == executionId)
                .LoadBalancedMachines.Select(p => p.Client.RemoteMachineIP).ToArray();
        }
    }
}
