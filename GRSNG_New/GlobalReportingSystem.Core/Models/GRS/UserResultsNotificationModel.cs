using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class UserResultsNotificationModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool DeliveryResult { get; set; }
    }
}
