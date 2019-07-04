using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Web.API.Models.Project;

namespace GlobalReportingSystem.Web.API.Controllers
{
    public class ProjectController : ApiController
    {
        private readonly IRepository<Project> projectRepository;

        private readonly IRepository<HostsConfiguration> hostsConfigurationRepository;

        private readonly IRepository<TestCycle> testCycleRepository;

        private readonly IRepository<AccountForTestRun> accountForTestRunRepository;

        public ProjectController(IRepository<Project> projectRepository,
            IRepository<HostsConfiguration> hostsConfigurationRepository,
            IRepository<TestCycle> testCycleRepository,
            IRepository<AccountForTestRun> accountForTestRunRepository)
        {
            this.projectRepository = projectRepository;
            this.hostsConfigurationRepository = hostsConfigurationRepository;
            this.testCycleRepository = testCycleRepository;
            this.accountForTestRunRepository = accountForTestRunRepository;
        }
        // GET 
        [HttpGet]
        [Route("~/api/projects/")]
        public ProjectsResponse GetGRSProjectNames()
        {
            return new ProjectsResponse { Projects = projectRepository.GetAllToList().Select(p => p.ProjectName).OrderBy(p => p) };
        }

        // GET 
        [HttpGet]
        [Route("~/api/projects/")]
        public ExecutionResponse GetExecutionSettings(string projectName)
        {
            var project = projectRepository.GetSingleOrDefault(p => p.ProjectName.ToLower() == projectName.ToLower());
            return new ExecutionResponse
            {
                Urls = project.HostsConfigurations.Select(p => p.ApplicationURL).OrderBy(p => p),
                TestCycles = project.TestCycles.Where(p=>p.isInnactive!=true).Select(p => p.CycleName).OrderBy(p=>p),
                Accounts = project.AccountForTestRuns.Select(p => new AccountModel { Login = p.UserLogin, Password = p.UserPassword }).OrderBy(p => p.Login)
            };
        }
    }
}
