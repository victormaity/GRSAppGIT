using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using System.Web.UI;
namespace GRSSikuli
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SikuliRetrieveService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SikuliRetriever.svc or SikuliRetrieveService.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SikuliRetrieveService : ISikuliRetrieveService
    {
        private readonly IRepository<SikuliObject> _sikuliObjectRepository;

        public SikuliRetrieveService(IRepository<SikuliObject> sikuliObjectRepository)
        {
            _sikuliObjectRepository = sikuliObjectRepository;
        }

        public void GetSikuliObject(string pName, string oName)
        {
            var item =
                _sikuliObjectRepository.GetSingleOrDefault(p => p.Project.ProjectName == pName && p.name == oName);
            if (item != null)
            {
                string contentType;
                if (item.contentType == null)
                    contentType = ".png";
                else contentType = ("." + item.contentType).Replace("..",".");

                /*HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.AddHeader("Content-Disposition",
                    "attachment; filename=" + item.name + contentType);
                HttpContext.Current.Response.AddHeader("Content-Length", item.content.Length.ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";

                //HttpContext.Current.Response.End();
                HttpContext.Current.Response.BinaryWrite(item.content);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();*/
                //A webservice returning a stream with the document data.

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.BufferOutput = true;
                HttpContext.Current.Response.ContentType = "application/x-download";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "atachment; filename=" + item.name + contentType);
                HttpContext.Current.Response.CacheControl = "public";

                byte[] buffer = item.content;
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        HttpContext.Current.Response.Flush();
                    }
                }
                HttpContext.Current.Response.Close();
        }
    }
}
