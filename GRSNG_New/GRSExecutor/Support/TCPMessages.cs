using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

public class SocketRequest
{

    public SocketRequest Deserialize(string inputXML)
    {
        using (TextReader reader = new StringReader(inputXML))
        {
            XmlSerializer xs = new XmlSerializer(typeof(SocketRequest));
            return (SocketRequest)xs.Deserialize(reader);
        }

    }

    public string Serialize()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SocketRequest));
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }

    public string CallBackMessenger { get; set; }
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string Owner { get; set; }
    public string projectName { get; set; }
    public string RequestType { get; set; }
    public string AcceptanceCriteria { get; set; }
    public string RequestParameters { get; set; }
    public ConfigurationInfoData EnvironmentConfiguration { get; set; }
    public string Attachment { get; set; }
    public string Include { get; set; }
    public string Exclude { get; set; }
    public string Priority { get; set; }
}

public class SocketResponse
{
    public bool isSuccess { get; set; }
    public string Message { get; set; }
    public ClientInfo clientInfo { get; set; }

    public SocketResponse Deserialize(string inputXML)
    {
        using (TextReader reader = new StringReader(inputXML))
        {
            XmlSerializer xs = new XmlSerializer(typeof(SocketResponse));
            return (SocketResponse)xs.Deserialize(reader);
        }
    }
    public string Serialize()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SocketResponse));
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}

public class ConfigurationInfoData
{
    public string URL { get; set; }
    public string Browser { get; set; }
    public bool BackDoor { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string ProfileName { get; set; }
    public string HostFileContent { get; set; }
    public string ConfFileName { get; set; }
    public string ConfFileContent { get; set; }
    public int? FailsToAlert { get; set; }
}

public class ClientInfo
{
    public string ip { get; set; }
    public string windows { get; set; }
    public string ff { get; set; }
    public string ch { get; set; }
    public string ie { get; set; }
    public string freespace { get; set; }
}