using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageLaunchProvider
    {
        LinearExecutionLaunchModel GetLinearExecutions(System.Security.Principal.IPrincipal user, string path);
        LoadBalancedLaunchModel GetLoadBalancedExecutions(System.Security.Principal.IPrincipal user, int? id, string path);
        void AddTestsExecution(IPrincipal user);
        NodesAndId GetTests(IPrincipal user, string path);
        NodesAndId GetChildren(IPrincipal user, string path, string parent);
        void AddLiearTests(int executionId, string tests, string key, string path, bool isJava);
        void AddLoadBalancedTests(int executionId, string tests, string key, string path, bool isJava);
        List<string> GetMachineIp(int executionId, string type);
        string GetMachineIpById(int imageId);
        NewLoadBalancedItem AddLoadBalancedExecution(IPrincipal user);
        void AddLinearCucumberTests(int executionId, string features, string tags);
        List<string> GetCategories(IPrincipal user, string path);
        /*--------------SUNDRAM KUMAR--------------*/
        List<Feature> GetAllFeatures(int ProjectId);
        bool DeleteFeatureFromDBByProjectId(int ProjectId);
        bool DeleteFeatureFromDBById(Int64 Id);
        bool DeleteFeatureFromDBById(List<Int64> Ids);
        bool AddFeaturesToDB(List<Feature> featureList);
        bool UpdateTags(List<string> TagList, int ProjectId);
        List<string> GetAllTagsByProjectId(int ProjectId);
        void CreateRelLogs(RelExecutionStatusLog logs);
        string ValidateRelExecutionBeforeSendRequest(int testsExecutionID);
        /*-----------------------------------------*/
    }
}
