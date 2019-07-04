using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace GRSSikuli
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISikuliRetrieveService" in both code and config file together.
    [ServiceContract]
    public interface ISikuliRetrieveService
    {
        [OperationContract]
        [WebGet(UriTemplate = "GetSikuliObject?pName={pName}&oName={oName}", ResponseFormat = WebMessageFormat.Json)]
        [Description("Get sikuli object")]
        void GetSikuliObject(string pName, string oName);
    }
}
