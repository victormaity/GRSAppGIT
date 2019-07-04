using System.Collections.Generic;
using GlobalReportingSystem.Core.Models.Entities;
using System;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class CyclesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Comments { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsDeactivated { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseDate { get; set; }
        public string TeamName { get; set; }
        public Int64 ReleaseId { get; set; }
        public Int64 TeamId { get; set; }

        public List<TestCycle> CyclesFromDb { get; set; }
        public List<DropdownList> TeamFromDb { get; set; }
        public List<DropdownList> ReleaseFromDb { get; set; }
    }

    public class ForPartial
    {
        public TestCycle CycleData { get; set; }
        public List<DropdownList> TeamFromDb { get; set; }
        public List<DropdownList> ReleaseFromDb { get; set; }
    }

    public class DropdownList
    {
        public Int64 Value { get; set; }
        public string Text { get; set; }
    }
}