using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.GRS;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageHomeProvider
    {
        HomePageStats GetHomePageStats(IPrincipal user, int deliveryItems);
        ExecutionReportPageDataModel GetExecutionReport(IPrincipal user, string duration = "30DAYS", string release = "ALL", string team = "ALL", string startdate = "", string enddate = "", int cycleid = 0);
        CountAndVMInfo GetSuitTestCaseExecutionCountAndClientInfo(string suitIds);
        TSTCInforBase GetTestSuitWithTestCasesDetails(string suitIds);
    }
}
