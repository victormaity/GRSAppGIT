using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class AdminToolGlobalNotificationsModel
    {
        public string Message { get; set; }
    }

    public class AdminToolCreateNewUserModel
    {
        [Required(ErrorMessage = "Please enter Username.")]
        [MaxLength(500, ErrorMessage = "Please enter maximum of 50 charactor.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter User Full Name.")]
        [MaxLength(100, ErrorMessage = "Please enter maximum of 100 charactor.")]
        public string UserFullName { get; set; }
        [Required(ErrorMessage = "Please enter User Email Address")]
        [MaxLength(100, ErrorMessage = "Please enter maximum of 100 charactor.")]
        [EmailAddress(ErrorMessage = "Email address is not correct. Please enter User Email Address.")]
        public string UserEmail { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsGlobalAdmin { get; set; }
        [Required(ErrorMessage = "Please select Project.")]
        public int ProjectID { get; set; }

        public List<PList> ProjectLists { get; set; }
    }

    public class PList
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class AdminToolAddNewProject
    {
        [Required(ErrorMessage = "Please enter Project Name")]
        [MaxLength(100, ErrorMessage = "Please enter maximum of 100 charactor without space")]
        //[RegularExpression(@"^[a-zA-Z0-9_-]$", ErrorMessage = "Alplabet in caps and small case, number from 0-9, underscore (_) and hifen (-) are allowed only.")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Please enter project display Name")]
        [MaxLength(100, ErrorMessage = "Please enter maximum of 100 charactor")]
        public string DisplayName { get; set; }

        [MaxLength(500, ErrorMessage = "Please enter maximum of 500 charactor")]
        public string QCLocation { get; set; }

        [MaxLength(300, ErrorMessage = "Please enter maximum of 300 charactor")]
        public string QCServer { get; set; }

        public bool Notification_ResultsDelivery { get; set; }

        //[RegularExpression(@"^[ 0-9]$", ErrorMessage = "Please enter number only")]
        //public string AssignedBugTracker { get; set; }

        public string CustomStatuses { get; set; }

        [MaxLength(500, ErrorMessage = "Please enter maximum of 500 charactor")]
        public string QcResultsPath { get; set; }

        [MaxLength(500, ErrorMessage = "Please enter maximum of 500 charactor")]
        public string QCTestPlan { get; set; }

        public bool AutoAnalysis { get; set; }

        public bool isPublic { get; set; }
        public bool isGUI { get; set; }
        public bool IsGITDefault { get; set; }

        [MaxLength(500, ErrorMessage = "Please enter maximum of 500 charactor")]
        public string GITPath { get; set; }

        [MaxLength(500, ErrorMessage = "Please enter maximum of 500 charactor")]
        public string SVNpath { get; set; }

        [MaxLength(50, ErrorMessage = "Please enter maximum of 50 charactor")]
        public string SVNlogin { get; set; }

        [MaxLength(50, ErrorMessage = "Please enter maximum of 50 charactor")]
        public string SVNpassword { get; set; }

        public string GITUSER { get; set; }
        public string GITPASSWORD { get; set; }

        [Required(ErrorMessage = "Please select Project Type.")]
        public int ProjectTypeID { get; set; }

        public List<PList> ProjectTypeList { get; set; }

    }

    public class ErrorControl
    {
        public int ErrorId { get; set; }
        public string ErrorMessage { get; set; }
    }

}
