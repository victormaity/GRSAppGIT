using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class FeatureTagsFromLocalProjectFolder
    {
    }

    public class FeatureFromFolder
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Tags { get; set; }
        public IList<ScenarioFromFolder> Scenarios { get; set; }
    }

    public class ScenarioFromFolder
    {
        public string Name { get; set; }
        public string Tags { get; set; }
        public IList<ScenarioLineFromFolder> ScenarioLines { get; set; }
        public string ExamplesTable { get; set; }
    }

    public class ScenarioLineFromFolder
    {
        public string Line { get; set; }
        public int OrderId { get; set; }
    }

    public class FeatureReturn
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class FeatureAndTags
    {
        public IEnumerable<FeatureReturn> Features { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int FeatureErrorId { get; set; }
        public string FeatureErrorMessage { get; set; }
        public int TagsErrorId { get; set; }
        public string TagsErrorMessage { get; set; }
        public int GeneralErrorId { get; set; }
        public string GeneralErrorMessage { get; set; }
    }

    public class ResponseResult
    {
        public int FeatureErrorId { get; set; }
        public string FeatureErrorMessage { get; set; }
        public int TagErrorId { get; set; }
        public string TagErrorMessage { get; set; }
        public int GeneralErrorId { get; set; }
        public string GeneralErrorMessage { get; set; }
    }

}
