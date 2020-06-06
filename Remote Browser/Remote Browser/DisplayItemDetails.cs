using AndroidExtendedCommands.CSharp.Data.SimpleJSON;
using System;
using System.IO;

namespace Remote_Browser
{
    public class DisplayItemDetails
    {
        public DisplayItemDetails(RemoteBrowserClient requester, string fullPath)
        {
            Requester = requester;
            FullPath = fullPath;
        }
        public void RequestDetails()
        {
            Requester.SendCommand("FILE-DETAILS: ", FullPath);
            var pkg = Requester.ReceivePackage();
            var errHandl = typeof(RemoteBrowserInnerErrorHandler).GetMethod(pkg);
            if (errHandl != null)
            { errHandl.Invoke(null, new object[] { }); return; }
            else
            {
                JSONNode f = JSON.Parse(pkg);
                Name = f["Name"].Value;
                Size = long.Parse(f["Size"].Value);
                CreationTime = DateTime.Parse(f["CreationTime"].Value);
                Attributes = (FileAttributes)int.Parse(f["Attributes"].Value);
                DirectoryName = f["DirectoryName"].Value;
                Extension = f["Extension"].Value;
                LastWriteTime = DateTime.Parse(f["LastWriteTime"].Value);
                LastAccessTime = DateTime.Parse(f["LastAccessTime"].Value);
            }
        }
        public long Size { get; private set; }
        public string Name { get; private set; }
        public string FullPath { get; }
        public string Extension { get; private set; }
        public string DirectoryName { get; private set; }
        public DateTime CreationTime { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public DateTime LastAccessTime { get; private set; }
        public RemoteBrowserClient Requester { get; }
    }
}
