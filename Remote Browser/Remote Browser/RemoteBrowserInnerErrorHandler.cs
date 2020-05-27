using static RemoteBrowserClient;

namespace Remote_Browser
{
    public static class RemoteBrowserInnerErrorHandler
    {
        public static RemoteBrowserClient clientInstance;
        public static DirectoryList CONNECTION_ENDED()
        {
            clientInstance.Shutdown();
            return new DirectoryList(new string[] { "CONNECTION CLOSED" }, new string[] { "CONNECTION CLOSED" });
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
