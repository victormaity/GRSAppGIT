using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ProjectConfigurationModel
    {
        public int Id { get; set; }
        //[Required (ErrorMessage="Please enter Project Identificator.")]
        //[MaxLength (100, ErrorMessage="Please enter maximum of 100 charactor.")]
        //[RegularExpression (@"^[A-Za-z0-9_-]$", ErrorMessage="Alphabet in caps and small case, numbers, underscore ( - ) and hyphen (-) are allowed only.")]
        public string ProjectId { get; set; }
        //[Required(ErrorMessage = "Please enter Display Name.")]
        //[MaxLength(100, ErrorMessage = "Please enter maximum of 100 charactor.")]
        public string DisplayName { get; set; }
        public string QcLocation { get; set; }
        public string QcDomProj { get; set; }
        public string QcResultsPath { get; set; }
        public string QcTestPlanPath { get; set; }
        public bool QcRewriteTestPlan { get; set; }
        public string SvnRepo { get; set; }
        public string SvnUser { get; set; }
        public string SvnPassword { get; set; }
        public string GITPath { get; set; }
        public string GITUser { get; set; }
        public string GITPassword { get; set; }
        public bool IsGITDefaultAccount { get; set; }
        public int CurrentBugTracker { get; set; }
        public bool EmailNotification { get; set; }
        public List<IssueTracker> Bugtrackers { get; set; }
        public int Selectedtab { get; set; }
    }

    public class IssueTracker
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
