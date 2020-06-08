using AndroidExtendedCommands.CSharp.Data.SimpleJSON;
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
            Requester.SendCommand("FILE-DETAILS", FullPath);
            var pkg = Requester.ReceivePackage();
            var errHandl = typeof(RemoteBrowserInnerErrorHandler).GetMethod(pkg);
            if (errHandl != null)
            { errHandl.Invoke(null, new object[] { }); return; }
            else if (JSON.Parse(pkg)["Type"].Value == "File")
            {
                JSONNode f = JSON.Parse(pkg);
                Type = f["Type"].Value;
                Name = f["Name"].Value;
                Size = long.Parse(f["Size"].Value);
                CreationTime = f["CreationTime"].Value;
                Attributes = (FileAttributes)int.Parse(f["Attributes"].Value);
                ParentDirectory = f["DirectoryName"].Value.Replace(";", "\\");
                Extension = f["Extension"].Value;
                LastWriteTime = f["LastWriteTime"].Value;
                LastAccessTime = f["LastAccessTime"].Value;
            }
            else
            {
                JSONNode f = JSON.Parse(pkg);
                Type = f["Type"].Value;
                Name = f["Name"].Value;
                CreationTime = f["CreationTime"].Value;
                Attributes = (FileAttributes)int.Parse(f["Attributes"].Value);
                ParentDirectory = f["DirectoryName"].Value.Replace(";", "\\");
                Extension = f["Extension"].Value;
                LastWriteTime = f["LastWriteTime"].Value;
                LastAccessTime = f["LastAccessTime"].Value;
            }
        }
        public long Size { get; private set; }
        public string Name { get; private set; }
        public string FullPath { get; }
        public string Extension { get; private set; }
        public string ParentDirectory { get; private set; }
        public string Type { get; private set; }
        public string CreationTime { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public string LastWriteTime { get; private set; }
        public string LastAccessTime { get; private set; }
        public RemoteBrowserClient Requester { get; }
    }
}
