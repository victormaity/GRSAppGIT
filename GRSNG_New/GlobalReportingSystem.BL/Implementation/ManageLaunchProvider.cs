using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.BL.Helper;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Enums;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using Microsoft.Practices.ServiceLocation;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageLaunchProvider : IManageLaunchProvider
    {
        private Core.Models.GRS.DB.Node _theNode;
        private readonly ISessionHelper _sessionHelper;
        private readonly IRepository<TestsExecution> _testExecutionRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<NewLoadBalanced> _newLoadBalancedRepository;
        private readonly IRepository<Client> _client;
        private readonly IRepository<Feature> _feature;
        private readonly IRepository<FeatureTag> _featureTag;
        private readonly IRepository<RelExecutionStatusLog> _relExecutionStatusLog;

        public ManageLaunchProvider(ISessionHelper sessionHelper, IRepository<TestsExecution> testExecutionRepository,
            IRepository<Project> projectRepository, IRepository<NewLoadBalanced> newLoadBalancedRepository, IRepository<Client> client,
            IRepository<Feature> feature, IRepository<FeatureTag> featureTag, IRepository<RelExecutionStatusLog> relExecutionStatusLog)
        {
            _sessionHelper = sessionHelper;
            _testExecutionRepository = testExecutionRepository;
            _projectRepository = projectRepository;
            _newLoadBalancedRepository = newLoadBalancedRepository;
            _client = client;
            _feature = feature;
            _featureTag = featureTag;
            _relExecutionStatusLog = relExecutionStatusLog;
        }

        public LinearExecutionLaunchModel GetLinearExecutions(IPrincipal user, string path)
        {
            var model = new LinearExecutionLaunchModel();
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var pName = _sessionHelper.GetSessionDetails(user).Project.ProjectName;
            var linearElements = new List<TestsExecutions>();

            _testExecutionRepository.GetIncludingWithFiltering(
                p => p.Project.ID == pId && p.ExecutionType == "linear",
                new Expression<Func<TestsExecution, object>>[]
                    {
                        p => p.AccountForTestRun, p => p.HostsConfiguration, p => p.Client1, p => p.TestCycle,
                        p => p.ExecutionConfiguration
                    }).OrderByDescending(x => x.ID)
            .ToList().ForEach(p => linearElements.Add(new TestsExecutions
            {
                TestsExecution = p
            }));

            model.TestsExecutions = linearElements;
            var project = _projectRepository.GetIncludingWithFiltering(p => p.ID == pId, new Expression<Func<Project, object>>[]
            {
                p => p.AccountForTestRuns, p => p.HostsConfigurations, p => p.ExecutionConfigurations,
                p => p.Clients, p => p.TestCycles
            }).First();



            model.ProjectDetails = new ProjectDetails
            {
                IsFrameworkSynced = File.Exists(path + @"\TestsStructure\" + project.ProjectName + ".xml"),
                Users = project.Accesses.Select(p => p.User.UserEmail).OrderBy(p => p).ToList(),
                Project = project,
                IsJava = File.Exists(@"D:\GRS\SVN_NEW2\" + pName + @"\pom.xml")
            };

            return model;
        }


        public LoadBalancedLaunchModel GetLoadBalancedExecutions(IPrincipal user, int? id, string path)
        {
            var model = new LoadBalancedLaunchModel();
            var pId = _sessionHelper.GetSessionDetails(user).Project.ID;
            var pName = _sessionHelper.GetSessionDetails(user).Project.ProjectName;
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

            model.ProjectInfo = new ProjectDetails
            {
                IsFrameworkSynced = File.Exists(path + @"\TestsStructure\" + project.ProjectName + ".xml"),
                Users = project.Accesses.Select(p => p.User.UserEmail).OrderBy(p => p).ToList(),
                Project = project,
                IsJava = File.Exists(@"D:\GRS\SVN_NEW2\" + pName + @"\pom.xml")
            };

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


        public void AddTestsExecution(IPrincipal user)
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
                FrameworkVersion = null,
                isNew = true
            });
            _testExecutionRepository.SaveChanges();
        }

        public NodesAndId GetTests(IPrincipal user, string path)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var nodeAndId = new NodesAndId();
            var file = path + @"\TestsStructure\" + project.ProjectName + ".xml";
            if (!File.Exists(file)) throw new Exception("Cant find stored tests file." + file);
            var fw = new ExecutionTestSuiteModel().Deserialize(File.ReadAllText(file));
            if (fw != null)
            {

                var arrayOfTests = GetArrayOfTests(fw).OrderBy(p => p);
                nodeAndId.Node = TreeBuilder.BuildTree(arrayOfTests, new[] { '.' }).Children.First();
            }
            else
            {
                nodeAndId = null;
            }
            return nodeAndId;
        }

        public List<string> GetCategories(IPrincipal user, string path)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var cats = new List<string>();
            var file = path + @"\TestsStructure\" + project.ProjectName + ".xml";
            if (!File.Exists(file)) throw new Exception("Cant find stored tests file." + file);
            var fw = new ExecutionTestSuiteModel().Deserialize(File.ReadAllText(file));
            if (fw != null)
            {
                cats.AddRange(fw.ExecutionTestSuites.SelectMany(p => p.Categories).OrderBy(p => p).ToList());
            }
            else
            {
                cats = null;
            }
            return cats.Distinct().ToList();
        }

        public NodesAndId GetChildren(IPrincipal user, string path, string parent)
        {
            var project = _sessionHelper.GetSessionDetails(user).Project;
            var nodeAndId = new NodesAndId();
            var file = path + @"\TestsStructure\" + project.ProjectName + ".xml";
            if (!File.Exists(file)) return null;
            var fw = new ExecutionTestSuiteModel().Deserialize(File.ReadAllText(file));
            if (fw != null)
            {
                var tests = fw;
                var arrayOfTests = new List<string>();
                foreach (var testFix in tests.ExecutionTestSuites.OrderBy(p => p.Namespace))
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

        public void AddLiearTests(int executionId, string tests, string key, string path, bool isJava)
        {
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionId);
            if (execution != null)
            {
                foreach (var test in tests.Split(','))
                {
                    execution.Tests += string.Join("|", GetTestsList(test, key, File.ReadAllText(path + @"\TestsStructure\" + execution.Project.ProjectName + ".xml"), isJava)) + "|";
                }
            }
            _testExecutionRepository.SaveChanges();
        }

        public void AddLinearCucumberTests(int executionId, string featuresStr, string tagsStr)
        {
            var features = featuresStr.Replace("@", "").Split(',');
            var tags = tagsStr.Replace("@", "").Split(',');
            var execution = _testExecutionRepository.GetFirstOrDefault(p => p.ID == executionId);
            string existedFeatures = String.Empty;
            string existedTags = String.Empty;
            if (execution.Tests.Contains("&&"))
            {
                existedFeatures = execution.Tests.Split(new string[] { "&&" }, StringSplitOptions.None)[0];
                existedTags = execution.Tests.Split(new string[] { "&&" }, StringSplitOptions.None)[1];
            }
            else if (!String.IsNullOrWhiteSpace(execution.Tests))
            {
                existedFeatures = execution.Tests;
            }

            if (execution != null)
            {
                if (features.Length > 0 && features.Count(p => !String.IsNullOrEmpty(p)) > 0)
                {
                    var prefix = existedFeatures.Length > 0 ? "|" : String.Empty;
                    existedFeatures += prefix + string.Join("|", features);
                }
                if (tags.Length > 0 && tags.Count(p => !String.IsNullOrEmpty(p)) > 0)
                {
                    var prefix = existedTags.Length > 0 ? "|" : String.Empty;
                    existedTags += prefix + string.Join("|", tags);
                }
               // execution.Tests = String.Concat(existedFeatures, String.IsNullOrWhiteSpace(existedTags) ? String.Empty : "&&", existedTags);
                execution.Tests = String.Concat(existedFeatures, String.IsNullOrWhiteSpace(existedTags) ? String.Empty : "", existedTags);

            }
            _testExecutionRepository.SaveChanges();
        }

        public void AddLoadBalancedTests(int executionId, string tests, string key, string path, bool isJava)
        {
            var section = _newLoadBalancedRepository.GetFirstOrDefault(p => p.ID == executionId);
            foreach (var test in tests.Split(','))
            {
                if (section != null)
                {
                    var testPaths = GetTestsList(test, key, File.ReadAllText(path + @"\TestsStructure\" + section.Project.ProjectName + ".xml"), isJava);
                    foreach (var testPath in testPaths)
                    {
                        section.LoadBalancedTests.Add(new LoadBalancedTest
                        {
                            Status = "Ready",
                            TestName = testPath
                        });
                    }
                }
            }
            _newLoadBalancedRepository.SaveChanges();
        }

        private IEnumerable<string> GetTestsList(string path, string key, string fs, bool isJava)
        {
            string testName = path.Split(new string[] { "__" }, StringSplitOptions.None).Last();
            path = path.Replace("__", ".");

            var manyTests = new List<string>();
            var outTests = new List<string>();
            var tcAddType = (TestsToExecute)Enum.Parse(typeof(TestsToExecute), key, true);
            var tests = new ExecutionTestSuiteModel();
            var arrayOfTests = new List<string>();

            if (!isJava)
            {
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
            return new List<string> { testName };
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

        private static IEnumerable<string> GetArrayOfTests(ExecutionTestSuiteModel tests)
        {
            return (from testFix in tests.ExecutionTestSuites from testcase in testFix.Tests select testFix.Namespace + "." + testFix.Name + "." + testcase).ToList();
        }

        public List<string> GetMachineIp(int executionId, string type)
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

        public NewLoadBalancedItem AddLoadBalancedExecution(IPrincipal user)
        {
            var framework =
                _sessionHelper.GetSessionDetails(user).Project.FilesStorages.SingleOrDefault(p => p.Name == "From_SVN");
            int? frameworkId = null;
            if (framework != null)
                frameworkId = framework.ID;
            _sessionHelper.GetSessionDetails(user)
                .Project.NewLoadBalanceds.Add(new NewLoadBalanced
                {
                    FrameworkId = frameworkId,
                    RemoteExecutionLink = Guid.NewGuid()
                });
            _sessionHelper.SaveChanges();

            if (framework != null)
                return new NewLoadBalancedItem
                {
                    NewLoadBalanced = _newLoadBalancedRepository.GetIncluding(p => p.FilesStorage).ToList().Last(),
                    Single = false
                };
            return new NewLoadBalancedItem
            {
                NewLoadBalanced = _newLoadBalancedRepository.GetAllToList(p => p.ID != null).Last(),
                Single = false
            };
        }

        public string GetMachineIpById(int imageId)
        {
            return _client.GetSingleOrDefault(p => p.ID == imageId).RemoteMachineIP;
        }

        /*--------------SUNDRAM KUMAR--------------*/

        public List<Feature> GetAllFeatures(int ProjectId)
        {
            return _feature.GetAllToList(F => F.ProjectId == ProjectId).ToList();
        }

        public bool DeleteFeatureFromDBByProjectId(int ProjectId)
        {
            try
            {
                _feature.Delete(F => F.ProjectId == ProjectId);
                _feature.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool DeleteFeatureFromDBById(Int64 Id)
        {
            try
            {
                _feature.Delete(F => F.ID == Id);
                _feature.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool DeleteFeatureFromDBById(List<Int64> Ids)
        {
            try
            {
                foreach (var delid in Ids)
                {
                    _feature.Delete(F => F.ID == delid);
                    _feature.SaveChanges();
                }
                return true;
            }
            catch { return false; }
        }

        public bool AddFeaturesToDB(List<Feature> featureList)
        {
            try
            {
                foreach (var item in featureList)
                {
                    Feature newFeatureItem = new Feature()
                    {
                        Path = item.Path,
                        Name = item.Name,
                        ProjectId = item.ProjectId
                    };
                    _feature.Add(newFeatureItem);
                    _feature.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTags(List<string> TagList, int ProjectId)
        {
            try
            {
                string Tags = string.Join(",", TagList);
                var taglst = _featureTag.GetAllToList(FT => FT.ProjectId == ProjectId);
                if (taglst.Count == 0)
                {
                    //Add Tag
                    FeatureTag ftag = new FeatureTag() { ProjectId = ProjectId, Tags = Tags };
                    _featureTag.Add(ftag);
                    _featureTag.SaveChanges();
                    return true;
                }
                else if (taglst.Count == 1)
                {
                    //Update Tag
                    var ftagitem = ServiceLocator.Current.GetInstance<IRepository<FeatureTag>>();
                    Int64 rowid = (from x in taglst select x.ID).FirstOrDefault();
                    var data = ftagitem.GetSingleOrDefault(p => p.ID == rowid && p.ProjectId == ProjectId);
                    data.Tags = Tags;
                    ftagitem.SaveChanges();
                    return true;
                }
                else
                {
                    //Delete All and Add New
                    _featureTag.Delete(F => F.ProjectId == ProjectId);
                    _feature.SaveChanges();
                    FeatureTag ftag = new FeatureTag() { ProjectId = ProjectId, Tags = Tags };
                    _featureTag.Add(ftag);
                    _featureTag.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAllTagsByProjectId(int ProjectId)
        {
            var tagdata = _featureTag.GetFirstOrDefault(FT => FT.ProjectId == ProjectId);
            if (tagdata != null)
            {
                string tags = tagdata.Tags;
                if (!string.IsNullOrEmpty(tags))
                {
                    string[] tagArray = tags.Split(',');
                    List<string> tagList = new List<string>(tagArray);
                    List<string> returnList = (from x in tagList where x != null || x != "" select x).Distinct().ToList();
                    if (returnList.Count > 0)
                    {
                        return returnList;
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
                else
                {
                    return new List<string>();
                }
            }
            else
            {
                return new List<string>();
            }
        }

        public void CreateRelLogs(RelExecutionStatusLog logs)
        {
            _relExecutionStatusLog.Add(logs);
            _relExecutionStatusLog.SaveChanges();
        }

        public string ValidateRelExecutionBeforeSendRequest(int testsExecutionID)
        {
            var testExecution = _testExecutionRepository.GetFirstOrDefault(FT => FT.ID == testsExecutionID);
            if (testExecution != null)
            {
                string belongToProject = Convert.ToString(testExecution.BelongToProject);
                string client = Convert.ToString(testExecution.Client);
                string tests = Convert.ToString(testExecution.Tests);
                string account = Convert.ToString(testExecution.Account);
                string profileHost = Convert.ToString(testExecution.ProfileHost);
                //string browser = Convert.ToString(testExecution.Browser);
                string remoteExecutionLink = Convert.ToString(testExecution.RemoteExecutionLink);
                string executionType = Convert.ToString(testExecution.ExecutionType);
                string targetTestCycle = Convert.ToString(testExecution.TargetTestCycle);

                if (!string.IsNullOrEmpty(belongToProject) && !string.IsNullOrEmpty(client) && !string.IsNullOrEmpty(tests) && !string.IsNullOrEmpty(account) &&
                    !string.IsNullOrEmpty(profileHost) 
                    //&& !string.IsNullOrEmpty(browser) 
                    && !string.IsNullOrEmpty(remoteExecutionLink) &&
                    !string.IsNullOrEmpty(executionType) && !string.IsNullOrEmpty(targetTestCycle))
                {
                    return "";
                }
                else
                {
                    string message = "Execution link is not fully configured. please select";

                    if (string.IsNullOrEmpty(client))
                    {
                        message = message + " Machine,";
                    }
                    if (string.IsNullOrEmpty(tests))
                    {
                        message = message + " Tests,";
                    }
                    //if (string.IsNullOrEmpty(browser))
                    //{
                    //    message = message + " Browser,";
                    //}
                    if (string.IsNullOrEmpty(profileHost))
                    {
                        message = message + " Envornment,";
                    }
                    if (string.IsNullOrEmpty(account))
                    {
                        message = message + " Account,";
                    }
                    if (string.IsNullOrEmpty(targetTestCycle))
                    {
                        message = message + " Target Test Cycle,";
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        message = message.Substring(0, message.Length - 1);
                        message = message + ".";
                    }
                    return message;
                }
            }
            else
            {
                return "Test execution does not exist.";
            }
        }

        /*-----------------------------------------*/



    }
}
