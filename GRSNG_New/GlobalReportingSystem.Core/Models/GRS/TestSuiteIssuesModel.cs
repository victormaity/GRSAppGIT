using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
  public  class TestSuiteIssuesModel
    {
      public int Id { get; set; }
      public List<IGrouping<string, TestCasesModel>> Duplicates { get; set; }
      public List<CycleDuplicates> CycleDuplicates { get; set; }
    }

    public class CycleDuplicates
    {
        public string TestSuiteName { get; set; }
        public int CountOfDuplicates { get; set; }
        public int TestSuiteId { get; set; }
    }
}
