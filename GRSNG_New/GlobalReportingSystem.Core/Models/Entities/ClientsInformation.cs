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
    
    public partial class ClientsInformation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientsInformation()
        {
            this.TestSuits = new HashSet<TestSuit>();
        }
    
        public int ID { get; set; }
        public string ClientURL { get; set; }
        public string ClientEnvironment { get; set; }
        public string ClientUser { get; set; }
        public string ClientIP { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public string ClientBrowser { get; set; }
        public string ClientOS { get; set; }
        public string ClientFreeSpace { get; set; }
        public System.Guid UI { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestSuit> TestSuits { get; set; }
        public virtual ClientsInformation ClientsInformation1 { get; set; }
        public virtual ClientsInformation ClientsInformation2 { get; set; }
    }
}
