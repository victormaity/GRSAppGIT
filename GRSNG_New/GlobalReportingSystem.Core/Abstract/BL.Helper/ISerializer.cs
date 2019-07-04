using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GlobalReportingSystem.Core.Abstract.BL.Helper
{
    public interface ISerializer<T> where T : class
    {
        T Deserialize(string inputXml);
        string Serialize();
    }
}
