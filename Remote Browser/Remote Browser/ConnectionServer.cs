using AndroidExtendedCommands.CSharp.Data.SimpleJSON;
using System.Net;
using System.Text.RegularExpressions;

namespace Remote_Browser
{
    public class ConnectionServer
    {
        public ConnectionServer(string serverName, string hostName, IPAddress lanIp, ushort port)
        {
            ServerName = serverName;
            PublicHost = hostName;
            LanIp = lanIp;
            Port = port;
        }
        public static implicit operator ConnectionServer(JSONNode json)
        {
            if (Regex.IsMatch(json["PublicHost"].Value, @"[\w\d]+\.[\w\d]+\.[\w\d]+"))
                return new ConnectionServer(json["ServerName"].Value, json["PublicHost"].Value, IPAddress.Parse(json["LanIp"].Value), ushort.Parse(json["Port"].Value));
            else
                return new ConnectionServer(json["ServerName"].Value, IPAddress.Parse(json["PublicHost"].Value), IPAddress.Parse(json["LanIp"].Value), ushort.Parse(json["Port"].Value));
        }
        public JSONNode GetJSON()
        {
            return JSON.Parse($"{{" +
                $"\"ServerName\" : \"{ServerName}\", " +
                $"\"PublicHost\" : \"{PublicHost}\", " +
                $"\"LanIp\" : \"{LanIp}\", " +
                $"\"Port\" : \"{Port}\"" +
                $"}}");
        }
        public ConnectionServer(string serverName, IPAddress publicIp, IPAddress lanIp, ushort port)
        {
            ServerName = serverName;
            PublicHost = publicIp.ToString();
            LanIp = lanIp;
            Port = port;
        }
        public RemoteBrowserClient CreateConnection(string initialDirectory = "C:\\")
        {
            if (Regex.IsMatch(PublicHost, @"[\w\d]+\.[\w\d]+\.[\w\d]+"))
                if (Dns.GetHostAddresses(PublicHost)[0] == AndroidExtendedCommands.CSharp.Info.AndroidInfo.ExternalIpAddress)
                    return new RemoteBrowserClient(LanIp.ToString(), Port, initialDirectory);
                else
                    return new RemoteBrowserClient(PublicHost, Port, initialDirectory, 0);
            else
                if (IPAddress.Parse(PublicHost) == AndroidExtendedCommands.CSharp.Info.AndroidInfo.ExternalIpAddress)
                return new RemoteBrowserClient(LanIp.ToString(), Port, initialDirectory);
            else
                return new RemoteBrowserClient(PublicHost, Port, initialDirectory);
        }
        public override string ToString()
        {
            return ServerName;

        }
        public string ServerName { get; }
        public string PublicHost { get; }
        public IPAddress LanIp { get; }
        public ushort Port { get; }
    }
}
