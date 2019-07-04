using System.Collections.Generic;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class UsersAndCyclesModel
    {
        public List<TestCycle> Cycles { get; set; }
        public List<User> Analysers { get; set; }
    }

    public class UsersAndTestCasesModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public List<TestCasesAndCycle> TestCasesAndCycle { get; set; }
    }
    public class TestCasesAndCycle
    {
        public int ParentTestCycleId { get; set; }
        public string ParentTestCycleName { get; set; }
        public List<TestCase> TestCases { get; set; }
    }
}