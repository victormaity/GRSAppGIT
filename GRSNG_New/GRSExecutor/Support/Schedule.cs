using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using GRSExecutor;
using GRSExecutor.Support;

namespace GAgent
{
    public class Schedules : Serializer<Schedules>
    {
        public List<Schedule> _Schedules { get; set; }
    }
    public class Schedule : Serializer<Schedule>
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public DateTime ShouldBeStarted { get; set; }
        public string FrameworkLocation { get; set; }
        public string TestsToRun { get; set; }
        public string CategoriesInclude { get; set; }
        public string CategoriesExclude { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public ConfigurationInfoData configurations { get; set; }
    }

    public class ScheduleDetails : Serializer<ScheduleDetails>
    {
        public DateTime ShouldBeStarted { get; set; }
    }
}
