using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Abstract.BL.Helper
{
    public interface ISessionHelper
    {
        User GetUser(IPrincipal user);
        
        UserSession GetSessionDetails(IPrincipal user);

        List<TestCycle> GetActiveTestCycles(IPrincipal user);

        bool IsSessionExist(IPrincipal user);

        void SaveChanges();

        UserSession GetUserSession_Services(IPrincipal user);
    }
}
