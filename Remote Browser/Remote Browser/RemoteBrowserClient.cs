using Android.App;
using AndroidExtendedCommands.CSharp.Web.Communication;
using Java.Lang;
using Remote_Browser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

public class RemoteBrowserClient : TCPClient
{
    public class DirectoryList
    {
        public DirectoryList(string[] files, string[] directories)
        {
            Files = files;
            Directories = directories;
        }

        public string[] Files { get; }
        public string[] Directories { get; }
    }
    public void Shutdown()
    {
        SendCommand("CLIENT-SHUTDOWN");
        Disconnect();
    }
    public RemoteBrowserClient(string hostName, ushort port, string initialDirectory, int addressListIndex = 0)
    {
        Ip = Dns.GetHostAddresses(hostName)[addressListIndex];
        Port = port;
        CurrentDirectory = initialDirectory;
        RemoteBrowserInnerErrorHandler.clientInstance = this;
    }
    public RemoteBrowserClient(string ip, ushort port, string initialDirectory = "C:\\")
    {
        Ip = IPAddress.Parse(ip);
        Port = port;
        CurrentDirectory = initialDirectory;
        RemoteBrowserInnerErrorHandler.clientInstance = this;
    }
    public Activity Activity { get; set; }
    public void SendCommand(string command, string args = null)
    {
        var s = command.ToUpper();
        s += args != null ? ": \"" + args + "\"" : "";
        SendPackage(new TcpPackage(s));
    }
    public DirectoryList ListDirectory()
    {
        if (CurrentDirectory == null)
            SendCommand("list-directory", oldDir);
        else
            SendCommand("list-directory", CurrentDirectory);
        string package;
        Thread.Sleep(100);
        try
        {
            package = ReceivePackage().ToString();
        }
        catch
        {
            return RemoteBrowserInnerErrorHandler.CONNECTION_ENDED();
        }
        if (typeof(RemoteBrowserInnerErrorHandler).GetMethod(package.Replace(" ", "_")) != null)
        {
            return typeof(RemoteBrowserInnerErrorHandler).GetMethod(package.Replace(" ", "_")).Invoke(null, null) as DirectoryList;
        }
        string filedata;
        var directoryData = "";
        if (package.Contains("Directories:\n"))
        {
            filedata = package.Substring(0, package.IndexOf("\nDirectories:\n"));
            directoryData = package.Substring(filedata.Length + 1);
        }
        else
        {
            filedata = package.Substring(0, package.IndexOf("\nDirectories:"));
        }
        List<string> mem = new List<string>();
        foreach (Match m in Regex.Matches(filedata, @"\t(.+);"))
            mem.Add(m.Groups[1].Value);
        string[] files = mem.ToArray();
        mem.Clear();
        foreach (Match m in Regex.Matches(directoryData, @"\t(.+);"))
            mem.Add(m.Groups[1].Value);
        return new DirectoryList(files, mem.ToArray());
    }
    public Stream RetrieveFile(string path)
    {
        SendCommand("retrieve-file", path);
        var package = ReceivePackage();
        return new MemoryStream(package.Data);
    }
    public event EventHandler<ClientNavigateEventArgs> Navigated;
    public event EventHandler<ClientNavigateEventArgs> DirectorySet;

    public void Navigate(string dirname)
    {
        CurrentDirectory = Path.Combine(currentDirectory.Replace("\\", "/"), dirname).Replace("/", "\\");
        Navigated?.Invoke(this, new ClientNavigateEventArgs(dirname));
    }
    public void NavigateBack()
    {
        var info = new DirectoryInfo(CurrentDirectory.Replace("\\", "/"));
        if (info.FullName.Length > 3)
        {
            CurrentDirectory = info.Parent.FullName.Replace("/", "\\");
            if (CurrentDirectory.Substring(0, 1) == "\\")
                CurrentDirectory = CurrentDirectory.Substring(1);
            if (CurrentDirectory.Length == 2)
                CurrentDirectory += "\\";
            Navigated?.Invoke(this, new ClientNavigateEventArgs(".."));
        }
    }
    public bool SetDirectory(string dir)
    {
        SendCommand("DirExists", dir);
        string data = ReceivePackage();
        bool flag = data == "1";
        Thread.Sleep(50);
        if (flag)
        {
            CurrentDirectory = dir;
            DirectorySet?.Invoke(this, new ClientNavigateEventArgs(dir));
        }
        return flag;
    }
    public class ClientNavigateEventArgs : EventArgs
    {
        public ClientNavigateEventArgs(string dirname)
        {
            DirName = dirname;
        }

        public string DirName { get; }
    }
    private string currentDirectory = "";

    public string oldDir = "";
    public string CurrentDirectory
    {
        get { return currentDirectory; }
        set { oldDir = currentDirectory; currentDirectory = value; }
    }

    public string GetFileSize(string file)
    {
        SendCommand("GetFileSize", file);
        var size = ReceivePackage();
        return size;
    }
}