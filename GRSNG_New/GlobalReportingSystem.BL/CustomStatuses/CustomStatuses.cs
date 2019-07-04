using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using GlobalReportingSystem.BL.Helper;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.BL.CustomStatuses
{
    public class CustomStatuses : Serializer<GlobalReportingSystem.Core.Models.GRS.DB.CustomStatuses>
    {
        private readonly int _projectId;

        //  private readonly List<Status> Statuses; 

        private readonly IRepository<Project> _projectRepository;

        public CustomStatuses(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public CustomStatuses(int projectId, IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
            this._projectId = projectId;
            var _project = _projectRepository.GetSingleOrDefault_Services(p => p.ID == projectId);
            if (_project != null)
            {
                this.Statuses = _project.CustomStatuses != null ? Deserialize(_project.CustomStatuses).Statuses : new List<Status>();
            }
        }

        public void Save()
        {
            var Proj = _projectRepository.GetSingleOrDefault(p => p.ID == _projectId);
            if (Proj != null)
            {
                Proj.CustomStatuses = this.Serialize();
                _projectRepository.SaveChanges();
            }
        }

        public List<Status> Statuses { get; set; }

        public List<Counts> GetStatusForTestSuite(List<vw_AnalyzedSubSteps> TC, bool renderDefects, CustomStatuses availableStatuses, out List<string> Defects, out int analyzed)
        {
            int tempA = 0;
            var defects = new List<string>();
            var ret = new List<Counts>();
            //var tsts = TS.TestCases.Where(p => p.TCState != "pass").ToList();
            //var tsts = TC.Where(p => p.TCState != "pass" || (p.TCState == "pass" && p.isAnalyzed)).ToList();//!!!!!!!!!!!!!!!!!!!
            foreach (var test in TC.GroupBy(p => p.TestCaseID))
            {
                tempA += test.Count(d => d.AnalyzedStatus != null) != 0 ? 1 : 0;
                            
                var statusIDsFromTest = new List<Guid>();
                test.Where(p => !string.IsNullOrEmpty(p.Defects)).ToList().ForEach(p => defects.AddRange(p.Defects.Split(',')));
                test.ForEach(p => statusIDsFromTest.Add(p.AnalyzedStatus.Value));
                statusIDsFromTest = statusIDsFromTest.Distinct().ToList();
                //Intify what status has largest priority;
                var status = getTopPriorityStatus(statusIDsFromTest, availableStatuses);

                //Check if status was alaredy added
                if (status != null)
                {
                    var alreadyAddedStatuses = ret.Select(p => p.Status).Select(p => p.UniqueID).ToList();
                    if (alreadyAddedStatuses.Contains(status.UniqueID))
                    {
                        ret.Single(p => p.Status.UniqueID == status.UniqueID).Count++;
                    }
                    else
                    {

                        ret.Add(new Counts
                        {
                            Count = 1,
                            Status = status

                        });
                    }
                }
            }
            Defects = defects.Distinct().ToList();
            analyzed = tempA;
            return ret;
        }

        public Status getTopPriorityStatus(List<Guid> Ids, CustomStatuses AvailableStatuses)
        {
            var theStat = AvailableStatuses.Statuses.OrderByDescending(p => p.Priority);
            foreach (var stat in theStat)
            {
                if (Ids.Contains(stat.UniqueID))
                    return stat;
            }
            return null;

        }

        public Status getStatus(Guid guid)
        {

            return Statuses.Find(p => p.UniqueID == guid);
        }
    }
}