using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using QCClient.Client;
using QcClient.Entities.QcEntities;
using QcClient.Entities.XmlEntities;
using QcClient.Tools;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageExportToExcel
    {
        string ExportTestSetsToExcel(IPrincipal user, int[] testCycleId);
    }
}
