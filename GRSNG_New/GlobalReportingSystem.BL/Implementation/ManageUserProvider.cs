using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Security;
using System.Net.Mail;
using GlobalReportingSystem.BL.Properties;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using Microsoft.Practices.ServiceLocation;
using Messages = GlobalReportingSystem.Core.Constants.Messages;

namespace GlobalReportingSystem.BL.Implementation
{
    public class ManageUserProvider : IManageUserProvider
    {
        private readonly IRepository<User> _userRepository;

        private readonly IRepository<Project> _projectRepository;

        private readonly ISessionHelper _sessionHelper;

        private readonly IEmailer _emailer;

        public ManageUserProvider(IRepository<User> userRepository, IRepository<Project> projectRepository,
                                  ISessionHelper sessionHelper, IEmailer emailer)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _sessionHelper = sessionHelper;
            _emailer = emailer;
        }

        public Guid AddUserToDb(AuthenticationModel model, string userHostAddress, string browser)
        {
            var user =
                _userRepository.GetFirstOrDefault(p => p.UserName == model.Login && p.UserPassword == model.Password);
            if (user == null)
            {
                throw new Exception(Messages.UserOrPasswordDoesNotMatchException);
            }

            if (user.UserBlocked == true)
            {
                throw new Exception(Messages.AccountHasBlocked);
            }

            if (user.Accesses.Count().Equals(0))
            {
                throw new Exception(Messages.AccountHasNotSubscriptionsException);
            }
            var sessionGuid = Guid.NewGuid();
            user.UserSessions.Add(new UserSession
                {
                    SessionOwner = user.ID,
                    SessionIP = userHostAddress,
                    SessionBrowser = browser,
                    SessionStartDate = DateTime.Now,
                    SessionID = sessionGuid,
                    SessionEndDate = DateTime.MaxValue
                });
            _userRepository.SaveChanges();
            //Send notification
            return sessionGuid;
        }

        public AuthenticationModel SelectProjectsByUser(IPrincipal user)
        {
            var model = new AuthenticationModel();


            var session = _sessionHelper.GetSessionDetails(user);
            if (session.User.UserGlobalAdmin)
            {
                model.ProjectIds =
                    _projectRepository.GetAllToList()
                                      .Select(p => new ProjectId { Id = p.ID, ProjectName = p.DisplayName })
                                      .ToList();
            }
            else
            {
                model.ProjectIds =
                    session.User.Accesses.Select(
                        p => new ProjectId { Id = p.Project.ID, ProjectName = p.Project.DisplayName }).ToList();
            }
            model.ProjectIds = model.ProjectIds.OrderBy(p => p.ProjectName).ToList();
            return model;
        }

        public void SaveLastUsedProject(AuthenticationModel model, IPrincipal user)
        {
            var realId = int.Parse(model.SelectedProject);
            _sessionHelper.GetSessionDetails(user).LinkedProject = realId;
            _sessionHelper.SaveChanges();
        }

        public bool IsSetProject(IPrincipal user)
        {
            return _sessionHelper.GetSessionDetails(user).Project != null;
        }

        public bool RecoverPassowrd(string email)
        {
            var user = _userRepository.GetFirstOrDefault(p => p.UserEmail == email);
            if (user == null)
                return false;
            ServiceLocator.Current.GetInstance<IEmailer>()
                          .SendMail(new List<string> { user.UserEmail }, "grs@clarivate.com",
                                    "[Global Reporting System - password recovery]",
                                    Resources.EmailTemplatePasswordRecovery.Replace("%user_name%", user.USerFullName)
                                             .Replace("%password%", user.UserPassword));
            return true;
        }

        public List<User> GetAllUsers()
        {
            return _userRepository.GetAllToList().ToList();

        }

        public void SendNotification(string message)
        {
            var allUsers = _userRepository.GetAllToList(p => p.UserEmail != null).ToList();
            foreach (var user in allUsers)
            {
                ServiceLocator.Current.GetInstance<IEmailer>()
                          .SendMail(new List<string> { user.UserEmail }, "grs.notifications@thomsonreuters.com",
                                    "[Global Reporting System - System notification]",
                                    Resources.EmailTemplateCommon.Replace("%user_name%", user.USerFullName)
                                             .Replace("%text%", message.Replace(Environment.NewLine, "<br />")));
            }
        }

