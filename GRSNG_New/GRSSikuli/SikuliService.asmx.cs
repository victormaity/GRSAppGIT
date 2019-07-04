using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Core.Models.Sikuli;
using GlobalReportingSystem.ServiceLocation;
using Microsoft.Practices.ServiceLocation;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;

namespace GRSSikuli
{
    /// <summary>
    /// Summary description for SikuliService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SikuliService : System.Web.Services.WebService
    {

        [WebMethod]
        public void UpdateSikuliItem(int id, int sessionId, string name, string description, string cathegory)
        {
            var sikuliObjects = ServiceLocator.Current.GetInstance<IRepository<SikuliObject>>();
            var item = sikuliObjects.GetSingleOrDefault(p => p.ID == id);
            var userId =
                ServiceLocator.Current.GetInstance<IRepository<UserSession>>()
                    .GetSingleOrDefault(p => p.ID == sessionId)
                    .User.ID;
            if (item != null)
            {
                item.name = name;
                item.description = description;
                item.modifiedBy = userId;
                item.modifiedOn = DateTime.Now;
                item.cathegory = string.IsNullOrEmpty(cathegory) ? null : cathegory;
            }
            sikuliObjects.SaveChanges();
        }

        [WebMethod]
        public SikuliItem GetSikuliItem(int id)
        {
            var item = ServiceLocator.Current.GetInstance<IRepository<SikuliObject>>().GetSingleOrDefault(p => p.ID == id);
            return new SikuliItem { Id = item.ID, Description = item.description, Name = item.name, ProjectName = item.Project.ProjectName };
        }

        [WebMethod]
        public void RemoveSikuliItem(int id)
        {
            var sikuliObject = ServiceLocator.Current.GetInstance<IRepository<SikuliObject>>();
            sikuliObject.Delete(
                sikuliObject.GetSingleOrDefault(p => p.ID == id));
            sikuliObject.SaveChanges();
        }

        [WebMethod]
        public void AddSikuliItem(byte[] content, int sessionId, string fileName)
        {
            var userSession = ServiceLocator.Current.GetInstance<IRepository<UserSession>>();
            var project = userSession.GetSingleOrDefault(p => p.ID == sessionId).Project;
            var userId = userSession.GetSingleOrDefault(p => p.ID == sessionId).User.ID;
            var contentType = fileName.Split('.').Last();
            if (project.SikuliObjects.FirstOrDefault(p => p.name == fileName.Split('.').First()) == null)
            {
                project.SikuliObjects.Add(
                    new SikuliObject
                    {
                        addedBy = userId,
                        content = content,
                        addedOn = DateTime.Now,
                        hash =
                            BitConverter.ToString(MD5.Create().ComputeHash(content))
                                .Replace("-", ""),
                        name = fileName.Split('.').First(),
                        parentProject = project.ID,
                        isSikuli = "true",
                        contentType = contentType,
                        modifiedBy = userId,
                        modifiedOn = DateTime.Now
                    });
                userSession.SaveChanges();
            }
        }

    }
}
