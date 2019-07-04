using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;

namespace GlobalReportingSystem.Core.Abstract.ProviderInterfaces
{
    public interface IManageUserProvider
    {
        Guid AddUserToDb(AuthenticationModel model, string userHostAddress, string browser);

        AuthenticationModel SelectProjectsByUser(IPrincipal user);

        void SaveLastUsedProject(AuthenticationModel model, IPrincipal user);

        bool IsSetProject(IPrincipal user);
        bool RecoverPassowrd(string email);
        List<User> GetAllUsers();
        void SendNotification(string message);
        void RegisterNewUser(AuthenticationModel model);
        UserProfileModel GetUserProfile(IPrincipal user);
        void UpdateUserProfile(IPrincipal user, UserProfileModel model);
        List<UserResultsNotificationModel> getUserAccesses(IPrincipal user);
        void updateUserNotifications(IPrincipal user, int projectId);

        //================================================
        User GetUserById(IPrincipal user, int userid);
        //================================================
    }
}
