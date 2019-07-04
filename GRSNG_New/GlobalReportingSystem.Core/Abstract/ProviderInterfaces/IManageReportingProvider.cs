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
    public interface IManageReportingProvider
    {
        List<CyclesModel> GetCumulativeStatistics(IPrincipal user);

        List<ReportCommulativeModel> AddReport(IPrincipal user, int cycleId);

        List<TestSuit> GetTestSuits(IPrincipal user, int cycleId);

        List<BugListModel> GetAllBugList(int cycleId);

        List<TestCase> getAnalysisDataByCycle(IPrincipal user, int cycleId);

        UsersAndCyclesModel GetAnalysisStatistics(IPrincipal user);

        UsersAndTestCasesModel getAnalysisDataByUser(int userId, DateTime startDate, DateTime? endDate);

        List<TestCaseDetails> GetFailedTestCases(string bugId, int cycleId);

        JiraIssueModel GetJiraIssue(string key);
    }
}
