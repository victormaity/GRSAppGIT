using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.GRS.PublicAccess;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageTestCycleProvider
    {
        bool IsTestCycleExist(int id);

        bool IsUserHasAccess(int user, int project);

        void SwitchProject(IPrincipal user, Project project);

        TestCycle GetTestCycle(int id);

        List<ItemId> GetTestSuitsByKey(string key, int cycleId);

        List<ItemId> GetTestSuitsByKeyAndIp(string ip, int cycleId, string testSuitName);

        TestCasesAndStepsModel GetTsLegend(int id, TestCasesAndStepsModel model);

        TestCasesAndStepsModel GetTestSuite(int id, string filter);

        TestCasesAndStepsModel GetTestSuiteAndLegend(int id, string filter);

        TestStepAndProject GetSubSteps(int id);

        TestCasesAndProject GetTestCases(int id);

        void UpdateSubStep(int id, string state, string analyzedStatus, string defects, int userId);

        void UpdateTestSetWithUserByStep(int testStep, int user);

        void DeleteTestSuites(List<string> tsIds);

        void DeleteTestCase(int id);

        void DeleteOldDefects(int stepId, string defectId);

        void UpdateTestSetsWithUser(List<int> testSets, int user);

        string GetTestSuiteName(int id);

        List<int> GetSelectedTestSuitIds(int cycleId, List<string> tsInclude, List<string> ipInclude, List<string> fnInclude, bool selectAll,
            List<string> tsExclude, List<string> ipExclude, List<string> fnExclude);

        List<Project> GetPublicAcessProjects();

        List<Client> GetAppropriateMachines(int projId);

        int GetAppropriateTests(int projId);

        List<HostsConfiguration> GetAppropriateEnvironments(int projId);

        List<AccountForTestRun> GetAppropriateAccounts(int projId);

        List<User> GetAppropriateUsers(int projId);

        List<ExecutionConfiguration> GetAppropriateConfigurations(int projId);

        FullConfigurationModel GetFullConfigurationModel(int projId);

        TestsExecution GetAppropriateExecution(int execId);

        List<TestsExecution> GetExecutionHistory(string user);


        object[] GetExecutionData(int execId);
        void RemoveAllDuplicates(int id);
        void RemoveDuplicate(int id, string name);
        void RemoveAllDuplicatesForTestCycle(int id);
        void MigrateTestSuite(List<int> testSuitesIds, int targetTestCycle);
        void MergeTestSuite(List<int> testSuitesIdsToMerge);
        void AddCommentToTestSuite(int id, string value);
        void UpdateSubStepsWithSameError(int subStepId, bool allSubsteps, bool checkedStep);
        bool CheckIfTesetsBelongsSameSet(List<int> ids);
        ItemId GetTestSuitDetails(string id);
        List<string> GetTestSuitesIdsByNames(int cycleId, List<string> tsNames);
        List<string> GetTestSuitesIdsByIP(int cycleId, List<string> tsNamesAndIps);
    }
}
