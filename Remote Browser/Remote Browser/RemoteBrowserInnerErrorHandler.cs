using System;
using static RemoteBrowserClient;

namespace Remote_Browser
{
    public static class RemoteBrowserInnerErrorHandler
    {
        public static RemoteBrowserClient clientInstance;
        public static T CONNECTION_ENDED<T>(object objType)
        {
            clientInstance.Shutdown();
            if (objType.GetType() == typeof(DirectoryList))
            {
                return (T)Convert.ChangeType(new DirectoryList(new string[] { "CONNECTION CLOSED" }, new string[] { "CONNECTION CLOSED" }), typeof(T));
            }
            else if (objType.GetType() == typeof(string[]))
            {
                return (T)Convert.ChangeType(new string[] { "CONNECTION ENDED" }, typeof(T));
            }
            return (T)(null as object);
        }
        public static DirectoryList ACCESS_DENIED()
        {
            clientInstance.CurrentDirectory = clientInstance.oldDir;
            return new DirectoryList(new string[] { "ACCESS DENIED" }, new string[] { "ACCESS DENIED" });
        }
        public static DirectoryList Code_rejected()
        {
            return new DirectoryList(new string[] { "Code rejected" }, new string[] { "Code rejected" });
        }
    }
}
