//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GlobalReportingSystem.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class vw_Defects
    {
        public string Defects { get; set; }
        public int TCycleID { get; set; }
        public string Bugtracker { get; set; }
        public int TCId { get; set; }
        public string TCName { get; set; }
        public string TCDescription { get; set; }
        public string qc_id { get; set; }
        public string TSName { get; set; }
    }
}
