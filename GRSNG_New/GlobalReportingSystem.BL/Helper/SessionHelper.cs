using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
//using GlobalReportingSystem.Data.DB;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.BL.Helper
{
    public class SessionHelper : ISessionHelper
    {
        private readonly IRepository<UserSession> _userSessionRepository;

        private readonly IRepository<TestCycle> _testCycleRepository;

        public SessionHelper(IRepository<UserSession> userSessionRepository, IRepository<TestCycle> testCycleRepository)
        {
            _userSessionRepository = userSessionRepository;
            _testCycleRepository = testCycleRepository;
        }

        public User GetUser(IPrincipal user)
        {
            return GetUserSession(user).User;
        }
        private UserSession GetUserSession(IPrincipal user)
        {
            var guid = Guid.Parse(user.Identity.Name);
            return _userSessionRepository.GetFirstOrDefault(p => p.SessionID == guid);
        }
        public UserSession GetUserSession_Services(IPrincipal user)
        {
            var guid = Guid.Parse(user.Identity.Name);
            return _userSessionRepository.GetFirstOrDefault_Services(p => p.SessionID == guid);
        }
        public UserSession GetSessionDetails(IPrincipal user)
        {
            return GetUserSession(user);
        }

        public List<TestCycle> GetActiveTestCycles(IPrincipal user)
        {
            var session = GetUserSession(user);
            return session.Project.TestCycles.Where(p => !p.isInnactive.HasValue || !p.isInnactive.Value).ToList();
        }

        public bool IsSessionExist(IPrincipal user)
        {
            return GetUserSession(user) != null;
        }
        public void SaveChanges()
        {
            _userSessionRepository.SaveChanges();
        }
        /*public static User GetUser(IPrincipal user)
        {
            using (var db = new GRSDataBaseEntities())
            {
                return db.\(user).User;
            }
        }
        public static UserSession GetSessionDetails(IPrincipal user)
        {
            using (var db = new GRSDataBaseEntities())
            {
                var guid = Guid.Parse(user.Identity.Name);
                var session = db.UserSessions
                  .Include(p => p.Project)
                  .FirstOrDefault(p => p.SessionID == guid);
                return session;
            }
        }

        public static List<TestCycle> GetActiveTestCycles(IPrincipal user)
        {
            using (var db = new GRSDataBaseEntities())
            {
                var session = db.GetSession(user);
                return session.Project.TestCycles.Where(p => !p.isInnactive.HasValue || !p.isInnactive.Value).ToList();
            }
        }

        public static bool IsSessionExist(IPrincipal user)
        {
            using (var db = new GRSDataBaseEntities())
            {
                var guid = Guid.Parse(user.Identity.Name);
                return db.UserSessions.Count(p => p.SessionID == guid) != 0;
            }
        }*/       
    }
}