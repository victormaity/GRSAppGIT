using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Xml.Serialization;
using Castle.Core.Internal;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GRSHarvest.Cucumber;
using GRSHarvest.Properties;
using System.Web.UI.DataVisualization.Charting;
using Ionic.Zip;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using TechTalk.JiraRestClient;
using Labels = GlobalReportingSystem.Core.Constants.Labels;

namespace GRSHarvest
{
    /// <summary>
    /// Summary description for ServiceConnector
    /// </summary>
    [WebService(Namespace = "http://GRSUri.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ServiceConnector : System.Web.Services.WebService
    {
        private string _newTempFolder = "";

        [WebMethod]
        public string AcceptReport(byte[] ZipFile, Nullable<int> taskId)
        {
            try
            {
                var appFolder = HttpContext.Current.Server.MapPath("~") + @"\";
                var tempFolder = HttpContext.Current.Server.MapPath("~/Temp");
                var newFile = string.Concat(tempFolder, @"\", Guid.NewGuid().ToString(), ".zip");
                File.WriteAllBytes(newFile, ZipFile);
                _newTempFolder = tempFolder + @"\" + Guid.NewGuid().ToString();
                Directory.CreateDirectory(_newTempFolder);
                using (var zipFile = new ZipFile(newFile))
                {
                    zipFile.ExtractAll(_newTempFolder);
                }
                File.Delete(newFile);

                int ciId = 0, projectId = 0;
                GRS_Report report = null;
                List<Cucumber.CucumberReport> reportCucumber = null;
                List<CucumberNew.CucumberReportNew> reportCucumberNew = null;//**** for new cucumber report format
                List<IGrouping<string, Cucumber.CucumberReport>> testSuites = null;
                bool isNewCucumberReport = false;


                if (File.Exists(_newTempFolder + @"\GRSReport.xml"))
                {
                    var xDoc = new XmlDocument();
                    xDoc.Load(_newTempFolder + @"\GRSReport.xml");

                    var serializer = new XmlSerializer(typeof(GRS_Report));
                    xDoc.InnerXml = xDoc.InnerXml.Replace("&#x1A;", "");
                    var byteArray = Encoding.UTF8.GetBytes(xDoc.InnerXml);
                    using (var stream = new MemoryStream(byteArray))
                    {
                        report = (GRS_Report)serializer.Deserialize(stream);
                        ciId = parseClientInfo(report);
                        projectId = getProjectID(report.client_info.project);
                    }
                }
                else
                {
                    var reportFile = Directory.GetFiles(_newTempFolder, "*.json").First();
                    var projName = reportFile.Split('\\').Last().Split('.').First();
                    projectId = getProjectID(projName);

                    try
                    {
                        reportCucumber = JsonConvert.DeserializeObject<List<Cucumber.CucumberReport>>(File.ReadAllText(reportFile));
                        testSuites = reportCucumber.GroupBy(p => p.name).ToList();
                        ciId = parseCucumberClientInfo(reportCucumber);

                    }
                    catch (Exception ex)
                    {
                        reportCucumberNew = JsonConvert.DeserializeObject<List<CucumberNew.CucumberReportNew>>(File.ReadAllText(reportFile));
                        ciId = parseCucumberClientInfoNew(reportCucumberNew);
                        isNewCucumberReport = true;
                    }
                    //**** for new cucumber report format
                }

                //start parsing section here:
                var uid = new List<Guid>();
                if (report != null)
                {
                    uid.Add(parseTestSuite(report, projectId, ciId));
                }
                else
                {
                    if (!isNewCucumberReport)
                    {
                        foreach (var testSuite in testSuites)
                        {
                            uid.Add(parseCucumberTestSuite(testSuite, projectId, ciId));
                        }
                    }
                    else
                    {
                        foreach (var reportNew in reportCucumberNew)
                        {
                            uid.Add(parseCucumberTestSuiteNew(reportNew.testset, projectId, ciId));
                        }
                    }
                }

                if (!Directory.Exists(appFolder + @"Attachments\" + projectId))
                    Directory.CreateDirectory(appFolder + @"Attachments\" + projectId);

                foreach (
                    var FileName in
                        Directory.GetFiles(_newTempFolder).Where(p => Path.GetFileName(p) != "GRSReport.xml"))
                {
                    switch (Path.GetExtension(FileName).Replace(".", "").ToUpper())
                    {
                        case "PNG":
                        case "JPG":
                        case "BMP":
                            if ((report != null || isNewCucumberReport) && !File.Exists(appFolder + @"Screenshots\" + Path.GetFileName(FileName)))
                                File.Move(FileName, appFolder + @"Screenshots\" + Path.GetFileName(FileName));
                            else if (report == null && !File.Exists(appFolder + @"Attachments\" + projectId + @"\" +
                                                                    Path.GetFileName(FileName)))
                                File.Move(FileName,
                                          appFolder + @"Attachments\" + projectId + @"\" +
                                          Path.GetFileName(FileName));
                            break;
                        case "FLV":
                        case "AVI":
                            File.Move(FileName, appFolder + @"Video\" + Path.GetFileName(FileName));
                            break;
                        default:
                            if (!File.Exists(appFolder + @"Attachments\" + projectId + @"\" +
                                             Path.GetFileName(FileName)))
                                File.Move(FileName,
                                          appFolder + @"Attachments\" + projectId + @"\" +
                                          Path.GetFileName(FileName));

                            break;
                    }
                }

                //Send Notification about the resuls
                //get list of recipients
                var _Project =
                    ServiceLocator.Current.GetInstance<IRepository<Project>>()
                                  .GetSingleOrDefault(p => p.ID == projectId);
                var _Users =
                    ServiceLocator.Current.GetInstance<IRepository<Access>>()
                                  .GetAll(p => p.AttachedProject == projectId && p.Guest == null && p.DeliveryResult)
                                  .ToList();
                if (_Project.Notification_ResultsDelivery.HasValue)
                {
                    if (_Project.Notification_ResultsDelivery.Value)
                    {
                        foreach (var gid in uid)
                        {
                            var res =
                                ServiceLocator.Current.GetInstance<IRepository<TestSuit>>()
                                              .GetSingleOrDefault(p => p.UI == gid);
                            if (res == null)
                                throw new ArgumentNullException("The System was not able to find requested test suite");

                            if (res.TestCases.Count != 0)
                            {
                                var message = Resources.EmailTemplate;
                                message = message.Replace("%project_name%", res.Project.DisplayName);
                                message = message.Replace("%test_cycle%",
                                                          (res.TestCycle != null
                                                               ? res.TestCycle.CycleName
                                                               : "Not defined"));
                                message = message.Replace("%test_suite%", res.TSName);
                                message = message.Replace("%ip_address%", res.ClientsInformation.ClientIP);
                                var timeS = new TimeSpan();
                                res.TestCases.ToList().ForEach(p => timeS += (p.TCEndTime - p.TCStartTime));
                                message = message.Replace("%duration%", timeS.ToString(@"dd\.hh\:mm\:ss"));
                                message = message.Replace("%browser%", res.ClientsInformation.ClientBrowser);
                                message = message.Replace("%environment%", res.ClientsInformation.ClientEnvironment);
                                message = message.Replace("%url%", res.ClientsInformation.ClientURL);
                                message = message.Replace("%account%", res.ClientsInformation.ClientUser);
                                //message = message.Replace("%account%", res.ClientsInformation.ClientUser);
                                var allTests = res.TestCases.GroupBy(p => p.TCName).Select(p => p.Last()).ToList();
                                var pass =
                                    allTests.Count(p => p.TCState == "pass");
                                message = message.Replace("%passed%", pass.ToString(CultureInfo.InvariantCulture));
                                var fail = allTests.Count(p => p.TCState == "fail");
                                message = message.Replace("%failed%", fail.ToString(CultureInfo.InvariantCulture));
                                var blocked = allTests.Count(p => p.TCState == "notcompleted");
                                message = message.Replace("%blocked%",
                                                          blocked.ToString(CultureInfo.InvariantCulture));
                                var notrunned = allTests.Count(p => p.TCState == "norun");
                                message = message.Replace("%notrunned%",
                                                          notrunned.ToString(CultureInfo.InvariantCulture));
                                message = message.Replace("%total%",
                                                          allTests.Count().ToString(CultureInfo.InvariantCulture));
                                message = message.Replace("%id%", res.ID.ToString(CultureInfo.InvariantCulture));

                                //draw chart
                                message = DrawChartMessage(pass, fail, blocked, notrunned, gid.ToString(), message);

                                //foreach (var user in _Users)
                                //{
                                //    ServiceLocator.Current.GetInstance<IEmailer>()
                                //                  .SendMail(new List<string> { user.User.UserEmail },
                                //                            "grs@clarivate.com",
                                //                            "[Global Reporting System - " + _Project.DisplayName +
                                //                            "] Results were delivered",
                                //                            message.Replace("%user_name%", user.User.USerFullName));
                                //}
                            }
                        }
                    }
                }

                //try to delete temp folder
                try
                {
                    Directory.Delete(_newTempFolder, true);
                }
                catch
                {
                }

                if (report != null)
                {
                    return uid.First().ToString();
                }
                return String.Empty;
            }
            catch (Exception ex)
            {
                throw new SoapException(ex.Message + "|" + ex.StackTrace, SoapException.ServerFaultCode);
            }
            finally
            {
                GC.Collect();
            }
            return String.Empty;
        }

        [WebMethod]
        public string AcceptReportTest()
        {
            try
            {
                _newTempFolder = @"D:\New folder";

                int ciId = 0, projectId = 0;
                GRS_Report report = null;
                List<Cucumber.CucumberReport> reportCucumber = null;
                List<CucumberNew.CucumberReportNew> reportCucumberNew = null;//**** for new cucumber report format
                List<IGrouping<string, Cucumber.CucumberReport>> testSuites = null;
                bool isNewCucumberReport = false;
                string jsonName = ".json";


                if (File.Exists(_newTempFolder + @"\GRSReport.xml"))
                {
                    var xDoc = new XmlDocument();
                    xDoc.Load(_newTempFolder + @"\GRSReport.xml");

                    var serializer = new XmlSerializer(typeof(GRS_Report));
                    var byteArray = Encoding.UTF8.GetBytes(xDoc.InnerXml);
                    using (var stream = new MemoryStream(byteArray))
                    {
                        report = (GRS_Report)serializer.Deserialize(stream);
                        ciId = parseClientInfo(report);
                        projectId = getProjectID(report.client_info.project);
                    }
                }
                else
                {
                    var reportFile = Directory.GetFiles(_newTempFolder, "*.json").First();
                    var projName = reportFile.Split('\\').Last().Split('.').First();
                    jsonName = projName + jsonName;
                    projectId = getProjectID(projName);

                    try
                    {
                        reportCucumber = JsonConvert.DeserializeObject<List<Cucumber.CucumberReport>>(File.ReadAllText(reportFile));
                        testSuites = reportCucumber.GroupBy(p => p.name).ToList();
                        ciId = parseCucumberClientInfo(reportCucumber);

                    }
                    catch (Exception)
                    {
                        reportCucumberNew = JsonConvert.DeserializeObject<List<CucumberNew.CucumberReportNew>>(File.ReadAllText(reportFile));
                        ciId = parseCucumberClientInfoNew(reportCucumberNew);
                        isNewCucumberReport = true;
                    }
                    //**** for new cucumber report format
                }

                //start parsing section here:
                var uid = new List<Guid>();
                if (report != null)
                {
                    uid.Add(parseTestSuite(report, projectId, ciId));
                }
                else
                {
                    if (!isNewCucumberReport)
                    {
                        foreach (var testSuite in testSuites)
                        {
                            uid.Add(parseCucumberTestSuite(testSuite, projectId, ciId));
                        }
                    }
                    else
                    {
                        foreach (var reportNew in reportCucumberNew)
                        {
                            uid.Add(parseCucumberTestSuiteNew(reportNew.testset, projectId, ciId));
                        }
                    }
                }
                var appFolder = HttpContext.Current.Server.MapPath("~") + @"\";
                if (!Directory.Exists(appFolder + @"Attachments\" + projectId))
                    Directory.CreateDirectory(appFolder + @"Attachments\" + projectId);

                foreach (
                    var FileName in
                        Directory.GetFiles(_newTempFolder).Where(p => Path.GetFileName(p) != "GRSReport.xml" && Path.GetFileName(p) != jsonName))
                {
                    switch (Path.GetExtension(FileName).Replace(".", "").ToUpper())
                    {
                        case "PNG":
                        case "JPG":
                        case "BMP":
                            if ((report != null || isNewCucumberReport) && !File.Exists(appFolder + @"Screenshots\" + Path.GetFileName(FileName)))
                                File.Move(FileName, appFolder + @"Screenshots\" + Path.GetFileName(FileName));
                            else if (report == null && !File.Exists(appFolder + @"Attachments\" + projectId + @"\" +
                                                                    Path.GetFileName(FileName)))
                                File.Move(FileName,
                                          appFolder + @"Attachments\" + projectId + @"\" +
                                          Path.GetFileName(FileName));
                            break;
                        case "FLV":
                        case "AVI":
                            File.Move(FileName, appFolder + @"Video\" + Path.GetFileName(FileName));
                            break;
                        default:
                            if (!File.Exists(appFolder + @"Attachments\" + projectId + @"\" +
                                             Path.GetFileName(FileName)))
                                File.Move(FileName,
                                          appFolder + @"Attachments\" + projectId + @"\" +
                                          Path.GetFileName(FileName));

                            break;
                    }
                }


                return String.Empty;
            }
            catch (Exception ex)
            {
                throw new SoapException(ex.Message + "|" + ex.StackTrace, SoapException.ServerFaultCode);
            }
            finally
            {
                GC.Collect();
            }
        }

        private static string DrawChartMessage(int pass, int fail, int blocked, int notrunned, string rId, string message)
        {
            var resultsChart = new Chart();
            resultsChart.ChartAreas.Add("ChartArea1");
            resultsChart.Width = 450;
            resultsChart.Height = 300;
            resultsChart.Series.Add("Default");
            double[] yValues = { pass, fail, blocked, notrunned };
            string[] xValues = { "PASSED", "FAILED", "BLOCKED", "NOT RUNNED" };
            resultsChart.Series["Default"].Points.DataBindXY(xValues, yValues);
            resultsChart.Series["Default"].Points[0].Color = Color.MediumSeaGreen;
            resultsChart.Series["Default"].Points[1].Color = Color.OrangeRed;
            resultsChart.Series["Default"].Points[2].Color = Color.Orange;
            resultsChart.Series["Default"].Points[3].Color = Color.Gray;
            resultsChart.Series["Default"].ChartType = SeriesChartType.Pie;
            resultsChart.Series["Default"]["PieLabelStyle"] = "Disabled";
            resultsChart.ChartAreas["ChartArea1"].Area3DStyle.Enable3D = true;
            resultsChart.Legends.Add(new Legend
            {
                Alignment = StringAlignment.Center,
                Docking = Docking.Bottom,
                LegendStyle = LegendStyle.Row,
            });
            resultsChart.Legends[0].Enabled = true;


            var chartsLocation = HttpContext.Current.Server.MapPath("~") + "\\Charts";
            if (!Directory.Exists(chartsLocation))
                Directory.CreateDirectory(chartsLocation);
            var fileName = rId + ".png";
            var fileNamePath = chartsLocation + @"\" + fileName;
            resultsChart.SaveImage(fileNamePath, ChartImageFormat.Png);
            message = message.Replace("%chart%", fileName);
            return message;
        }

        private int parseClientInfo(GRS_Report report)
        {
            var ci = new ClientsInformation
            {
                ClientBrowser = report.client_info.browser,
                ClientEnvironment = report.client_info.environment,
                ClientFreeSpace = report.client_info.free_space,
                ClientIP = report.client_info.ip,
                ClientOS = report.client_info.os,
                ClientURL = report.client_info.url,
                ClientUser = report.client_info.user,
                UI = Guid.NewGuid(),
                EndTime = DateTime.Parse(report.client_info.end_time),
                StartTime = DateTime.Parse(report.client_info.start_time)
            };
            var allClientsInfo = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
            allClientsInfo.Add(ci);
            allClientsInfo.SaveChanges();
            //get it back and return ID
            var res = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAll(p => p.UI == ci.UI).ToList();
            if (!res.Count.Equals(0))
                return res[0].ID;
            throw new Exception("Can't find requred information");
        }

        private int parseCucumberClientInfo(List<Cucumber.CucumberReport> report)
        {
            var ci = new ClientsInformation
            {
                ClientBrowser = null,
                ClientEnvironment = report.Last() == null ? string.Empty :
                    (report.Last().elements.Last() == null ? string.Empty :
                    (report.Last().elements.Last().steps.Last().output == null ? string.Empty :
                    ("Environment: " + report.Last().elements.Last().steps.Last().output.First() + "\r\n\r\n"))),
                ClientFreeSpace = "",
                ClientIP = HttpContext.Current.Request.UserHostAddress,
                ClientOS = "",
                ClientURL = "",
                ClientUser = "",
                UI = Guid.NewGuid(),
                EndTime = DateTime.Now, // DateTime.Parse(report.client_info.end_time),
                StartTime = DateTime.Now //DateTime.Parse(report.client_info.start_time)
            };
            var allClientsInfo = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
            allClientsInfo.Add(ci);
            allClientsInfo.SaveChanges();
            //get it back and return ID
            var res = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAll(p => p.UI == ci.UI).ToList();
            if (!res.Count.Equals(0))
                return res[0].ID;
            throw new Exception("Can't find requred information");
        }

        private int parseCucumberClientInfoNew(List<CucumberNew.CucumberReportNew> report)
        {
            var ci = new ClientsInformation
            {
                ClientBrowser = report.Last() == null ? string.Empty :
                    (report.Last().testset == null ? string.Empty :
                    (report.Last().testset.browser ?? string.Empty)),
                ClientEnvironment = report.Last() == null ? string.Empty :
                    (report.Last().testset == null ? string.Empty :
                    (report.Last().testset.url == null ? string.Empty :
                    ("Environment: " + report.Last().testset.url + "\r\n\r\n"))),
                ClientFreeSpace = "",
                ClientIP = report.Last() == null ? string.Empty :
                    (report.Last().testset == null ? string.Empty :
                    (report.Last().testset.ip ?? string.Empty)),
                ClientOS = report.Last() == null ? string.Empty :
                    (report.Last().testset == null ? string.Empty :
                    (report.Last().testset.os ?? string.Empty)),
                ClientURL = report.Last() == null ? string.Empty :
                    (report.Last().testset == null ? string.Empty :
                    (report.Last().testset.url ?? string.Empty)),
                ClientUser = "",
                UI = Guid.NewGuid(),
                EndTime = DateTime.Now, // DateTime.Parse(report.client_info.end_time),
                StartTime = report.Last() == null ? DateTime.Now :
                    (report.Last().testset == null ? DateTime.Now :
                    (DateTime.Parse(report.Last().testset.time)))
            };
            var allClientsInfo = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
            allClientsInfo.Add(ci);
            allClientsInfo.SaveChanges();
            //get it back and return ID
            var res = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAll(p => p.UI == ci.UI).ToList();
            if (!res.Count.Equals(0))
                return res[0].ID;
            throw new Exception("Can't find requred information");
        }

        private Guid parseTestSuite(GRS_Report report, int parentProject, int CID)
        {
            var TS = new TestSuit();
            //Retrieve currnt test cycle:
            //var currentTestCycle = xDoc.SelectSingleNode("//test_cycle");
            TestCycle targetTc = null;
            if (!String.IsNullOrWhiteSpace(report.client_info.test_cycle))
            {
                targetTc =
                    ServiceLocator.Current.GetInstance<IRepository<TestCycle>>()
                        .GetSingleOrDefault(p => p.CycleName == report.client_info.test_cycle && p.ParentProject == parentProject);
                
            }
            else
            {
                targetTc =
                    ServiceLocator.Current.GetInstance<IRepository<TestCycle>>()
                        .GetSingleOrDefault(p => p.ParentProject == parentProject && p.CycleIsCurrent.HasValue && (bool)p.CycleIsCurrent);
            }
            if (targetTc != null)
            {
                TS.ParentTestCycle = targetTc.ID;
            }
            TS.ParentClientInfo = CID;
            TS.ParentProject = parentProject;
            TS.TSName = report.TestSet.name;
            TS.TSStart = DateTime.Parse(report.TestSet.start_time);
            // Parse(xDoc.SelectSingleNode("//TestSet").Attributes["start_time"].InnerText);
            TS.UI = Guid.NewGuid();
            TS.DeliveryTime = DateTime.Now;

            if (report.TestSet.TestCase != null)
            {
                TS.TestCases = parseTestCases(report.TestSet.TestCase.Where(p => p != null).ToArray());
                TS.FailedTestCases = TS.TestCases.Count(u => u.TCState != "pass");
                TS.PassedTestCases = TS.TestCases.Count(u => u.TCState == "pass");
            }
            else
            {
                TS.TestCases = null;
                TS.FailedTestCases = 0;
                TS.PassedTestCases = 0;
            }

            var testSuit = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
            testSuit.Add(TS);
            testSuit.SaveChanges();
            //get it back and return ID
            return TS.UI;
        }

        private Guid parseCucumberTestSuite(IGrouping<string, Cucumber.CucumberReport> report, int parentProject, int CID)
        {
            var TS = new TestSuit();

            //if (report.client_info.test_cycle != null)
            //{
            //    TestCycle TargetTC = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetSingleOrDefault(p => p.CycleName == tCycle);
            //    if (TargetTC != null)
            //    {
            //        TS.ParentTestCycle = TargetTC.ID;
            //    }
            //}
            TS.ParentTestCycle = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetSingleOrDefault(p => p.ParentProject == parentProject && p.CycleIsCurrent.HasValue && p.CycleIsCurrent.Value).ID;

            TS.ParentClientInfo = CID;
            TS.ParentProject = parentProject;
            TS.TSName = report.Key;
            TS.TSStart = DateTime.Now;// re-write
            TS.UI = Guid.NewGuid();
            TS.DeliveryTime = DateTime.Now;

            TS.TestCases = parseCucumberTestCases(report.SelectMany(p => p.elements).ToList());
            TS.FailedTestCases = TS.TestCases.Count(u => u.TCState != "pass");
            TS.PassedTestCases = TS.TestCases.Count(u => u.TCState == "pass");

            var testSuit = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
            testSuit.Add(TS);
            testSuit.SaveChanges();
            UpdateJiraFeature(report, TS);
            return TS.UI;
        }

        private Guid parseCucumberTestSuiteNew(CucumberNew.Testset testset, int parentProject, int CID)
        {
            var TS = new TestSuit();


            //var cycle = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>()
            //    .GetFirstOrDefault(
            //        p =>
            //            p.ParentProject == parentProject && p.CycleIsCurrent.HasValue &&
            //            p.CycleName.ToLower() == testset.targettestcycle.ToLower());





            var cycle = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>()
                .GetSingleOrDefault(
                    p =>
                        p.ParentProject == parentProject && p.CycleIsCurrent.HasValue &&
                        p.CycleName.ToLower() == testset.targettestcycle.ToLower());

            TS.ParentTestCycle = cycle == null ? ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetSingleOrDefault(p => p.ParentProject == parentProject && p.CycleIsCurrent.HasValue && p.CycleIsCurrent.Value).ID : cycle.ID;

            TS.ParentClientInfo = CID;
            TS.ParentProject = parentProject;
            TS.TSName = testset.name;
            TS.TSStart = DateTime.Parse(testset.time);// re-write
            TS.UI = Guid.NewGuid();
            TS.DeliveryTime = DateTime.Now;

            TS.TestCases = parseCucumberTestCasesNew(testset.tests.ToList());
            TS.FailedTestCases = TS.TestCases.Count(u => u.TCState != "pass");
            TS.PassedTestCases = TS.TestCases.Count(u => u.TCState == "pass");

            var testSuit = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
            testSuit.Add(TS);
            testSuit.SaveChanges();
            UpdateJiraFeatureNew(testset, TS);
            return TS.UI;
        }

        private void UpdateJiraFeature(IGrouping<string, Cucumber.CucumberReport> report, TestSuit TS)
        {
            try
            {
                var jiraFeature = report.First().tags.Where(p => p.name.ToLower().StartsWith(Labels.JiraSearchTag));
                var isJiraStory = jiraFeature.Any();
                if (isJiraStory)
                {
                    var jiraClient = new JiraClient(Labels.JiraLoginPageUrl, Labels.JiraLogin, Labels.JiraPassword);

                    string comment = report.Last() == null ? string.Empty :
                        (report.Last().elements.Last() == null ? string.Empty :
                        (report.Last().elements.Last().steps.Last().output == null ? string.Empty :
                        ("Environment: " + report.Last().elements.Last().steps.Last().output.First() + "\r\n\r\n")));
                    TS.TestCases.ForEach(
                        p =>
                            comment +=
                                string.Concat((p.TCState == "fail" ? "(x)" : (p.TCState == "pass" ? "(/)" : "(!)")), "  ",
                                p.TCName.Split(new string[] { "\r\n" }, StringSplitOptions.None).Last(), "\r\n"));

                    var issueRef = new IssueRef { key = jiraFeature.First().name.ToLower().Remove(0, Labels.JiraSearchTag.Length) };
                    var feature = jiraClient.LoadIssue(issueRef);
                    var jiraComment = jiraClient.CreateComment(feature, comment);
                    feature.fields.comments.Add(jiraComment);
                    jiraClient.UpdateIssue(feature);
                }
            }
            catch { }
        }

        private void UpdateJiraFeatureNew(CucumberNew.Testset testset, TestSuit TS)
        {
            try
            {
                if (testset != null)
                {
                    var jiraFeature = testset.name;
                    var isJiraStory = jiraFeature.ToLower().StartsWith(Labels.JiraSearchTag);
                    if (isJiraStory)
                    {
                        var jiraClient = new JiraClient(Labels.JiraLoginPageUrl, Labels.JiraLogin, Labels.JiraPassword);

                        string comment = "Environment: " + testset.url + "\r\n\r\n";
                        TS.TestCases.ForEach(
                            p =>
                                comment +=
                                    string.Concat(
                                        (p.TCState == "fail" ? "(x)" : (p.TCState == "pass" ? "(/)" : "(!)")), "  ",
                                        p.TCName.Split(new string[] { "\r\n" }, StringSplitOptions.None).Last(), "\r\n"));

                        var issueRef = new IssueRef
                        {
                            key = jiraFeature.ToLower().Remove(0, Labels.JiraSearchTag.Length)
                        };
                        var feature = jiraClient.LoadIssue(issueRef);
                        var jiraComment = jiraClient.CreateComment(feature, comment);
                        feature.fields.comments.Add(jiraComment);
                        jiraClient.UpdateIssue(feature);
                    }
                }
            }
            catch { }
        }

        private void performAutoAnalysis(SubStep dbSubStep, int parentProject, int parentTS)
        {
            // Perform auto analysis
            var testStep = ServiceLocator.Current.GetInstance<IRepository<TestStep>>().GetFirstOrDefault(p => p.ID == parentTS);

            if (testStep.TestCase.TestSuit.Project.AutoAnalysis.HasValue &&
                testStep.TestCase.TestSuit.Project.AutoAnalysis.Value)
            {
                if (!dbSubStep.SubStepValid)
                {
                    var message = UpdateByRegEx(dbSubStep.SubStepMessage);
                    var cache = ServiceLocator.Current.GetInstance<IRepository<AutoAnalysisCache>>().GetFirstOrDefault(p => p.ErrorMessage == message && p.ProjectID == testStep.TestCase.TestSuit.Project.ID);
                    if (cache != null)
                    {
                        dbSubStep.AnalyzedStatus = cache.Status;
                        dbSubStep.Defects = cache.DefectID;
                    }
                }
            }
        }

        private string UpdateByRegEx(string message)
        {
            message = Regex.Replace(message, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", "");
            message = Regex.Replace(message, @"\d+\.\d+", "");
            message = Regex.Replace(message, @":\d+", "");
            return message;
        }

        private List<TestStep> parseCucumberTestStepData(IEnumerable<Step> testSteps)
        {
            try
            {
                var dbTestSteps = new List<TestStep>();

                foreach (var testStep in testSteps)
                {
                    var dbTestStep = new TestStep();

                    DateTime tempDateTime;
                    if (DateTime.TryParse(null/*testStep.date*/, out tempDateTime))
                        dbTestStep.StepStartTime = tempDateTime;
                    else
                        dbTestStep.StepStartTime = DateTime.Now;
                    dbTestStep.StepType = testStep.result.status.ToLower().Contains("skipped") || testStep.result.status.ToLower().Contains("undefined") ? "norun" :
                        (testStep.result.status.ToLower().Contains("fail") ? "fail" : "pass");
                    dbTestStep.StepDescription = testStep.name;

                    dbTestStep.StepExpected = null;

                    var errorMessage = testStep.result.error_message;
                    if (errorMessage != null)
                    {
                        dbTestStep.StepActual = testStep.result.error_message;
                        dbTestStep.SubSteps = parseCucumberTestSubStepData(testStep.result);
                    }
                    dbTestStep.StepInputData = String.Empty;
                    if (testStep.embeddings != null && testStep.embeddings.Count != 0)
                    {
                        string attachments = null;
                        foreach (var embedding in testStep.embeddings)
                        {
                            if (embedding.data != null)
                            {
                                using (var ms = Base64ToMemoryStream(embedding.data))
                                {
                                    var attachName = Guid.NewGuid() + "." + embedding.mime_type.Split('/').Last();
                                    using (var file = new FileStream(_newTempFolder + "\\" + attachName, FileMode.Create,
                                          FileAccess.Write))
                                    {
                                        ms.WriteTo(file);
                                    }
                                    if (attachments == null)
                                        attachments += attachName;
                                    else
                                        attachments += ";" + attachName;
                                }
                            }
                        }
                        dbTestStep.Attachments = attachments;
                    }

                    dbTestStep.UI = Guid.NewGuid();

                    dbTestSteps.Add(dbTestStep);
                }
                return dbTestSteps;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test step data: see inner exception for detailes", ex);
            }
        }

        private List<TestStep> parseCucumberTestStepDataNew(IEnumerable<CucumberNew.Step> testSteps)
        {
            try
            {
                var dbTestSteps = new List<TestStep>();
                var orderCounter = 1;
                foreach (var testStep in testSteps)
                {
                    var dbTestStep = new TestStep();

                    DateTime tempDateTime;
                    if (DateTime.TryParse(/*null*/testStep.date, out tempDateTime))
                        dbTestStep.StepStartTime = tempDateTime;
                    else
                        dbTestStep.StepStartTime = DateTime.Now;
                    dbTestStep.StepType = testStep.status.ToLower().Contains("skipped") || testStep.status.ToLower().Contains("undefined") ? "norun" :
                        (testStep.status.ToLower().Contains("fail") ? "fail" : "pass");
                    dbTestStep.StepDescription = testStep.title;
                    dbTestStep.SortOrder = orderCounter++;
                    dbTestStep.StepExpected = null;

                    var errorMessage = testStep.error_message;
                    if (errorMessage != null)
                    {
                        dbTestStep.StepActual = testStep.error_message;
                    }
                    dbTestStep.SubSteps = parseCucumberTestSubStepDataNew(testStep.substeps);
                    dbTestStep.StepInputData = String.Empty;
                    if (testStep.StepAttachment != null && testStep.StepAttachment.Count != 0)
                    {
                        string attachments = null;
                        foreach (var embedding in testStep.StepAttachment)
                        {
                            if (embedding.content != null)
                            {
                                using (var ms = Base64ToMemoryStream(embedding.content))
                                {
                                    var attachName = (String.IsNullOrEmpty(embedding.name) ? Guid.NewGuid().ToString() : embedding.name) + "." + embedding.mime.Split('/').Last();
                                    using (var file = new FileStream(_newTempFolder + "\\" + attachName, FileMode.Create,
                                          FileAccess.Write))
                                    {
                                        ms.WriteTo(file);
                                    }
                                    if (attachments == null)
                                        attachments += attachName;
                                    else
                                        attachments += ";" + attachName;
                                }
                            }
                        }
                        dbTestStep.Attachments = attachments;
                    }

                    dbTestStep.UI = Guid.NewGuid();

                    dbTestSteps.Add(dbTestStep);
                }
                return dbTestSteps;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test step data: see inner exception for detailes", ex);
            }
        }

        private List<TestStep> parseTestStepData(GRS_ReportTestSetTestCaseStep[] testSteps)
        {
            try
            {
                var dbTestSteps = new List<TestStep>();

                foreach (var testStep in testSteps)
                {
                    var dbTestStep = new TestStep();

                    DateTime tempDateTime;
                    if (DateTime.TryParse(testStep.time, out tempDateTime))
                        dbTestStep.StepStartTime = tempDateTime;
                    dbTestStep.StepType = testStep.type;
                    dbTestStep.StepDescription = testStep.description;

                    dbTestStep.StepExpected = testStep.expected;
                    dbTestStep.StepActual = testStep.actual;
                    dbTestStep.StepInputData = testStep.inputdata;
                    dbTestStep.Attachments = testStep.attachment;
                    dbTestStep.UI = Guid.NewGuid();

                    dbTestStep.SubSteps = testStep.substep == null ? null : parseTestSubStepData(testStep.substep);

                    dbTestSteps.Add(dbTestStep);
                }
                return dbTestSteps;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test step data: see inner exception for detailes", ex);
            }
        }

        private List<SubStep> parseTestSubStepData(GRS_ReportTestSetTestCaseStepSubstep[] subSteps)
        {
            try
            {
                var dbSubSteps = new List<SubStep>();

                foreach (var subStep in subSteps)
                {
                    SubStep dbSubStep = new SubStep();

                    bool tempBool;
                    if (bool.TryParse(subStep.valid, out tempBool))
                        dbSubStep.SubStepValid = tempBool;
                    //
                    dbSubStep.SubStepTechInfo = subStep.tech_info;
                    DateTime tempDateTime;

                    if (DateTime.TryParse(subStep.time, out tempDateTime))
                        dbSubStep.SubStepTime = tempDateTime;
                    //
                    dbSubStep.SubStepScreenShot = subStep.screenshot;
                    dbSubStep.SubStepScreenShotDriver = subStep.screenshot_driver;
                    ////
                    //if (TestSubStep.Attributes["tech_info"] != null)
                    //    dbSubStep.SubStepTechInfo = TestSubStep.Attributes["tech_info"].Value;

                    dbSubStep.SubStepMessage = subStep.Value;

                    dbSubSteps.Add(dbSubStep);

                    //performAutoAnalysis(dbSubStep);
                }
                return dbSubSteps;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test substep data: see inner exception for detailes", ex);
            }
        }

        private List<SubStep> parseCucumberTestSubStepData(Cucumber.Result subStep)
        {
            try
            {
                var dbSubSteps = new List<SubStep>();
                var dbSubStep = new SubStep();
                dbSubStep.SubStepValid = false;
                DateTime tempDateTime;

                if (DateTime.TryParse(null, out tempDateTime))//re-write
                    dbSubStep.SubStepTime = tempDateTime;
                else
                    dbSubStep.SubStepTime = DateTime.Now;

                //dbSubStep.SubStepScreenShot = subStep.screenshot; //re-write
                //dbSubStep.SubStepScreenShotDriver = subStep.screenshot_driver;  //re-write

                dbSubStep.SubStepMessage = subStep.error_message;

                dbSubSteps.Add(dbSubStep);
                return dbSubSteps;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test substep data: see inner exception for detailes", ex);
            }
        }

        private List<SubStep> parseCucumberTestSubStepDataNew(IEnumerable<CucumberNew.Substep> subSteps)
        {
            try
            {
                if (subSteps != null)
                {
                    var dbSubSteps = new List<SubStep>();

                    foreach (var substep in subSteps)
                    {
                        var dbSubStep = new SubStep();

                        dbSubStep.SubStepValid = substep.status == "passed";

                        DateTime tempDateTime;

                        if (DateTime.TryParse(substep.date, out tempDateTime)) //re-write
                            dbSubStep.SubStepTime = tempDateTime;
                        else
                            dbSubStep.SubStepTime = DateTime.Now;

                        dbSubStep.SubStepMessage = substep.error_message;
                        dbSubStep.SubStepMessage = substep.message;

                        if (substep.SubstepAttachment != null && substep.SubstepAttachment.Count != 0)
                        {
                            string attachments = null;
                            foreach (var embedding in substep.SubstepAttachment)
                            {
                                if (embedding.content != null)
                                {
                                    using (var ms = Base64ToMemoryStream(embedding.content))
                                    {
                                        var attachName = (String.IsNullOrEmpty(embedding.name) ? Guid.NewGuid().ToString() : embedding.name) + "." + embedding.mime.Split('/').Last();
                                        using (
                                            var file = new FileStream(_newTempFolder + "\\" + attachName,
                                                FileMode.Create,
                                                FileAccess.Write))
                                        {
                                            ms.WriteTo(file);
                                        }
                                        if (attachments == null)
                                            attachments += attachName;
                                        else
                                            attachments += ";" + attachName;
                                    }
                                }
                            }
                            dbSubStep.SubStepScreenShot = attachments;
                        }

                        dbSubSteps.Add(dbSubStep);
                    }

                    return dbSubSteps;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test substep data: see inner exception for detailes", ex);
            }
        }

        private List<TestCase> parseCucumberTestCases(List<Cucumber.Element> testCases)
        {
            try
            {
                var listTestCases = new List<TestCase>();

                foreach (var testCase in testCases)
                {
                    var dbTestCase = new TestCase();

                    DateTime tempDateTime;
                    if (DateTime.TryParse(null/*testCase.start_time*/, out tempDateTime))//re-write
                        dbTestCase.TCStartTime = tempDateTime;
                    else
                        dbTestCase.TCStartTime = DateTime.Now;

                    if (DateTime.TryParse(null/*testCase.end_time*/, out tempDateTime))//re-write
                        dbTestCase.TCEndTime = tempDateTime;
                    else
                        dbTestCase.TCEndTime = DateTime.Now;

                    dbTestCase.TCState = testCase.steps.Count(p => p.result.status == "skipped") != 0 ? "notcompleted" :
                        (testCase.steps.Count(p => p.result.status == "failed") != 0 ? "fail" : "pass");

                    dbTestCase.TCName = string.Join("   ", testCase.tags.Select(p => (p.name as string))) + "\r\n" + testCase.name;
                    dbTestCase.Criticality = String.Empty;//re-write
                    dbTestCase.TCDescription = null;
                    dbTestCase.qc_id = null;//re-write
                    dbTestCase.UI = Guid.NewGuid();

                    dbTestCase.TestSteps = parseCucumberTestStepData(testCase.steps);

                    var tcAttachments = string.Join(";", dbTestCase.TestSteps.Select(p => p.Attachments).Where(p => p != null));

                    dbTestCase.TCAttachments = tcAttachments;

                    listTestCases.Add(dbTestCase);
                }
                return listTestCases;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test cases data: see inner exception for detailes", ex);
            }
        }

        private List<TestCase> parseCucumberTestCasesNew(List<CucumberNew.Test> testCases)
        {
            try
            {
                var listTestCases = new List<TestCase>();

                foreach (var testCase in testCases)
                {
                    var dbTestCase = new TestCase();

                    DateTime tempDateTime;
                    if (DateTime.TryParse(testCase.testcase.time, out tempDateTime))
                        dbTestCase.TCStartTime = tempDateTime;
                    else
                        dbTestCase.TCStartTime = DateTime.Now;

                    if (DateTime.TryParse(testCase.testcase.steps.Where(p => p.substeps != null).SelectMany(p => p.substeps.Select(b => b.date)).Max(), out tempDateTime))//re-write
                        dbTestCase.TCEndTime = tempDateTime;
                    else
                        dbTestCase.TCEndTime = DateTime.Now;

                    dbTestCase.TCState = testCase.testcase.steps.Count(p => p.status == "skipped") != 0 ? "notcompleted" :
                        (testCase.testcase.steps.Count(p => p.status == "failed") != 0 ? "fail" : "pass");

                    dbTestCase.TCName = string.Join("   ", testCase.testcase.tag) + "\r\n" + testCase.testcase.title;
                    dbTestCase.Criticality = String.Empty;//re-write
                    dbTestCase.TCDescription = null;
                    dbTestCase.qc_id = null;//re-write
                    dbTestCase.UI = Guid.NewGuid();

                    dbTestCase.TestSteps = parseCucumberTestStepDataNew(testCase.testcase.steps);

                    var tcAttachments = string.Join(";", dbTestCase.TestSteps.Select(p => p.Attachments).Where(p => p != null));

                    dbTestCase.TCAttachments = tcAttachments;

                    listTestCases.Add(dbTestCase);
                }
                return listTestCases;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test cases data: see inner exception for detailes", ex);
            }
        }

        private List<TestCase> parseTestCases(GRS_ReportTestSetTestCase[] testCases)
        {
            try
            {
                var listTestCases = new List<TestCase>();

                foreach (var testCase in testCases)
                {
                    var dbTestCase = new TestCase();

                    var tcAttachments = new List<string>();
                    if (testCase.step != null)
                    {
                        tcAttachments.AddRange(testCase.step.Select(p => p.attachment).Where(p => p != null));
                        tcAttachments.AddRange(testCase.step.Select(p => p.video).Where(p => p != null));
                    }
                    string tempString = "";
                    foreach (var tcAttachment in tcAttachments)
                    {
                        if (tempString.Trim() == "")
                            tempString = tcAttachment;
                        else
                            tempString = string.Concat(tempString, ";", tcAttachment);
                    }
                    dbTestCase.TCAttachments = tempString;
                    DateTime tempDateTime;
                    if (DateTime.TryParse(testCase.start_time, out tempDateTime))
                        dbTestCase.TCStartTime = tempDateTime;
                    if (DateTime.TryParse(testCase.end_time, out tempDateTime))
                        dbTestCase.TCEndTime = tempDateTime;
                    dbTestCase.TCState = testCase.state;
                    dbTestCase.TCName = testCase.name;
                    dbTestCase.Criticality = testCase.criticality;
                    dbTestCase.TCDescription = testCase.description;
                    dbTestCase.qc_id = testCase.qc_id;
                    dbTestCase.UI = Guid.NewGuid();

                    dbTestCase.TestSteps = testCase.step == null ? null : parseTestStepData(testCase.step);

                    listTestCases.Add(dbTestCase);
                }
                return listTestCases;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse and store test cases data: see inner exception for detailes", ex);
            }
        }

        /// <summary>
        /// Gets current cycle name (build/regression/sprint name) for pointed project
        /// </summary>
        /// <param name="ProjectName">Name of current project</param>
        /// <returns></returns>
        [WebMethod]
        public string currentCycle(string ProjectName)
        {
            TestCycle TC;
            if (!isDataBaseAvailable())
                return "";
            else
            {

                if (getProjectID(ProjectName) != 0)
                {
                    var Project = ServiceLocator.Current.GetInstance<IRepository<Project>>().GetSingleOrDefault(p => p.ProjectName.Equals(ProjectName));
                    TC = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetAllToList(p => p.Project.ID == Project.ID).SingleOrDefault(p => (bool)p.CycleIsCurrent);
                    if (TC == null)
                        return "";
                }
                else
                    throw new SoapException(ProjectName + " project is absent in Database!", SoapException.ServerFaultCode);
            }
            return TC.CycleName;
        }

        /// <summary>
        /// Check if current project exists in data base
        /// </summary>
        /// <param name="ProjectName"></param>
        /// <returns></returns>
        [WebMethod]
        public bool isProjectExists(string ProjectName)
        {
            return getProjectID(ProjectName) != 0;
        }

        /// <summary>
        /// Checks if database is available
        /// </summary>
        /// <returns></returns>
        private bool isDataBaseAvailable()
        {
            try
            {

            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Internal method that checks if current project exists in database
        /// </summary>
        /// <param name="ProjectName"></param>
        /// <returns></returns>
        private int getProjectID(string ProjectName)
        {
            var project =
                ServiceLocator.Current.GetInstance<IRepository<Project>>()
                    .GetSingleOrDefault(p => p.ProjectName.ToLower().Equals(ProjectName.ToLower()));
            return project == null ? 0 : project.ID;

        }

        private MemoryStream Base64ToMemoryStream(String line)
        {
            return new MemoryStream(Convert.FromBase64String(line.Replace(@"\", "")));
        }
    }
}
