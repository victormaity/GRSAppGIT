using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using Microsoft.Practices.ServiceLocation;

namespace GRSSikuli
{
   
    public class GetSikuliController : ApiController
    {
       
      

       

        public HttpResponseMessage GetSikuliObject(string pName, string oName)
        {
            var sikuliObjectRepository = ServiceLocator.Current.GetInstance<IRepository<SikuliObject>>();
            var item =
                sikuliObjectRepository.GetSingleOrDefault(p => p.Project.ProjectName == pName && p.name == oName);
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            if (item != null)
            {
                
                result.Content = new StreamContent(new MemoryStream(item.content));
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("image/png");
            }
            return result;
        }
    }
}