using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;

namespace GlobalReportingSystem.Web.API.Controllers
{
    public class TestSuitController: ApiController
    {
        private readonly IManageTestCycleProvider manageTestCycleProvider;

        private readonly IRepository<TestCycle> testCycleRepository;

        private readonly IRepository<TestSuit> testSuitRepository;

        public TestSuitController(IManageTestCycleProvider manageTestCycleProvider, IRepository<TestCycle> testCycleRepository,
            IRepository<TestSuit> testSuitRepository)
        {
            this.manageTestCycleProvider = manageTestCycleProvider;
            this.testCycleRepository = testCycleRepository;
            this.testSuitRepository = testSuitRepository;
        }


        // GET 
        [HttpGet]
        [Route("~/api/testSuits/")]
        public HttpResponseMessage GetTestSuitIdForGreenGo(string ip, string cycleName, string suitName, string projectName)
        {
            var cycle =
                testCycleRepository.GetSingleOrDefault(
                    p => p.Project.ProjectName == projectName && p.CycleName == cycleName);
            if (cycle == null)
            {
               return Request.CreateResponse(HttpStatusCode.BadRequest,"Test Suit is not found");
            }
            var suits=manageTestCycleProvider.GetTestSuitsByKeyAndIp(ip, cycle.ID, suitName);
            if (!suits.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Test Suit is not found");
            }
            var lastTestSuitId = suits.Select(p => p.Id).Max();
            var testSuit = testSuitRepository.GetSingleOrDefault(p => p.ID == lastTestSuitId);
            if (testSuit == null || testSuit.TSStart.ToShortDateString() != DateTime.Now.ToUniversalTime().ToShortDateString())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Test Suit is not found");
            }
            return Request.CreateResponse<int>(HttpStatusCode.OK,  testSuit.ID);
        }
    }
}