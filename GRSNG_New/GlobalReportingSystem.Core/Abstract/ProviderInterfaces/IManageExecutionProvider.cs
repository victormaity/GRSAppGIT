using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageExecutionProvider
    {
        LinearExecutionViewModel GetLinearExecutions(IPrincipal user);

        LoadBalancedViewModel GetLoadBalancedExecutions(IPrincipal user, int? id);

        LoadBalancedMachine GetLoadBalancedMachines(int machineId);

        NewLoadBalancedItem AddLoadBalancedExecution(IPrincipal user, int? frameworkId);

        LoadBalancedMachine AddLoadBalancedMachine(IPrincipal user, int id);

        void AddTestsExecution(IPrincipal user, int frameworkId);

        void DeleteLoadBalancedSection(IPrincipal user, int id);

        void DeleteLoadBalancedMachine(IPrincipal user, int id);

        void SetLoadBalancedMachine(int machineId, int clientId);

        LoadBalancedMachine SetLoadBalancedAccount(int accountid, int machineid);

        LoadBalancedMachine SetLoadBalancedEnvironment(int environmentid, int machineid);

        LoadBalancedMachine SetLoadBalancedBrowser(string browser, int machineid);

        LoadBalancedMachine SetLoadBalancedExecutionConfig(int confid, int machineid);

        LoadBalancedMachine SetLoadBalancedTargetTc(int cycleid, int machineid);

        void SetLoadBalancedPriority(string priority, int machineid);
        
        void AddLoadBalancedTests(int sectionId, string tests, string key);

        List<string> StopExecution(int executionId, string type);

        void DeleteLinearElement(int id);

        void SetLinearMachine(int clientid, int executionid);

        void SetLinearBrowser(string browser, int executionid);

        void SetLinearExecutionConfiguration(int confid, int executionid);

        void SetLinearEnvironment(int environmentid, int executionid);

        void SetLinearTestCycle(int cycleid, int executionid);

        void SetLinearAccount(int accountid, int executionid);

        void AddLiearTests(int executionId, string tests, string key);

        int AddFullExecution(string user, int projId, string tests, int frameworkId, string browser, int? client, int? account,
            int? hostConf, int? execConf, string subscribers);

        void DeleteLinearTest(int executionid, string testname);

        string[] GetCategoriesForFramework(int id);

        LoadBalancedMachine SetCategoriesForMachine(int id, string categories, bool include);

        LoadBalancedMachine SetPriorityForMachine(int id, string priorities);

        void SetPriorityForExecution(int id, string priorities);

        void SetSubscriberForExecution(int id, string subscribers);

        void SetCategotyForExecution(int id, string categories, bool include);

        NodesAndId GetTestsForFramework(int id);

        NodesAndId GetChildren(int id, string parent);

        int GetLinearIdByRel(string rel, string vm, string env, string account = "");

        int GetLoadBalanceIdByRel(string rel);

        void SubmitResponse(string message, bool error);

        void DeleteLbTests(List<string> testsList);

        void ResetLbTests(List<string> testsList);

        string[] GetLoadBalancedMashines(int executionId);

        List<string> GetCategoryForExecution(int id, bool include);

        List<string> GetCategoryForMachine(int id, bool include);
    }
}