        public void RegisterNewUser(AuthenticationModel model)
        {
            var userEmail =
                 _userRepository.GetFirstOrDefault(p => p.UserEmail == model.Email);
            if (userEmail != null)
            {
                throw new Exception(string.Format("User with email {0} was already registered. Please use restore password feature.", userEmail.UserEmail));
            }

            var userLogin =
                _userRepository.GetFirstOrDefault(p => p.UserName == model.Login);
            if (userLogin != null)
            {
                throw new Exception(string.Format("Login {0} was already taken. Please choose anoter one.", userLogin.UserName));
            }

            var newPassword = Membership.GeneratePassword(8, 1);
            newPassword = Regex.Replace(newPassword, @"[^a-zA-Z0-9]", m => "9");
            _userRepository.Add(new User
                {
                    UserAdmin = false,
                    UserEmail = model.Email,
                    UserName = model.Login,
                    USerFullName = model.FullName,
                    UserPassword = newPassword
                });
            _userRepository.SaveChanges();
            //Send email
            var message = @"Your new GRS account is being created.<br/>Your login is <b>" + model.Login + "</b><br/>Your temporary passowrd is <b>" + newPassword + @"</b> <br/> please change it during next login.<br/><b>WARNING:</b> for the moment your account does not have any project's subscriptions. Please contact project leads for subscriptions.";
            ServiceLocator.Current.GetInstance<IEmailer>()
                         .SendMail(new List<string> { model.Email }, "grs.notifications@thomsonreuters.com",
                                   "[Global Reporting System - System notification]",
                                   Resources.EmailTemplateCommon.Replace("%user_name%", model.FullName)
                                            .Replace("%text%", message.Replace(Environment.NewLine, "<br />")));
        }

        public UserProfileModel GetUserProfile(IPrincipal user)
        {
            var userProfile = _sessionHelper.GetSessionDetails(user).User;
            return new UserProfileModel()
            {
                Id = userProfile.ID,
                FullName = userProfile.USerFullName,
                Email = userProfile.UserEmail,
                Password = userProfile.UserPassword,
                QCLogin = userProfile.UserQCLogin,
                QCPassword = userProfile.UserQCPassword
            };
        }

        public void UpdateUserProfile(IPrincipal user, UserProfileModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                throw new Exception("Password can't be empty!");
            }
            try
            {
                var mail = new MailAddress(model.Email);
            }
            catch (Exception ex)
            {
                throw new Exception("Provided email is not valid!");
            }
            var userProfile = _sessionHelper.GetSessionDetails(user).User;
            userProfile.UserEmail = model.Email;
            userProfile.USerFullName = model.FullName;
            userProfile.UserPassword = model.Password;
            userProfile.UserQCLogin = model.QCLogin;
            userProfile.UserQCPassword = model.QCPassword;
            _sessionHelper.SaveChanges();
        }

        public List<UserResultsNotificationModel> getUserAccesses(IPrincipal user)
        {
            var userProfile = _sessionHelper.GetSessionDetails(user).User;
            var userAccesses = new List<UserResultsNotificationModel>();
            foreach (var group in userProfile.Accesses.GroupBy(p => p.AttachedProject))
            {
                if (group.Count() > 1)
                {
                    foreach (var element in group.Skip(1))
                    {
                        userProfile.Accesses.Remove(element);
                    }
                }
            }
            _sessionHelper.SaveChanges();
            foreach (var access in userProfile.Accesses)
            {
                if (access.AttachedProject != null && access.Project.Notification_ResultsDelivery.HasValue && (bool)access.Project.Notification_ResultsDelivery)
                {
                    userAccesses.Add(
                        new UserResultsNotificationModel
                        {
                            ProjectId = (int)access.AttachedProject,
                            ProjectName = access.Project.DisplayName,
                            DeliveryResult = access.DeliveryResult
                        }
                        );
                }
            }
            //if(userProfile.UserGlobalAdmin)
            //{
            //    var tmp = userProfile.Accesses.Select(t => t.AttachedProject);
            //    var projects = _projectRepository.GetAllToList(p => !tmp.Contains(p.ID));
            //    foreach(var project in projects)
            //    {
            //        if (project.Notification_ResultsDelivery.HasValue && (bool)project.Notification_ResultsDelivery)
            //        {
            //            userAccesses.Add(new UserResultsNotificationModel
            //                {
            //                    ProjectId = project.ID,
            //                    ProjectName = project.DisplayName,
            //                    DeliveryResult = true
            //                });
            //        }
            //    }
            //}
            return userAccesses;
        }

        public void updateUserNotifications(IPrincipal user, int projectId)
        {
            var userProfile = _sessionHelper.GetSessionDetails(user).User;
            var access = userProfile.Accesses.Where(p => p.AttachedProject != null && p.AttachedProject == projectId).ToList();
            
            //if (access.Count == 0)
            //{
            //    userProfile.Accesses.Add(
            //        new Access
            //        {
            //            AttachedProject = projectId,
            //            DeliveryResult = false
            //        }
            //        );
            //}
            //else
            //{
                access.First().DeliveryResult = !access.First().DeliveryResult;
            //}
            _sessionHelper.SaveChanges();
        }

        //=======================================
        public User GetUserById(IPrincipal user, int userid)
        {
            var requesterUser = _sessionHelper.GetSessionDetails(user);
            if (requesterUser.User.UserGlobalAdmin)
            {
                return _userRepository.GetSingleOrDefault(u => u.ID == userid);
            }
            else
            {
                return new User();
            }
        }
        //======================================
    }
}
