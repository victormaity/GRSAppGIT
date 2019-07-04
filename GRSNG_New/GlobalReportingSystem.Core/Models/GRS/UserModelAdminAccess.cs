using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalReportingSystem.Core.Models.GRS
{
    public class UserModelAdminAccess
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public bool UserAdmin { get; set; }
        public bool UserGlobalAdmin { get; set; }
        public bool UserBlocked { get; set; }
    }

    public class UserListModel
    {
        public int ErrorId { get; set; }
        public string ErrorMessage { get; set; }
        public List<UserModelAdminAccess> UserModelAdminAccess { get; set; }
    }

    public class ProjectListModel
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }
    }
}
