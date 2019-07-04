using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.Executor;
using GRSExecutor.Support;
using Ionic.Zip;
using Microsoft.Practices.ServiceLocation;

namespace GRSExecutor
{
    /// <summary>
    /// Summary description for Linear
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Linear : WebService
    {
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void RunTests(int id)
        {
            CommonMethods.RunLinearElement(id);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string WaitForRelResults(int executionId)
        {
            Report responses = null;

            while (responses == null)
            {
                try
                {
                    List<int> testSetIds;
                    var execution = CommonMethods.GetLinearExecutionById(executionId);
                    var machineIp = execution.Client1.RemoteMachineIP;
                    responses = CommonMethods.LinearResultsWaiter(machineIp, execution, out testSetIds);
                    if (responses != null)
                    {
                        XmlSerializer xsSubmit = new XmlSerializer(responses.GetType());
                        StringWriter sww = new StringWriter();
                        XmlWriter writer = XmlWriter.Create(sww);
                        xsSubmit.Serialize(writer, responses);
                        var xml = sww.ToString();

                        try
                        {
                            CommonMethods.SendNolioExcelReport(testSetIds, xml, executionId);
                        }
                        catch
                        {
                        }
                        return xml;
                    }
                }
                catch
                { return null; }
            }
            return null;
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ReRunTest(int tcId)
        {
            var executionId = (new CommonMethods()).GetNunitPath(tcId);
            CommonMethods.RunLinearElement(executionId);
        }
    }
}
