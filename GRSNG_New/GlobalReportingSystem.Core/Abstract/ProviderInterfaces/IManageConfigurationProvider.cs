using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageConfigurationProvider
    {
        void SetCurrentTestCycle(IPrincipal user, int id);

        void SetTestCycleInnactive(IPrincipal user, int id);

        string DeleteTestCycles(IPrincipal user, int testCycleId);

        void SetExecutionConfiguration(IPrincipal user, ExecutionConfigurationsModel model);

        void SetMachineToProject(IPrincipal user, int id);

        void SetIssueStatus(bool isJiraIssueIsNotNull, string jiraIssueSummary, Issue newIssue, string theStatName);

        void AssignUserOnProject(IPrincipal user, int id);

        void UnassignUser(IPrincipal user, int id);

        void AddAccountForTestRun(IPrincipal user, ExecutionAccountsModel model);

        void AddHostsConfiguration(IPrincipal user, ExecutionEnvironmentsModel model);

        void AddCycle(IPrincipal user, CyclesModel model);

        void AddClient(IPrincipal user, ExecutionMachniesModel model, ref string message);

        void DeleteClient(IPrincipal user, int id);

        CyclesModel GetCycles(IPrincipal user, string testcycletype);

        ExecutionConfigurationsModel GetExecutionConfigurations(IPrincipal user);

        IEnumerable<vw_FilesStorage> GetVwFilesStorage(IPrincipal user);

        ExecutionEnvironmentsModel GetExecutionEnvironments(IPrincipal user);

        ExecutionAccountsModel GetExecutionAccounts(IPrincipal user);

        ProjectAndUsersModel GetProjectAndUsers(IPrincipal user);

        ProjectAndUsersModel GetProjectAndUsers(IPrincipal user, string filter);

        ExecutionMachniesModel GetExecutionMachnies(IPrincipal user, string filter, ref string message);

        List<string> GetListOfPossibleDefects(IPrincipal user, out string bugtrackerLogin, out string bugtrackerPass, out int projId);

        List<Status> GetCustomStatuses(IPrincipal user);

        Status GetCustomStatus(IPrincipal user, string name);

        FilesStorage GetFramework(int id);

        void Sync(string zipFileLocation, IPrincipal user, string serverMapPath);

        void ReleaseMachine(int id);

        void DeleteAccount(IPrincipal user, int id);

        void DeleteHostsConfiguration(IPrincipal user, int id);

        void DeleteFileStorage(IPrincipal user, int id);

        void DeleteExecutionConfiguration(IPrincipal user, int id);

        void EditExecutionConfiguration(IPrincipal user, int id, string name, string fileName, string content);

        void EditExecutionAccount(IPrincipal user, int id, string name, string login, string password, string comments);

        void EditExecutionEnvironment(IPrincipal user, int id, string name, string url, string content);

        void EditTestCycle(IPrincipal user, int id, string name, string start, string end, string comment, string ReleaseName, string ReleaseDate, string TeamName);

        Issue CreateIssue(int projId, string defect);

        bool CheckUniqueValues(IPrincipal user, string color, string statusName, string statusID, Guid uniqueID,
            ref string message);

        void AddCustomStatus(IPrincipal user, Status status);

        void EditCustomStatus(IPrincipal user, Status status);

        List<SikuliObject> GetSikuliObjects(IPrincipal user);

        int GetProject(IPrincipal user);

        //string UpdateFrameworkFromSvn(int projId, IPrincipal user, string path);

        Dictionary<string, string> GetQcPath(int projectId, string testSet);

        bool SetQcPath(string testSetId, string qcPath);

        //List<string> SubmitProjectSettings(Dictionary<string, string> values, int projectId, IPrincipal user);

        void SetUpdatedConfiguration(IPrincipal user, ProjectConfigurationModel model);
        ProjectConfigurationModel GetProjectConfiguration(IPrincipal user);
        string PerformSvnSync(IPrincipal user, string path);
        void AssignUserOnProject(int projectId, int userId);
        void UnassignUser(int projectId, int userId);
        ProjectAndUsersModel GetProjectAndUsers(int projectId, string filter);
        ProjectAndUsersModel GetProjectAndUsers(int projectId);
        List<Project> GetAvailableProjects();
        void DeleteDirectory(string path);

        //-----------------SUNDRAM------------------------------------
        bool CloneProjectForFeaturesAndTagsFromGIT(string LocalDiskPath, bool IsGITDefault, string SVNPath, string SVNLogin, string SVNPassword, string GITPath, string GITLogin, string GITPassword, string ProjectName);
        Project GetProjectDataById(int ProjectId);
        string GetUserNameById(int UserId);
        void LockProjectForFeatureTagUpdate(int ProjectId, int UserId);
        void ReleaseProjectForFeatureTagUpdate(int ProjectId, int UserId);

        //====================================================================

        string DeleteOldRecordByAdmin(IPrincipal currentuser, int projectId, DateTime beforeDate);
        UserListModel GetUserListData(IPrincipal currentuser);
        //Assign User Admin right 
        string AssignUserAdminRight(IPrincipal assineeUser, int userToAssign);
        //Unassign User Admin right
        string UnAssignUserAdminRight(IPrincipal assineeUser, int userToUnassign);
        //Assign Global Admin right 
        string AssignGlobalAdminRight(IPrincipal assineeUser, int userToAssign);
        //Unassigned admin right to user
        string UnAssignGlobalAdminRight(IPrincipal assineeUser, int userToUnassign);
        //UnBlockUser
        string UnBlockUser(IPrincipal assineeUser, int userToUnassign);
        //BlockUser
        string BlockUser(IPrincipal assineeUser, int userToUnassign);
        bool CheckIsAdminUsingPage(IPrincipal currentUser);
        List<ProjectListModel> GetProjectListForDeleteOldRecord();
        ErrorControl AddNewUser(IPrincipal currentUser, string UserName, string UserFullName, string UserEmail, bool IsUserAdmin, bool IsUserGlobalAdmin, int ProjectID);
        ErrorControl AddNewProject(IPrincipal currentUser, AdminToolAddNewProject newProjectData);
        string DeleteHarvestTempUploadedFile(IPrincipal currentUser);
        string DeleteWEBUITempFiles(IPrincipal currentUser);
        string DeleteChartFiles(IPrincipal currentUser);
        ManageTestSuits GetTestSuitList(IPrincipal currentUser, int projectId, DateTime dateBefore);
        string DeleteSuitSuitsByIds(IPrincipal currentUser, List<Int64> TSuitIds);
        List<TrackingExecutionsAndLogs> GetRelExecutionTrackingData(IPrincipal currentUser, string MachineIp);
        bool DeleteRelExecutionTrackingDataAndLogs(IPrincipal currentUser, Int64[] relExIds);
        List<ProjectType> GetProjectTypes();
        //====================================================================
        #region TEAM
        TeamInfoModel GetTeams(IPrincipal user);
        TeamInfo GetTeamById(Int64 teamid);
        string AddNewTeam(IPrincipal user, string teamName, string comment);
        string UpdateTeamInfo(IPrincipal user, Int64 teamId, string teamName, string comment);
        string DeleteTeam(IPrincipal user, Int64 teamId);
        #endregion TEAM

        #region RELEASE
        ReleaseInfoModel GetReleases(IPrincipal user);
        ReleaseInfo GetReleaseById(Int64 releaseid);
        string AddNewRelease(IPrincipal user, string releaseName, string releaseDate, string comment);
        string UpdateReleaseInfo(IPrincipal user, Int64 releaseId, string releaseName, string releaseDate, string comment);
        string DeleteRelease(IPrincipal user, Int64 releaseId);
        #endregion RELEASE

        //==============================================================================================

        #region Group
        List<ExecutionGroupModel> GetExecutionGroupInfo(IPrincipal user);
        ResultReturnModel AddExecutionGroup(IPrincipal user, string groupName);
        ResultReturnModel UpdateExecutionGroup(IPrincipal user, Int64 id, string groupName);
        ResultReturnModel DeleteExecutionGroup(IPrincipal user, Int64 id);
        #endregion Group
    }
}
