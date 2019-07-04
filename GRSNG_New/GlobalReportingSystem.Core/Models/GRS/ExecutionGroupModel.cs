using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class ExecutionGroupModel
    {
        public Int64 Id { get; set; }
        public string GroupName { get; set; }
    }

    public class AddExecutionGroupModel
    {
        [Required(ErrorMessage = "Enter Group Name")]
        public string GroupName { get; set; }
    }

    public class EditExecutionGroupModel
    {
        [Required(ErrorMessage = "Group Id not mapped")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Group name")]
        public string GroupName { get; set; }
    }

    public class ExecutionGroupClient_GetModel 
    {
        public Int64 ID { get; set;  }
        public Int64 ExecutionGroupId { get; set; }
        public Int64 ClientId{get; set;}
        public string MachintIp { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string WindowVersion { get; set; }
        public bool IsFirefoxExist { get; set; }
        public bool IsIEExist { get; set; }
        public bool IsChromeExist { get; set; }
    }
    public class ExecutionGroupClient_AddModel 
    {
        [Required(ErrorMessage="Please select Execution Group")]
        public Int64 ExecutionGroupId { get; set; }

        [Required (ErrorMessage = "Please select Client")]
        public Int64 ClientId { get; set; }
    }
    public class ExecutionGroupClient_UpdateModel 
    {
        [Required(ErrorMessage = "Please Select Exectuon Group Client")]
        public Int64 ID { get; set; }

        [Required(ErrorMessage = "Please select Execution Group")]
        public Int64 ExecutionGroupId { get; set; }

        [Required(ErrorMessage = "Please select Client")]
        public Int64 ClientId { get; set; }
    }
    public class ExecutionGroupClient_DeleteModel 
    {
        [Required(ErrorMessage = "Please Select Exectuon Group Client")]
        public Int64 ID { get; set; }
    }

}
