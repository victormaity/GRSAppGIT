using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using Castle.Core.Internal;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using Microsoft.Practices.ServiceLocation;

namespace GRSAnalyzer
{
    /// <summary>
    /// Summary description for AnalysisService
    /// </summary>
   /* [WebService(Namespace = "http://tempuri.org/")]*/
    [WebService(Namespace = "http://microsoft.com/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class AnalysisService : System.Web.Services.WebService
    {
        [WebMethod(EnableSession = false)]
        public void PerformAutoMigration(List<int> testSuiteIds, int migrationType)
        {
            var tsId = testSuiteIds.FirstOrDefault();
            var ts = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetFirstOrDefault(p => p.ID == tsId);

            var analysisCacheForProject =
                ServiceLocator.Current.GetInstance<IRepository<AutoAnalysisCache>>().GetAllToList(p => p.ProjectID == ts.Project.ID);
            
            foreach (var testSuiteId in testSuiteIds)
            {
                var allSubStepsItems = ServiceLocator.Current.GetInstance<IRepository<SubStep>>();
                //var allSubSteps = allSubStepsItems.GetAll(p => p.TestStep.TestCase.TestSuit.ID == testSuiteId && !p.SubStepValid).ToList();

                //var notAnalyzedSubSteps = allSubSteps.Where(p => p.AnalyzedStatus == null).ToList();

                //var notAnalyzedSubSteps22 = allSubStepsItems.GetFirstOrDefault(p => p.AnalyzedStatus == null);
                //notAnalyzedSubSteps22.Defects = "CheckSave";
                //allSubStepsItems.SaveChanges();
                var notAnalyzedSubSteps = allSubStepsItems.GetAll(p => p.TestStep.TestCase.TestSuit.ID == testSuiteId && !p.SubStepValid && p.AnalyzedStatus == null);

                foreach (var notAnalyzedSubStep in notAnalyzedSubSteps)
                {
                    var message = UpdateByRegEx(notAnalyzedSubStep.SubStepMessage);
                    var cacheAnalogueAllRules = analysisCacheForProject.FirstOrDefault(p => UpdateByRegEx(p.ErrorMessage) == message && p.TestSuiteName == notAnalyzedSubStep.TestStep.TestCase.TestSuit.TSName && p.TestCaseName == notAnalyzedSubStep.TestStep.TestCase.TCName);
                    if (cacheAnalogueAllRules != null)
                    {
                        notAnalyzedSubStep.AnalyzedStatus = cacheAnalogueAllRules.Status;
                        notAnalyzedSubStep.Defects = cacheAnalogueAllRules.DefectID;
                        notAnalyzedSubStep.TestStep.TestCase.isAnalyzed = true;
                    }
                    else if (migrationType != 1)
                    {
                        var cacheAnalogueTestSuiteOnly = analysisCacheForProject.FirstOrDefault(p => UpdateByRegEx(p.ErrorMessage) == message && p.TestSuiteName == notAnalyzedSubStep.TestStep.TestCase.TestSuit.TSName);
                        if (cacheAnalogueTestSuiteOnly != null)
                        {
                            notAnalyzedSubStep.AnalyzedStatus = cacheAnalogueTestSuiteOnly.Status;
                            notAnalyzedSubStep.Defects = cacheAnalogueTestSuiteOnly.DefectID;
                            notAnalyzedSubStep.TestStep.TestCase.isAnalyzed = true;
                        }
                        else if (migrationType != 2)
                        {
                            var cacheAnalogueEvrything = analysisCacheForProject.FirstOrDefault(p => UpdateByRegEx(p.ErrorMessage) == message);
                            if (cacheAnalogueEvrything != null)
                            {
                                notAnalyzedSubStep.AnalyzedStatus = cacheAnalogueEvrything.Status;
                                notAnalyzedSubStep.Defects = cacheAnalogueEvrything.DefectID;
                                notAnalyzedSubStep.TestStep.TestCase.isAnalyzed = true;
                            }
                        }
                    }
                }
                //notAnalyzedSubSteps.SaveChanges();
                //var analyzedSubSteps = allSubSteps.Where(p => p.AnalyzedStatus != null).ToList();

                var analyzedSubSteps = allSubStepsItems.GetAll(p => p.TestStep.TestCase.TestSuit.ID == testSuiteId && !p.SubStepValid && p.AnalyzedStatus != null);
                foreach (var analyzedSubStep in analyzedSubSteps)
                {
                    var message = UpdateByRegEx(analyzedSubStep.SubStepMessage);
                    var cacheAnalogueAllRules = analysisCacheForProject.FirstOrDefault(p => UpdateByRegEx(p.ErrorMessage) == message && p.TestSuiteName == analyzedSubStep.TestStep.TestCase.TestSuit.TSName && p.TestCaseName == analyzedSubStep.TestStep.TestCase.TCName);
                    if (cacheAnalogueAllRules != null)
                    {
                        cacheAnalogueAllRules.Status = analyzedSubStep.AnalyzedStatus;
                        cacheAnalogueAllRules.DefectID = analyzedSubStep.Defects;
                    }/*
                    else
                    {
                        var cacheAnalogueTestSuiteOnly = analysisCacheForProject.FirstOrDefault(p => p.ErrorMessage == message && p.TestSuiteName == analyzedSubStep.TestStep.TestCase.TestSuit.TSName);
                        if (cacheAnalogueTestSuiteOnly != null)
                        {
                            cacheAnalogueTestSuiteOnly.Status = analyzedSubStep.AnalyzedStatus;
                            cacheAnalogueTestSuiteOnly.DefectID = analyzedSubStep.Defects;
                        }
                        else
                        {
                            var cacheAnalogueEvrything = analysisCacheForProject.FirstOrDefault(p => p.ErrorMessage == message);
                            if (cacheAnalogueEvrything != null)
                            {
                                cacheAnalogueEvrything.Status = analyzedSubStep.AnalyzedStatus;
                                cacheAnalogueEvrything.DefectID = analyzedSubStep.Defects;
                            }
                        }
                    }*/
                    //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
                }
                allSubStepsItems.SaveChanges();
            }
        }

        public string UpdateByRegEx(string message)
        {
            message = Regex.Replace(message, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", "");
            message = Regex.Replace(message, @"\d+\.\d+", "");
            message = Regex.Replace(message, @":\d+", "");
            return message;
        }
    }
}
