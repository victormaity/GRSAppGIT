using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using GAgent;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GRSExecutor.Support;
using System.IO;
using Microsoft.Practices.ServiceLocation;

namespace GRSExecutor
{
    /// <summary>
    /// Summary description for Executor
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Executor : System.Web.Services.WebService
    {
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void RunTests(int id)
        {
            CommonMethods.RunSchenario(id);
            //CommonMethods.ExecutionMonitor(id);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SendConsoleCommand(string ip, string command)
        {
            var _command = command.Substring(1, (command.Length - 2));

            var file = Server.MapPath("~") + "searches.txt";
            if (!File.Exists(file))
                File.WriteAllText(file, "");
            var lines = File.ReadAllLines(file).ToList();
            if (!lines.Contains(_command))
                lines.Add(_command);
            File.WriteAllLines(file, lines);
            if (string.IsNullOrEmpty(ip))
                return "";
            var response = CommonMethods.SendRequestToClient(ip, new SocketRequest { RequestType = "console_request", RequestParameters = _command });
            if (!response.isSuccess)
                throw new SoapException(response.Message, SoapException.ServerFaultCode);
            return response.Message ?? "Command was executed";
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string[] GetAutocomplete()
        {
            var file = Server.MapPath("~") + "searches.txt";
            if (!File.Exists(file))
                File.WriteAllText(file, "");
            return File.ReadAllLines(file);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PingByID(int ID)
        {
            var pingResult = CommonMethods.PingTheClient(null, ID);
            if (pingResult != null)
            {
                return pingResult.isSuccess ? "free" : "busy";
            }
            return "null";
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PingByIP(string IP)
        {
            try
            {
                var pingResult = CommonMethods.PingTheClient(IP, 0);
                if (pingResult != null)
                {
                    return pingResult.isSuccess ? "free" : "busy";
                }
                return "null";
            }
            catch
            {
                return "null";
            }

        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void syncSchedule(int clientID)
        {
            var schedules = new Schedules();
            schedules._Schedules = new List<Schedule>();
            var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(p => p.ID == clientID);
            if (client != null)
            {
                var executions = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetAllToList(p => p.Client == clientID && p.ExecutionType == "schedule");
                foreach (var execution in executions)
                {
                    var newScheduleElement = new Schedule();
                    newScheduleElement.id = execution.ID.ToString();
                    newScheduleElement.ShouldBeStarted = new ScheduleDetails().Deserialize(execution.Scheduling).ShouldBeStarted;
                    newScheduleElement.Status = "New";
                    schedules._Schedules.Add(newScheduleElement);
                }
                //send request to client
                var response = CommonMethods.SendRequestToClient(client.RemoteMachineIP, new SocketRequest { RequestType = "syncSchedule", RequestParameters = schedules.Serialize() });
                if (!response.isSuccess) throw new Exception(response.Message);

            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SendCommandToMachine(string ip, string command)
        {
            var stopResponse = CommonMethods.SendRequestToClient(ip, new SocketRequest { RequestType = command });
            if (!stopResponse.isSuccess) throw new Exception(stopResponse.Message);
        }
        /*
        public class theUser
        {
            public string AccountName { get; set; }
            public string UserLogin { get; set; }
            public string UserPassword { get; set; }
            public string Comments { get; set; }
        }

        public class theProfile
        {
            public string ProfileName { get; set; }
            public string URL { get; set; }
            public bool isBackDoor { get; set; }
            public string hostFile { get; set; }
        }

        public class theConfiguration
        {
            public string ConfigurationName { get; set; }
            public string FileName { get; set; }
            public string FileContent { get; set; }
        }

        public class theLoginPage
        {
            public string LP_Name { get; set; }
            public string LP_URL { get; set; }
        }
       */
    }
}
