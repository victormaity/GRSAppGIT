using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalReportingSystem.Core.Models.GRS.DB
{
    public class CustomStatuses
    {
        public List<Status> Statuses { get; set; }
    }

    public class Counts
    {
        public int Count { get; set; }
        public Status Status { get; set; }
        public int Total { get; set; }
    }
    public class Status
    {
        public Guid UniqueID { get; set; }
        public int Priority { get; set; }
        public string StatusID { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public bool CountAsPass { get; set; }
        public bool NotAddToCache { get; set; }
    }

    public class Total
    {
        public string Browser { get; set; }
        public int Passes { get; set; }
        public int Na { get; set; }
        public int TotalTotal { get; set; }
        public Counts customCounts { get; set; }
    }
}