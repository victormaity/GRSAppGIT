using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GRSHarvest.CucumberNew
{
    public class SubstepAttachment
    {
        public string mime { get; set; }
        public string name { get; set; }
        public string content { get; set; }
    }

    public class Substep
    {
        public string date { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
        public List<SubstepAttachment> SubstepAttachment { get; set; }
        public string time { get; set; }
    }

    public class StepAttachment
    {
        public string mime { get; set; }
        public string name { get; set; }
        public string content { get; set; }
    }

    public class Step
    {
        public List<Substep> substeps { get; set; }
        public List<StepAttachment> StepAttachment { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
    }

    public class Testcase
    {
        public string tag { get; set; }
        public string time { get; set; }
        public string title { get; set; }
        public List<Step> steps { get; set; }
        public string status { get; set; }
    }

    public class Test
    {
        public Testcase testcase { get; set; }
    }

    public class Testset
    {
        public List<Test> tests { get; set; }
        public string ip { get; set; }
        public string name { get; set; }
        public string time { get; set; }
        public string url { get; set; }
        public string targettestcycle { get; set; }
        public string os { get; set; }
        public string browser { get; set; }
    }

    public class CucumberReportNew
    {
        public Testset testset { get; set; }
    }
}