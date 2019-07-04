using GlobalReportingSystem.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class TeamInfoModel
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public string Comment { get; set; }
        public List<TeamInfo> TeamFromDb { get; set; }
    }

    public class ReleaseInfoModel
    {
        public int Id { get; set; }
        public string ReleaseName { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Comment { get; set; }
        public List<ReleaseInfo> ReleaseFromDb { get; set; }
    }
}
