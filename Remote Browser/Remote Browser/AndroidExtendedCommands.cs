using Android;
using Android.App;
using AndroidExtendedCommands.CSharp.DataTypeExtensions;
using AndroidExtendedCommands.CSharp.DataTypeExtensions.RegularExpressions;
using AndroidExtendedCommands.CSharp.Info;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AndroidExtendedCommands
{
    namespace Extensions
    {
        [ContentProperty(nameof(Source))]
        public class ImageResourceExtension : IMarkupExtension
        {
            public string Source { get; set; }

            public object ProvideValue(IServiceProvider serviceProvider)
            {
                if (Source == null)
                {
                    return null;
                }

                return ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            }
        }
    }
    public class Dialogs
    {
        public class DialogResult : EventArgs
        {
            public DialogResult()
            {

            }
            public DialogResult(string content, DialogButtons result)
            {
                Content = content;
                Result = result;
            }
            public static implicit operator DialogResult(string v)
            {
                switch (v)
                {
                    case "": return new DialogResult(v, DialogButtons.Negative);
                    default: return new DialogResult(v, DialogButtons.Positive);
                }
            }
            public static implicit operator DialogResult(DialogButtons b)
            {
                return new DialogResult("", b);
            }
            public static implicit operator string(DialogResult v)
            {
                return v.Content;
            }

            public string Content { get; }
            public DialogButtons Result { get; }

            public override string ToString()
            {
                return Content;
            }
        }
        public class MultiDialogResult : EventArgs
        {
            public MultiDialogResult()
            {
                CheckedItems = new bool[0];
                Items = new string[0];
            }
            public MultiDialogResult(string[] items, bool[] checkedItems)
            {
                Items = items;
                CheckedItems = checkedItems;
            }
            public string[] Items { get; }
            public bool[] CheckedItems { get; }
        }
        public enum DialogButtons
        {
            Positive,
            Negative,
            Neutral
        }
        public static Dialog CreateDialog(Activity activity, string title, string message, EventHandler<DialogResult> result = null, DialogButtons buttons = DialogButtons.Neutral, string neutralButton = null, string positiveButton = null, string negativeButton = null)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetTitle(title);
            builder.SetMessage(message);
            if (buttons.HasFlag(DialogButtons.Negative))
                builder.SetNegativeButton(negativeButton, (sender, e) =>
                {
                    result?.Invoke(sender, DialogButtons.Negative);
                });
            if (buttons.HasFlag(DialogButtons.Positive))
                builder.SetPositiveButton(positiveButton, (sender, e) =>
                {
                    result?.Invoke(sender, DialogButtons.Positive);
                });
            if (buttons.HasFlag(DialogButtons.Neutral))
                builder.SetNeutralButton(neutralButton, (sender, e) =>
                {
                    result?.Invoke(sender, DialogButtons.Neutral);
                });
            return builder.Create();
        }
        public static void ShowDialog(Activity activity, string title, string message, EventHandler<DialogResult> result = null, DialogButtons buttons = DialogButtons.Neutral, string neutralButton = null, string positiveButton = null, string negativeButton = null)
        {
            var d = CreateDialog(activity, title, message, result, buttons, neutralButton, positiveButton, negativeButton);
            d.Show();
        }
        public static class ItemSelection
        {
            public static void ShowSingleSelect(Activity activity, string[] items, string title, EventHandler<DialogResult> onResult)
            {
                var alert = new AlertDialog.Builder(activity);
                int selected = -1;
                alert.SetTitle(title);
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    onResult(sender, items[selected]);
                });
                alert.SetNegativeButton("Cancel", (sender, e) =>
                {
                    onResult(sender, "");
                });
                alert.SetSingleChoiceItems(items, -1, (sender, e) =>
                {
                    selected = e.Which;
                });
                alert.Show();
            }
            public static void ShowMultiSelect(Activity appContext, string[] items, string title, EventHandler<MultiDialogResult> onResult)
            {
                var alert = new AlertDialog.Builder(appContext);
                alert.SetTitle(title);
                bool[] selected = new bool[items.Length];
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    onResult(sender, new MultiDialogResult(items, selected));
                });
                alert.SetNegativeButton("Cancel", (sender, e) =>
                {
                    onResult(sender, new MultiDialogResult(items, new bool[0]));
                });
                alert.SetMultiChoiceItems(items, selected, (sender, e) =>
                {
                    selected[e.Which] = e.IsChecked;
                });
                alert.Show();
            }
            public static AlertDialog.Builder CreateSingleSelect(Activity appContext, string[] items, string title, string positiveButton = "OK", string negativeButton = "Cancel", EventHandler<Android.Content.DialogClickEventArgs> positiveButtonClick = null, EventHandler<Android.Content.DialogClickEventArgs> negativeButtonClick = null, EventHandler<Android.Content.DialogClickEventArgs> itemSelected = null)
            {
                var alert = new AlertDialog.Builder(appContext);
                alert.SetTitle(title);
                if (positiveButtonClick != null)
                    alert.SetPositiveButton(positiveButton, positiveButtonClick);
                else
                    alert.SetPositiveButton(positiveButton, (sender, args) => { });
                if (positiveButtonClick != null)
                    alert.SetNegativeButton(negativeButton, negativeButtonClick);
                else
                    alert.SetNegativeButton(negativeButton, (sender, args) => { });
                if (itemSelected != null)
                    alert.SetSingleChoiceItems(items, -1, itemSelected);
                else
                    alert.SetSingleChoiceItems(items, -1, (sender, args) => { });
                return alert;
            }
            public static AlertDialog.Builder CreateMultiSelect(Activity appContext, string[] items, bool[] checkedItems, string title, string positiveButton = "OK", string negativeButton = "Cancel", EventHandler<Android.Content.DialogClickEventArgs> positiveButtonClick = null, EventHandler<Android.Content.DialogClickEventArgs> negativeButtonClick = null, EventHandler<Android.Content.DialogMultiChoiceClickEventArgs> itemSelected = null)
            {
                var alert = new AlertDialog.Builder(appContext);
                alert.SetTitle(title);
                if (positiveButtonClick != null)
                    alert.SetPositiveButton(positiveButton, positiveButtonClick);
                else
                    alert.SetPositiveButton(positiveButton, (sender, args) => { });
                if (positiveButtonClick != null)
                    alert.SetNegativeButton(negativeButton, negativeButtonClick);
                else
                    alert.SetNegativeButton(negativeButton, (sender, args) => { });
                if (itemSelected != null)
                    alert.SetMultiChoiceItems(items, checkedItems, itemSelected);
                else
                    alert.SetMultiChoiceItems(items, checkedItems, (sender, args) => { });
                return alert;
            }
        }
    }
    namespace Permissions
    {
        public class PermissionHandler
        {
            public static Activity Activity { get; set; }
            public PermissionHandler(params string[] permissions)
            {
                Permissions = permissions;
                CheckPermissions();
            }
            public PermissionHandler(Activity activity, params string[] permissions)
            {
                Activity = activity;
                Permissions = permissions;
                CheckPermissions();
            }
            private void CheckPermissions()
            {
                if (Permissions != null)
                    foreach (var p in Permissions)
                        if (Activity.CheckSelfPermission(p) == Android.Content.PM.Permission.Granted && !grantedPermissions.Contains(p))
                            grantedPermissions.Add(p);
            }
            public void AddPermission(string permission)
            {
                var tmp = Permissions.ToList();
                if (tmp != null)
                {
                    tmp.Add(permission);
                    Permissions = tmp.ToArray();
                    CheckPermissions();
                }
                CheckPermissions();
            }
            public void RemovePermission(string permission)
            {
                var tmp = Permissions.ToList();
                if (tmp != null)
                {
                    tmp.Remove(permission);
                    Permissions = tmp.ToArray();
                    CheckPermissions();
                }
            }
            public static PermissionObject[] PermissionList
            {
                get
                {
                    var a = new List<PermissionObject>();
                    foreach (var f in typeof(Manifest.Permission).GetFields())
                        if (f.IsStatic)
                            a.Add(new PermissionObject(f.Name, f.GetValue(null).ToString()));
                    return a.ToArray();
                }
            }
            public class PermissionObject
            {
                public PermissionObject(string name, string value)
                {
                    Name = name;
                    Value = value;
                }
                public override string ToString()
                {
                    return Name;
                }
                public string Name { get; }
                public string Value { get; }
            }
            public bool HasAllPermissions()
            {
                if (Permissions != null)
                    foreach (var p in Permissions)
                        if (!HasPermission(p))
                            return false;
                return true;
            }
            public void RequestAllPermissions()
            {
                List<string> request = new List<string>();
                if (Permissions != null)
                    foreach (var p in Permissions)
                        if (!HasPermission(p))
                            request.Add(p);
                if (request.Count > 0)
                {
                    var id = new Random().Next(int.MaxValue / 2);
                    Activity.RequestPermissions(request.ToArray(), id);
                }
            }
            private readonly List<string> grantedPermissions = new List<string>();
            private Dictionary<int, string> permissionRequestCodes = new Dictionary<int, string>();
            public void RequestPermission(string permission)
            {
                if (!HasPermission(permission))
                {
                    var id = new Random().Next(int.MaxValue / 2);
                    permissionRequestCodes.Add(id, permission);
                    if (Activity.ShouldShowRequestPermissionRationale(permission))
                    {
                        //set alert for executing the task
                        AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
                        alert.SetTitle("Permissions Needed");
                        alert.SetMessage("The application need special permissions to continue");
                        alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                        {
                            Activity.RequestPermissions(new string[] { permission }, id);
                        });

                        alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                        {

                        });

                        Dialog dialog = alert.Create();
                        Device.BeginInvokeOnMainThread(() => dialog.Show());


                        return;
                    }

                    Activity.RequestPermissions(new string[] { permission }, id);
                }
            }
            public void RequestPermissionResult(int requestCode, string[] permissions, [Android.Runtime.GeneratedEnum] Android.Content.PM.Permission[] grantResults)
            {
                for (int i = 0; i < permissions.Length; i++)
                    if (grantResults[i] == Android.Content.PM.Permission.Granted)
                        grantedPermissions.Add(permissions[i]);
                if (permissionRequestCodes.ContainsKey(requestCode))
                    permissionRequestCodes.Remove(requestCode);
                //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
            public bool HasPermission(string permission)
            {
                return grantedPermissions.Contains(permission);
            }

            public string[] Permissions { get; set; }
        }
    }
    namespace CSharp
    {
        namespace Web
        {
            public static class Pastebin
            {
                public class Paste
                {
                    public Paste() { }
                    public Paste(string content, string title, string syntax)
                    {
                        Content = content;
                        Title = title;
                        Syntax = syntax;
                    }
                    public string Content { get; set; }
                    public string Title { get; set; }
                    public string Syntax { get; private set; }
                }
                private static string GetLanguage(Syntax s)
                {
                    switch (s)
                    {
                        case Syntax.CSharp:
                            return s.ToString().ToLower();
                        case Syntax.C:
                            return s.ToString().ToLower();
                        case Syntax.CPlusPlus:
                            return "cpp".ToLower();
                        case Syntax.Lua:
                            return s.ToString().ToLower();
                        case Syntax.MySQL:
                            return s.ToString().ToLower();
                        case Syntax.HTML:
                            return "html4strict";
                        case Syntax.jQuery:
                            return s.ToString().ToLower();
                        case Syntax.HTML5:
                            return s.ToString().ToLower();
                        case Syntax.Java:
                            return s.ToString().ToLower();
                        case Syntax.JSON:
                            return s.ToString().ToLower();
                        case Syntax.Java5:
                            return s.ToString().ToLower();
                        case Syntax.Raw:
                            return "";
                        default:
                            return s.ToString().ToLower();
                    }
                }
                private static string GetExpirationTime(ExpirationTime time)
                {
                    switch (time)
                    {
                        case ExpirationTime.Never:
                            return "N";
                        case ExpirationTime.OneHour:
                            return "1H";
                        case ExpirationTime.TenMinutes:
                            return "10M";
                        case ExpirationTime.OneDay:
                            return "1D";
                        case ExpirationTime.OneWeek:
                            return "1W";
                        case ExpirationTime.TwoWeeks:
                            return "2W";
                        case ExpirationTime.OneMonth:
                            return "1M";
                        case ExpirationTime.SixMonts:
                            return "6M";
                        case ExpirationTime.OneYear:
                            return "1Y";
                        default:
                            return time.ToString();
                    }
                }
                public static bool UploadNew(string Username, string Password, string DevKey, string Text, string PasteName, Syntax Syntax, ExpirationTime ExpirationTime, Visibility Visibility)
                {
                    //text, name, syntax, visibility, expiration
                    string LoginURL = "http://pastebin.com/api/api_login.php";
                    string PostURL = "http://pastebin.com/api/api_post.php";
                    var IBody = Text;
                    var IType = Convert.ToInt32(Visibility).ToString();
                    var ITime = GetExpirationTime(ExpirationTime);
                    var IFormat = GetLanguage(Syntax);
                    NameValueCollection IQuery = new NameValueCollection
                {
                    { "api_dev_key", DevKey },
                    { "api_user_name", Username },
                    { "api_user_password", Password }
                };

                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        byte[] respBytes = wc.UploadValues(LoginURL, IQuery);
                        string resp = Encoding.UTF8.GetString(respBytes);

                        if (resp.Contains("Bad API request"))
                        {
                            throw new System.Net.WebException("Bad Request", System.Net.WebExceptionStatus.SendFailure);
                        }
                        else
                        {
                            var UserKey = resp;
                            if (string.IsNullOrEmpty(IBody.Trim())) { return false; }
                            if (string.IsNullOrEmpty(PasteName.Trim())) { return false; }
                            IQuery = new NameValueCollection
                        {
                            { "api_dev_key", DevKey },
                            { "api_option", "paste" },
                            { "api_paste_code", IBody },
                            { "api_paste_private", IType },
                            { "api_paste_name", (string)PasteName },
                            { "api_paste_expire_date", ITime },
                            { "api_paste_format", IFormat },
                            { "api_user_key", UserKey }
                        };

                            using (System.Net.WebClient IClient = new System.Net.WebClient())
                            {
                                string IResponse = Encoding.UTF8.GetString(IClient.UploadValues(PostURL, IQuery));

                                if (!Uri.TryCreate(IResponse, UriKind.Absolute, out Uri isValid))
                                {
                                    throw new System.Net.WebException("Paste Error", System.Net.WebExceptionStatus.SendFailure);
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                public static bool UploadNew(string Username, string Password, string DevKey, string Text, string PasteName, string Syntax, ExpirationTime ExpirationTime, Visibility Visibility)
                {
                    //text, name, syntax, visibility, expiration
                    string LoginURL = "http://pastebin.com/api/api_login.php";
                    string PostURL = "http://pastebin.com/api/api_post.php";
                    var IBody = Text;
                    var IType = Convert.ToInt32(Visibility).ToString();
                    var ITime = GetExpirationTime(ExpirationTime);
                    var IFormat = Syntax;
                    NameValueCollection nameValueCollection = new NameValueCollection
                {
                    { "api_dev_key", DevKey },
                    { "api_user_name", Username },
                    { "api_user_password", Password }
                };
                    NameValueCollection IQuery = nameValueCollection;

                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        byte[] respBytes = wc.UploadValues(LoginURL, IQuery);
                        string resp = Encoding.UTF8.GetString(respBytes);

                        if (resp.Contains("Bad API request"))
                        {
                            throw new System.Net.WebException("Bad Request", System.Net.WebExceptionStatus.SendFailure);
                        }
                        else
                        {
                            var UserKey = resp;
                            if (string.IsNullOrEmpty(IBody.Trim())) { return false; }
                            if (string.IsNullOrEmpty(PasteName.Trim())) { return false; }
                            IQuery = new NameValueCollection
                        {
                            { "api_dev_key", DevKey },
                            { "api_option", "paste" },
                            { "api_paste_code", IBody },
                            { "api_paste_private", IType },
                            { "api_paste_name", (string)PasteName },
                            { "api_paste_expire_date", ITime },
                            { "api_paste_format", IFormat },
                            { "api_user_key", UserKey }
                        };

                            using (System.Net.WebClient IClient = new System.Net.WebClient())
                            {
                                string IResponse = Encoding.UTF8.GetString(IClient.UploadValues(PostURL, IQuery));

                                if (Uri.TryCreate(IResponse, UriKind.Absolute, out Uri isValid))
                                {
                                    return true;
                                }
                                else
                                {
                                    throw new System.Net.WebException("Paste Error", System.Net.WebExceptionStatus.SendFailure);
                                }
                            }
                        }
                    }
                }
                public static string GetRawData(string dataID)
                {
                    return new System.Net.WebClient().DownloadString("https://pastebin.com/raw/" + dataID);
                }
                public enum Syntax
                {
                    CSharp, C, CPlusPlus, Lua, MySQL, HTML, HTML5,
                    Java,
                    JSON,
                    Java5,
                    jQuery,
                    Raw
                }
                public enum ExpirationTime
                {
                    Never,
                    OneHour,
                    TenMinutes,
                    OneDay,
                    OneWeek,
                    TwoWeeks,
                    OneMonth,
                    SixMonts,
                    OneYear
                }
                public enum Visibility
                {
                    Public = 0,
                    Unlisted = 1,
                    Private = 2
                }
            }
            namespace Communication
            {
                public class TcpPackage
                {
                    public static TcpPackage FromRawData(params byte[] rawData)
                    {
                        var data = new byte[rawData.Length - 4];
                        for (int i = 0; i < rawData.Length - 4; i++)
                            data[i] = rawData[i + 4];
                        return new TcpPackage(data);
                    }
                    public TcpPackage(string data)
                    {
                        SetData(Encoding.ASCII.GetBytes(data));
                    }
                    public override string ToString()
                    {
                        return Encoding.ASCII.GetString(Data);
                    }
                    public TcpPackage(params byte[] data)
                    {
                        SetData(data);
                    }
                    void SetData(byte[] data)
                    {
                        Data = data;
                    }
                    public byte[] RawData
                    {
                        get
                        {
                            List<byte> bytes = new List<byte>();
                            var s = Size.ToString("X8");
                            bytes.AddRange(Encoding.ASCII.GetBytes(s));
                            bytes.AddRange(Data);
                            return bytes.ToArray();
                        }
                    }
                    public static implicit operator TcpPackage(string v)
                    {
                        return new TcpPackage(v);
                    }
                    public static implicit operator TcpPackage(byte[] v)
                    {
                        return new TcpPackage(v);
                    }
                    public static implicit operator string(TcpPackage v)
                    {
                        return v.ToString();
                    }
                    public static implicit operator byte[](TcpPackage v)
                    {
                        return v.Data;
                    }
                    public static implicit operator int(TcpPackage v)
                    {
                        return v.Size;
                    }
                    public int Size { get => Data.Length; }
                    public byte[] Data { get; private set; }
                }
                public class TCPClient
                {
                    public TCPClient(Socket clientSocket, IPAddress ip, ushort port)
                    {
                        ClientSocket = clientSocket;
                        Port = port;
                        Ip = ip;
                    }
                    public TCPClient(string hostName, ushort port = 0, int addressListIndex = 0)
                    {
                        Ip = Dns.GetHostAddresses(hostName)[addressListIndex];
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPClient(Socket clientSocket)
                    {
                        ClientSocket = clientSocket;
                        try
                        {
                            Port = (ushort)((IPEndPoint)clientSocket.RemoteEndPoint).Port;
                            Ip = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
                        }
                        catch { }
                    }
                    public TCPClient(string ip, ushort port = 0)
                    {
                        if (!string.IsNullOrEmpty(ip) && Regex.IsMatch(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                            Ip = IPAddress.Parse(ip);
                        else
                            Ip = IPAddress.Parse("127.0.0.1");
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPClient(IPAddress ip, ushort port = 0)
                    {
                        Ip = ip;
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPClient(ushort port = 0)
                    {
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Ip = IPAddress.Parse("127.0.0.1");
                        Setup();
                    }
                    public TCPClient(IPAddress ip)
                    {
                        Ip = ip;
                        Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPClient()
                    {
                        Ip = IPAddress.Parse("127.0.0.1");
                        Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    private void Setup()
                    {
                        ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    public void Connect(IPAddress ip)
                    {
                        if (!Connected)
                            Connect(ip, Port);
                    }
                    public void Connect(string ip)
                    {
                        if (!Connected)
                            try { Connect(IPAddress.Parse(ip)); } catch (Exception ex) { throw ex; }
                    }
                    public void Connect(IPAddress ip, ushort port)
                    {
                        if (!Connected)
                        {
                            try
                            {
                                ClientSocket.Connect(ip, port);
                            }
                            catch (Exception ex) { throw ex; }
                        }
                    }
                    public bool Connected { get => ClientSocket.Connected; }
                    public void ConnectAsync()
                    {
                        if (!Connected)
                            ClientSocket.BeginConnect(Ip, Port, (IAsyncResult result) =>
                            {
                                ClientSocket.EndConnect(result);
                            }, null);
                    }
                    public void Connect()
                    {
                        if (!Connected)
                            Connect(Ip, Port);
                    }
                    public void Disconnect()
                    {
                        if (Connected)
                        {
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                        }
                    }
                    public int ReceiveTimeout { get => ClientSocket.ReceiveTimeout; set => ClientSocket.ReceiveTimeout = value; }
                    public int SendTimeout { get => ClientSocket.SendTimeout; set => ClientSocket.SendTimeout = value; }
                    public AddressFamily AddressFamily { get => ClientSocket.AddressFamily; }
                    public int Avaliable { get => ClientSocket.Available; }
                    public bool EnableBroadcast { get => ClientSocket.EnableBroadcast; set => ClientSocket.EnableBroadcast = value; }

                    public Stream ReceiveFile()
                    {
                        var package = ReceivePackage();
                        var buffer = package.Data;
                        return new MemoryStream(buffer);
                    }
                    public byte[] Receive()
                    {
                        return Receive(2048);
                    }
                    public byte[] Receive(int bufferSize)
                    {
                        var buffer = new byte[bufferSize];
                        int received = ClientSocket.Receive(buffer, SocketFlags.None);
                        if (received == 0) return null;
                        var data = new byte[received];
                        Array.Copy(buffer, data, received);
                        return data;
                    }
                    public void SendPackage(TcpPackage package)
                    {
                        ClientSocket.Send(package.RawData, package.Size + 8, SocketFlags.None);
                    }
                    public void SendPackage(TcpPackage package, SocketFlags flags)
                    {
                        ClientSocket.Send(package.RawData, package.Size + 8, flags);
                    }
                    public void SendPackage(TcpPackage package, SocketFlags flags, out SocketError errorCode)
                    {
                        ClientSocket.Send(package.RawData, 0, package.Size + 8, flags, out errorCode);
                    }
                    public byte[] ReceiveExact(int size)
                    {
                        int mustReceive = size;
                        byte[] buffer;
                        List<byte> data = new List<byte>();
                        while (mustReceive != 0)
                        {
                            buffer = new byte[mustReceive];
                            var rec = ClientSocket.Receive(buffer, mustReceive, SocketFlags.None);
                            mustReceive -= rec;
                            if (rec != 0)
                            {
                                byte[] copy = new byte[rec];
                                Array.Copy(buffer, copy, rec);
                                data.AddRange(copy);
                            }
                        }
                        return data.ToArray();
                    }
                    public TcpPackage ReceivePackage()
                    {
                        byte[] buffer;
                        int mustReceive = 8;
                        List<byte> data = new List<byte>();
                        while (mustReceive != 0)
                        {
                            buffer = new byte[mustReceive];
                            var rec = ClientSocket.Receive(buffer, mustReceive, SocketFlags.None);
                            mustReceive -= rec;
                            if (rec != 0)
                            {
                                byte[] copy = new byte[rec];
                                Array.Copy(buffer, copy, rec);
                                data.AddRange(copy);
                            }
                        }
                        string sizeData = Encoding.ASCII.GetString(data.ToArray());
                        var size = Convert.ToInt32("0x" + sizeData, 16);
                        data.Clear();
                        mustReceive = size;
                        while (mustReceive != 0)
                        {
                            buffer = new byte[mustReceive];
                            var rec = ClientSocket.Receive(buffer, mustReceive, SocketFlags.None);
                            mustReceive -= rec;
                            if (rec != 0)
                            {
                                byte[] copy = new byte[rec];
                                Array.Copy(buffer, copy, rec);
                                data.AddRange(copy);
                            }
                        }
                        return new TcpPackage(data.ToArray());
                    }
                    public string ReceiveString()
                    {
                        return ReceiveString(2048);
                    }
                    public string ReceiveString(int bufferSize)
                    {
                        string text = Encoding.ASCII.GetString(Receive(bufferSize));
                        return text;
                    }
                    public int Send(byte[] buffer)
                    {
                        return ClientSocket.Send(buffer);
                    }
                    public int Send(byte[] buffer, SocketFlags flags)
                    {
                        return ClientSocket.Send(buffer, flags);
                    }
                    public int Send(byte[] buffer, int size, SocketFlags flags)
                    {
                        return ClientSocket.Send(buffer, size, flags);
                    }
                    public int Send(byte[] buffer, int offset, int size, SocketFlags flags)
                    {
                        return ClientSocket.Send(buffer, offset, size, flags);
                    }
                    public int Send(byte[] buffer, int offset, int size, SocketFlags flags, out SocketError errorCode)
                    {
                        return ClientSocket.Send(buffer, offset, size, flags, out errorCode);
                    }
                    public int SendString(string text)
                    {
                        return Send(Encoding.ASCII.GetBytes(text));
                    }
                    public void SendFile(string file, SocketFlags flags, out SocketError errorCode)
                    {
                        var reader = new BinaryReader(File.OpenRead(file));
                        var buffer = reader.ReadBytes((int)reader.BaseStream.Length);
                        reader.Close();
                        SendPackage(new TcpPackage(buffer), flags, out errorCode);
                    }
                    public void SendFile(string file, SocketFlags flags)
                    {
                        SendFile(file, flags, out SocketError _);
                    }
                    public void SendFile(string file)
                    {
                        SendFile(file, SocketFlags.None, out SocketError _);
                    }
                    public Socket ClientSocket { get; private set; }
                    public IPAddress Ip { get; set; }

                    private ushort _port;

                    public ushort Port
                    {
                        get { return _port; }
                        set
                        {
                            if (value != 0)
                                _port = value;
                            else
                                _port = _port == 0 ? ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString()) : _port;
                        }
                    }

                }
                public class TCPServer
                {
                    public TCPServer(string hostName, ushort port = 0, int addressListIndex = 0)
                    {
                        Ip = Dns.GetHostAddresses(hostName)[addressListIndex];
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPServer(string ip, ushort port = 0)
                    {
                        if (!string.IsNullOrEmpty(ip) && Regex.IsMatch(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                            Ip = IPAddress.Parse(ip);
                        else
                            Ip = IPAddress.Parse("127.0.0.1");
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPServer(IPAddress ip, ushort port = 0)
                    {
                        Ip = ip;
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPServer(ushort port = 0)
                    {
                        if (port != 0)
                            Port = port;
                        else
                            Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Ip = IPAddress.Parse("127.0.0.1");
                        Setup();
                    }
                    public TCPServer(IPAddress ip)
                    {
                        Ip = ip;
                        Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public TCPServer()
                    {
                        Ip = IPAddress.Parse("127.0.0.1");
                        Port = ushort.Parse(new Random().Next(888, int.Parse(ushort.MaxValue.ToString())).ToString());
                        Setup();
                    }
                    public bool Running { get; private set; }
                    readonly List<TCPClient> clients = new List<TCPClient>();
                    public TCPClient[] ConnectedClients { get => clients.ToArray(); }
                    public Socket ServerSocket { get; private set; }
                    public bool BeginReceiveOnConnection { get; set; } = true;
                    public bool AutoRelistenForMessages { get; set; } = true;
                    public int PacketBufferSize { get => buffer.Length; set => buffer = new byte[value]; }
                    void Setup()
                    {
                        clients.Clear();
                        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                    }
                    private byte[] buffer = new byte[2048];
                    public void Shutdown()
                    {
                        foreach (var client in ConnectedClients)
                            DisconnectClient(client, "Server shutdown.");
                        ServerSocket.Close();
                        Running = false;
                    }
                    void RemoveClient(Socket client)
                    {
                        foreach (var c in clients)
                            if (c.ClientSocket == client)
                            { clients.Remove(c); break; }
                    }
                    TCPClient AddClient(Socket client)
                    {
                        var c = new TCPClient(client);
                        clients.Add(c);
                        return c;
                    }
                    public enum DataReceiveType
                    {
                        Normal,
                        TcpPackage
                    }
                    private void OnClientDataReceived(IAsyncResult data)
                    {
                        Socket current = (Socket)data.AsyncState;
                        int received;

                        try
                        {
                            received = current.EndReceive(data);
                        }
                        catch
                        {
                            // Don't shutdown because the socket may be disposed and its disconnected anyway.
                            current.Close();
                            RemoveClient(current);
                            ClientDisconnected?.Invoke(this, new ClientConnectionArgs(new TCPClient(current), "Client forcefully disconnected"));
                            return;
                        }
                        byte[] recBuf = new byte[received];
                        Array.Copy(buffer, recBuf, received);
                        ClientDataReceived?.Invoke(this, new ClientDataArgs(new TCPClient(current), recBuf));
                        if (AutoRelistenForMessages)
                            try { current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnClientDataReceived, current); } catch { }
                    }
                    public void SendPackageToClient(TCPClient client, TcpPackage package)
                    {
                        client.SendPackage(package);
                    }
                    public byte[] ReceiveExactFromClient(TCPClient client, int size)
                    {
                        return client.ReceiveExact(size);
                    }
                    public TcpPackage ReceivePackageFromClient(TCPClient client)
                    {
                        return client.ReceivePackage();
                    }
                    public int[] Broadcast(byte[] buffer)
                    {
                        List<int> returns = new List<int>();
                        foreach (var c in clients)
                            returns.Add(SendToClient(c, buffer, SocketFlags.Broadcast));
                        return returns.ToArray();
                    }
                    public int[] Broadcast(byte[] buffer, int size)
                    {
                        List<int> returns = new List<int>();
                        foreach (var c in clients)
                            returns.Add(SendToClient(c, buffer, size, SocketFlags.Broadcast));
                        return returns.ToArray();
                    }
                    public int[] Broadcast(byte[] buffer, int offset, int size)
                    {
                        List<int> returns = new List<int>();
                        foreach (var c in clients)
                            returns.Add(SendToClient(c, buffer, offset, size, SocketFlags.Broadcast));
                        return returns.ToArray();
                    }
                    public int[] Broadcast(byte[] buffer, int offset, int size, out SocketError[] errorCodes)
                    {
                        List<int> returns = new List<int>();
                        List<SocketError> errors = new List<SocketError>();
                        foreach (var c in clients)
                        {
                            returns.Add(SendToClient(c, buffer, offset, size, SocketFlags.Broadcast, out SocketError error));
                            errors.Add(error);
                        }
                        errorCodes = errors.ToArray();
                        return returns.ToArray();
                    }
                    public int[] BroadcastString(string text)
                    {
                        List<int> returns = new List<int>();
                        foreach (var c in clients)
                            returns.Add(SendStringToClient(c, text));
                        return returns.ToArray();
                    }
                    public void BroadcastFile(string file)
                    {
                        foreach (var c in clients)
                            SendFileToClient(c, file);
                    }
                    public void BroadcastFile(string file, out SocketError[] errorCode)
                    {
                        List<SocketError> errorCodes = new List<SocketError>();
                        foreach (var c in clients)
                        { SendFileToClient(c, file, SocketFlags.Broadcast, out SocketError error); errorCodes.Add(error); }
                        errorCode = errorCodes.ToArray();
                    }
                    public void BroadcastFile(string file, SocketFlags flags, out SocketError[] errorCode)
                    {
                        List<SocketError> errorCodes = new List<SocketError>();
                        foreach (var c in clients)
                        { SendFileToClient(c, file, flags, out SocketError error); errorCodes.Add(error); }
                        errorCode = errorCodes.ToArray();
                    }
                    public int SendToClient(TCPClient client, byte[] buffer)
                    {
                        return client.Send(buffer);
                    }
                    public int SendToClient(TCPClient client, byte[] buffer, SocketFlags flags)
                    {
                        return client.Send(buffer, flags);
                    }
                    public int SendToClient(TCPClient client, byte[] buffer, int size, SocketFlags flags)
                    {
                        return client.Send(buffer, size, flags);
                    }
                    public int SendToClient(TCPClient client, byte[] buffer, int offset, int size, SocketFlags flags)
                    {
                        return client.Send(buffer, offset, size, flags);
                    }
                    public int SendToClient(TCPClient client, byte[] buffer, int offset, int size, SocketFlags flags, out SocketError errorCode)
                    {
                        return client.Send(buffer, offset, size, flags, out errorCode);
                    }
                    public int SendStringToClient(TCPClient client, string text)
                    {
                        return SendToClient(client, Encoding.ASCII.GetBytes(text));
                    }
                    public void SendFileToClient(TCPClient client, string fileName)
                    {
                        client.SendFile(fileName);
                    }
                    public void SendFileToClient(TCPClient client, string fileName, SocketFlags flags)
                    {
                        client.SendFile(fileName, flags, out SocketError _);
                    }
                    public void SendFileToClient(TCPClient client, string fileName, SocketFlags flags, out SocketError errorCode)
                    {
                        client.SendFile(fileName, flags, out errorCode);
                    }
                    private void RefuseConnection(ClientConnectionArgs e)
                    {
                        DisconnectClient(e.Client, "Connection refused!");
                    }
                    private void OnClientConnection(IAsyncResult request)
                    {
                        if (ClientTryConnect != null)
                        {
                            Socket socket = null;
                            try { socket = ServerSocket.EndAccept(request); }
                            catch (Exception ex) { OnClientConnectionFailed(socket, ex.Message); return; }
                            var client = new TCPClient(socket);
                            var args = new ClientConnectionArgs(client, "Client connecting...");
                            if (ClientTryConnect.Invoke(this, args))
                            {
                                clients.Add(client);
                                ClientConnected?.Invoke(this, new ClientConnectionArgs(client, "Client #" + (clients.IndexOf(client) + 1) + " connected"));
                                if (BeginReceiveOnConnection)
                                    try { socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnClientDataReceived, socket); } catch { RemoveClient(socket); ClientDisconnected?.Invoke(this, new ClientConnectionArgs(new TCPClient(socket), "Client forcefully disconnected")); }
                                ServerSocket.BeginAccept(OnClientConnection, null);
                            }
                            else
                                RefuseConnection(args);
                        }
                        else
                        {
                            Socket socket = null;
                            try { socket = ServerSocket.EndAccept(request); }
                            catch (Exception ex) { OnClientConnectionFailed(socket, ex.Message); return; }
                            var client = AddClient(socket);
                            if (BeginReceiveOnConnection)
                                try { socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnClientDataReceived, socket); } catch { RemoveClient(socket); ClientDisconnected?.Invoke(this, new ClientConnectionArgs(new TCPClient(socket), "Client forcefully disconnected")); }
                            ClientConnected?.Invoke(this, new ClientConnectionArgs(client, "Client #" + (clients.IndexOf(client) + 1) + " connected"));
                            ServerSocket.BeginAccept(OnClientConnection, null);
                        }
                    }
#pragma warning disable CS0067
                    public event EventHandler<ClientConnectionArgs> ClientConnectionRefused;
#pragma warning restore CS0067
                    public delegate bool ClientConnectionHandler(object sender, ClientConnectionArgs e);
                    public event ClientConnectionHandler ClientTryConnect;
                    private class ASResult : IAsyncResult
                    {
                        public bool IsCompleted { get; set; }

                        public WaitHandle AsyncWaitHandle { get; set; }

                        public object AsyncState { get; set; }

                        public bool CompletedSynchronously { get; set; }
                    }
                    public void DisconnectClient(TCPClient client)
                    {
                        try { client.ClientSocket.Shutdown(SocketShutdown.Both); } catch { }
                        try { client.ClientSocket.Close(); } catch { }
                        RemoveClient(client.ClientSocket);
                        ClientDisconnected?.Invoke(this, new ClientConnectionArgs(client, "Manual Disconnection through 'Server.DisconnectClient()'"));
                    }
                    public void DisconnectClient(TCPClient client, string msg)
                    {
                        try { client.ClientSocket.Send(Encoding.ASCII.GetBytes(msg)); client.ClientSocket.Shutdown(SocketShutdown.Both); } catch { }
                        try { client.ClientSocket.Close(); } catch { }
                        RemoveClient(client.ClientSocket);
                        ClientDisconnected?.Invoke(this, new ClientConnectionArgs(client, msg));
                    }
                    public void BeginReceive(int clientIndex)
                    {
                        BeginReceive(clients[clientIndex]);
                    }
                    public void BeginReceive(TCPClient client)
                    {
                        if (clients.Contains(client))
                            client.ClientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnClientDataReceived, client);
                        else
                            throw new Exception("Client is either not connected or not been listed correctly!");
                    }
                    public void Start()
                    {
                        ServerSocket.Listen(0);
                        ServerSocket.BeginAccept(OnClientConnection, null);
                        Running = true;
                    }
                    public IPAddress Ip { get; set; }
                    public ushort Port { get; set; }
                    private void OnClientConnectionFailed(Socket client, string msg = null)
                    {
                        ClientConnectionFailed?.Invoke(this, new ClientConnectionArgs(new TCPClient(client), msg == null ? "" : msg));
                    }
                    public event EventHandler<ClientConnectionArgs> ClientConnectionFailed;
                    public event EventHandler<ClientConnectionArgs> ClientConnected;
                    public event EventHandler<ClientConnectionArgs> ClientDisconnected;
#pragma warning disable CS0067 // false warning disabled
                    public event EventHandler<ClientDataArgs> ClientDataReceived;
#pragma warning restore CS0067
                    public class ClientDataArgs : EventArgs
                    {
                        public ClientDataArgs(TCPClient client, object data)
                        {
                            Client = client;
                            Data = data;
                        }

                        public TCPClient Client { get; }
                        public object Data { get; }
                    }
                    public class ClientConnectionArgs : EventArgs
                    {
                        public ClientConnectionArgs(TCPClient client, string msg)
                        {
                            Client = client;
                            Msg = msg;
                        }

                        public TCPClient Client { get; }
                        public string Msg { get; }
                    }
                }
            }
            public class FTPClient
            {
                internal static List<string> Add(List<string> list, List<string> add)
                {
                    foreach (var val in add) list.Add(val); return list;
                }
                public class FTPDirectory
                {
                    public static string GetPath(string path)
                    {
                        return new FTPDirectory(null, path).Path;
                    }
                    public FTPDirectory(FTPClient client, string path)
                    {
                        Client = client;
                        Path = path;
                    }
                    public void Move(string newPath)
                    {
                        Name = newPath;
                        Path = newPath;
                    }
                    public string[] ListAllDirectories()
                    {
                        List<string> dirs = new List<string>();
                        foreach (var directory in ListDirectories())
                            if (Client.IsDirectory(directory))
                                if (directory.Split('/').Last() != "." && directory.Split('/').Last() != "..")
                                { dirs.Add(Client.GetDirectory(directory).Path); dirs = Add(dirs, Client.GetDirectory(directory).ListAllDirectories().ToList()); }
                        return dirs.ToArray();
                    }
                    public string[] ListAllFiles()
                    {
                        List<string> files = new List<string>();
                        files = Add(files, ListFiles().ToList());
                        foreach (var dir in ListAllDirectories())
                            files = Add(files, Client.GetDirectory(dir).ListFiles().ToList());
                        return files.ToArray();
                    }
                    public void Download(string dest)
                    {
                        if (System.IO.Directory.Exists(dest))
                            Download(dest + "\\" + Name, true);
                        else
                        {
                            var directories = ListAllDirectories();
                            var files = ListAllFiles();
                            var dir = new System.IO.DirectoryInfo(dest);
                            if (dir.Exists)
                                dir.Delete(true);
                            dir.Create();
                            foreach (var d in directories)
                                new System.IO.DirectoryInfo(dir.FullName + "\\" + Client.GetDirectory(d).Path.Substring(this.Path.Length).Replace("/", "\\")).Create();
                            foreach (var f in files)
                                Client.GetFile(f).Download(new System.IO.FileInfo(dir.FullName + "\\" + Client.GetFile(f).Path.Substring(this.Path.Length).Replace("/", "\\")).FullName);
                        }
                    }
                    public void Download(string dest, bool overwrite)
                    {
                        if (overwrite)
                            if (System.IO.Directory.Exists(dest))
                                System.IO.Directory.Delete(dest, true);
                        Download(dest);
                    }
                    private string GetDirectoryUploadPath(System.IO.DirectoryInfo root, System.IO.DirectoryInfo dir)
                    {
                        return Path + (dir.FullName.Substring(root.FullName.Length)).Replace("\\", "/");
                    }
                    private string GetFileUploadPath(System.IO.DirectoryInfo root, System.IO.FileInfo file)
                    {
                        return Path + (file.FullName.Substring(root.FullName.Length)).Replace("\\", "/");
                    }
                    public bool Exists()
                    {
                        try
                        {
                            var request = Client.CreateRequest(Path + "/7192h89ca67384h18913nf8asuy934081h9.txt");
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = true;
                            request.UsePassive = true;
                            request.KeepAlive = true;
                            System.IO.Stream requestStream = request.GetRequestStream();
                            requestStream.Close();
                            request = Client.CreateRequest(Path + "/7192h89ca67384h18913nf8asuy934081h9.txt");
                            request.Method = WebRequestMethods.Ftp.DeleteFile;
                            request.GetResponse();
                            return true;
                        }
                        catch { return false; }
                    }
                    public void UploadAsync(string src)
                    {
                        new System.Threading.Thread(new ParameterizedThreadStart(InternalUploadAsync)).Start(src);
                    }
                    public int MaxValue = -1;
                    public event ProgressChangedEventHandler AsyncUploadProgressChanged;
                    public event ProgressChangedEventHandler AsyncUploadComplete;
                    public event EventHandler MaxValueObtained;
                    void InternalUploadAsync(object src)
                    {
                        if (Exists())
                            Delete(true);
                        Create();
                        var dir = new System.IO.DirectoryInfo(src.ToString());
                        var dirs = dir.GetAllDirectories();
                        var files = dir.GetAllFiles();
                        MaxValue = dirs.Length + files.Length * 100;
                        MaxValue--;
                        MaxValueObtained?.Invoke(this, EventArgs.Empty);
                        var progress = 0;
                        foreach (var directory in dirs)
                        {
                            new FTPDirectory(Client, GetDirectoryUploadPath(dir, directory)).Create();
                            progress++;
                            AsyncUploadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress, "Creating Directory: " + directory.Name));
                        }
                        foreach (var file in files)
                        {
                            var request = Client.CreateRequest(GetFileUploadPath(dir, file));
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = true;
                            request.UsePassive = true;
                            var fs = System.IO.File.ReadAllBytes(file.FullName);
                            var rs = request.GetRequestStream();
                            var currentFileProgress = 0;
                            for (int offset = 0; offset < fs.Length; offset += 1024)
                            {
                                currentFileProgress = (int)(offset * 100 / fs.Length);
                                AsyncUploadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress + currentFileProgress, "Uploading File: " + file.Name));
                                var cs = fs.Length - offset;
                                if (cs > 1024) cs = 1024;
                                rs.Write(fs, offset, cs);
                            }
                            progress += 100;
                            rs.Close();
                            rs.Dispose();
                        }
                        AsyncUploadComplete?.Invoke(this, new ProgressChangedEventArgs(MaxValue, "Complete"));
                    }
                    public void UploadDirectory(string src)
                    {
                        if (Exists())
                            Delete(true);
                        Create();
                        var dir = new System.IO.DirectoryInfo(src);
                        foreach (var directory in dir.GetAllDirectories())
                            new FTPDirectory(Client, GetDirectoryUploadPath(dir, directory)).Create();
                        foreach (var file in dir.GetAllFiles())
                            new FTPFile(Client, GetFileUploadPath(dir, file)).UploadFile(file.FullName);
                    }
                    public string[] ListDirectories(bool fullnames = true)
                    {
                        var all = List();
                        List<string> files = new List<string>();
                        foreach (var val in all)
                            if (Client.IsDirectory(val))
                                files.Add(val);
                        return files.ToArray();
                    }
                    public string[] ListFiles(bool fullnames = true)
                    {
                        var all = List();
                        List<string> files = new List<string>();
                        foreach (var val in all)
                            if (!Client.IsDirectory(val))
                                files.Add(val);
                        return files.ToArray();
                    }
                    public string[] List(bool fullnames = true)
                    {
                        StringBuilder result = new StringBuilder();
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.ListDirectory;
                        FtpWebResponse responseDir = (FtpWebResponse)request.GetResponse();
                        System.IO.StreamReader readerDir = new System.IO.StreamReader(responseDir.GetResponseStream());
                        string line = readerDir.ReadLine();
                        while (line != null)
                        {
                            if (line.Split('/').Last() != "." && line.Split('/').Last() != "..")
                            {
                                if (fullnames)
                                    result.Append(Path + "/" + line.Split('/').Last());
                                else
                                    result.Append("/" + line.Split('/').Last());
                                result.Append("\n");
                            }
                            line = readerDir.ReadLine();
                        }
                        if (result.ToString() == "")
                            return new string[0];
                        result.Remove(result.ToString().LastIndexOf('\n'), 1);
                        responseDir.Close();
                        return result.ToString().Split('\n');
                    }
                    public string Create()
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            return response.StatusDescription;
                    }
                    public void Delete(bool recursive = false)
                    {
                        var list = List();
                        if (recursive)
                            if (list.Length != 0)
                                foreach (var file in list)
                                    if (Client.IsDirectory(file))
                                        if (file.Split('/').Last() != "." && file.Split('/').Last() != "..")
                                            new FTPDirectory(Client, file).Delete(true);
                                        else { }
                                    else
                                        if (file.Split('/').Last() != "." && file.Split('/').Last() != "..")
                                        if (new FTPFile(Client, file).Exists())
                                            new FTPFile(Client, file).Delete();
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                        request.UseBinary = true;
                        request.UsePassive = true;
                        request.KeepAlive = true;
                        request.GetResponse().Close();
                    }
                    public string Name
                    {
                        get
                        {
                            return Path.Split('/')[Path.Split('/').Length - 1];
                        }
                        set
                        {
                            var ftpRequest = Client.CreateRequest(Path);
                            ftpRequest.UseBinary = true;
                            ftpRequest.UsePassive = false;
                            ftpRequest.KeepAlive = false;
                            ftpRequest.Proxy = null;
                            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                            ftpRequest.RenameTo = value;
                            var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                            ftpResponse.Close();
                            var arr = Path.Split('/').ToList();
                            arr.RemoveAt(arr.Count - 1);
                            arr.Add(value);
                            var newPath = "";
                            foreach (var v in arr)
                                newPath += "/" + v;
                            Path = newPath;
                        }
                    }

                    public FTPClient Client { get; }
                    private string _path = "";
                    public string Path
                    {
                        get => _path;
                        private set { _path = value.Replace("\\", "/"); if (value.Replace("\\", "/") == "/") return; if (_path.Last() == '/') _path = _path.Substring(0, _path.Length - 1); if (_path.Substring(0, 1) != "/") _path = "/" + _path; }
                    }
                }
                public class FTPFile
                {
                    public static string GetPath(string path)
                    {
                        return new FTPFile(null, path).Path;
                    }
                    public FTPFile(FTPClient client, string path)
                    {
                        Client = client;
                        Path = path;
                    }
                    public FTPClient Client { get; }
                    private string _path = "";
                    public string Path
                    {
                        get => _path;
                        private set { if (value.Replace("\\", "/").Substring(0, 1) != "/") _path = "/" + value.Replace("\\", "/"); else _path = value.Replace("\\", "/"); }
                    }
                    public virtual void Move(string newPath)
                    {
                        Name = newPath;
                        Path = newPath;
                    }
                    public FtpWebRequest GetRequest() => Client.CreateRequest(Path);
                    public FtpWebRequest GetDownloadRequest()
                    {
                        FtpWebRequest request = Client.CreateRequest(Path);
                        request.KeepAlive = true;
                        request.UsePassive = true;
                        request.UseBinary = true;
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        return request;
                    }
                    public FtpWebRequest GetUploadRequest()
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.UseBinary = true;
                        request.KeepAlive = true;
                        request.UsePassive = true;
                        return request;
                    }
                    public virtual string Create()
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        System.IO.StreamWriter requestStream = new System.IO.StreamWriter(request.GetRequestStream());
                        requestStream.Write("");
                        requestStream.Close();
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            return $"Upload File Complete, status: {response.StatusDescription}";
                    }
                    public virtual void Download(string dest)
                    {
                        FtpWebRequest request = Client.CreateRequest(Path);
                        request.KeepAlive = true;
                        request.UsePassive = true;
                        request.UseBinary = true;
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) // Error here
                        using (System.IO.Stream responseStream = response.GetResponseStream())
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(responseStream))
                        using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(new System.IO.FileInfo(dest).OpenWrite()))
                        {
                            while (true)
                                try { writer.Write(reader.ReadByte()); }
                                catch { writer.Close(); break; }
                        }
                    }
                    public static void DownloadFile(FTPClient connection, string webFile, string dest) => connection.GetFile(webFile).Download(dest);
                    public virtual void UploadAsync(string src)
                    {
                        new System.Threading.Thread(new ParameterizedThreadStart(InternalUploadAsync)).Start(src);
                    }
                    public event ProgressChangedEventHandler AsyncUploadProgressChanged;
                    public event ProgressChangedEventHandler AsyncUploadComplete;
                    void InternalUploadAsync(object src)
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.UseBinary = true;
                        request.UsePassive = true;
                        var fs = System.IO.File.ReadAllBytes(src.ToString());
                        var rs = request.GetRequestStream();
                        for (int offset = 0; offset < fs.Length; offset += 1024)
                        {
                            AsyncUploadProgressChanged?.Invoke(this, new ProgressChangedEventArgs((int)(offset * 100 / fs.Length), "uploading"));
                            var cs = fs.Length - offset;
                            if (cs > 1024) cs = 1024;
                            rs.Write(fs, offset, cs);
                        }
                        rs.Close();
                        rs.Dispose();
                        AsyncUploadComplete?.Invoke(this, new ProgressChangedEventArgs(100, "complete"));
                    }
                    public virtual void UploadFile(string src)
                    {
                        System.IO.FileStream fs = System.IO.File.OpenRead(src);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        try
                        {
                            var request = Client.CreateRequest(Path);
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = true;
                            request.UsePassive = true;
                            request.KeepAlive = true;
                            request.ConnectionGroupName = "group";
                            System.IO.Stream requestStream = request.GetRequestStream();
                            requestStream.Write(buffer, 0, buffer.Length);
                            requestStream.Flush();
                            requestStream.Close();
                        }
                        catch (Exception)
                        {
                            var request = Client.CreateRequest(Path);
                            request.Method = WebRequestMethods.Ftp.UploadFile;
                            request.UseBinary = true;
                            request.UsePassive = true;
                            request.KeepAlive = true;
                            request.ConnectionGroupName = "group";
                            System.IO.Stream requestStream = request.GetRequestStream();
                            requestStream.Write(buffer, 0, buffer.Length);
                            requestStream.Flush();
                            requestStream.Close();
                        }
                    }
                    public static void UploadFile(FTPClient connection, string webDest, string src) => connection.GetFile(webDest).UploadFile(src);
                    public virtual bool Exists()
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.GetFileSize;
                        try
                        {
                            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                            return true;
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response.StatusCode ==
                                FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    public FTPDirectory Directory
                    {
                        get
                        {
                            if (Path == "/" + Name)
                                return new FTPDirectory(Client, "/");
                            else
                            {
                                var dir = "";
                                for (int i = 1; i < Path.Split('/').Length - 1; i++)
                                    dir += "/" + Path.Split('/')[i];
                                return new FTPDirectory(Client, dir + "/");
                            }
                        }
                    }
                    public virtual string Delete()
                    {
                        var request = Client.CreateRequest(Path);
                        request.Method = WebRequestMethods.Ftp.DeleteFile;
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            return response.StatusDescription;
                    }
                    public string Name
                    {
                        get
                        {
                            return Path.Split('/')[Path.Split('/').Length - 1];
                        }
                        set
                        {
                            var ftpRequest = Client.CreateRequest(Path);
                            ftpRequest.UseBinary = true;
                            ftpRequest.UsePassive = false;
                            ftpRequest.KeepAlive = false;
                            ftpRequest.Proxy = null;
                            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                            ftpRequest.RenameTo = value;
                            var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                            ftpResponse.Close();
                            var arr = Path.Split('/').ToList();
                            arr.RemoveAt(arr.Count - 1);
                            arr.Add(value);
                            var newPath = "";
                            foreach (var v in arr)
                                newPath += "/" + v;
                            Path = newPath;
                        }
                    }
                }
                public string UserName { get; private set; }
                public string Password { get; private set; }
                public int Port { get; private set; }
                public string Session { get; private set; }
                internal FtpWebRequest CreateRequest()
                {
                    var request = (FtpWebRequest)WebRequest.Create("ftp://" + Session);
                    request.Credentials = new NetworkCredential(this.UserName, this.Password);
                    return request;
                }
                internal FtpWebRequest CreateRequest(string path)
                {
                    var request = (FtpWebRequest)WebRequest.Create("ftp://" + Session + path);
                    request.Credentials = new NetworkCredential(this.UserName, this.Password);
                    return request;
                }
                public static FTPClient Create(string connectionString)
                {
                    return new FTPClient(connectionString);
                }
                public FTPClient(string connectionString, char assigner = '=')
                {
                    UserName = connectionString.GetRegexMatch("(" + "UserName".GetRegexLowerUpperPattern() + assigner.ToString() + ")+([\\w\\.\\@]+),?").Groups[2].Value;
                    Password = connectionString.GetRegexMatch("(" + "Password".GetRegexLowerUpperPattern() + assigner.ToString() + ")+([\\w\\.\\@]+),?").Groups[2].Value;
                    Session = connectionString.GetRegexMatch("(" + "Session".GetRegexLowerUpperPattern() + assigner.ToString() + ")+([\\w\\.\\@]+),?").Groups[2].Value;
                    Port = 21;
                }
                public FTPClient(string _userName, string _password, string _session)
                {
                    UserName = _userName;
                    Password = _password;
                    Session = _session;
                    Port = 21;
                }
                public bool IsDirectory(string path)
                {
                    return !path.Split('/').Last().Contains(".");
                }
                public void DeleteDirectory(string path, bool recursive = false) => GetDirectory(path).Delete(recursive);
                public void CreateDirectory(string path) => GetDirectory(path).Create();
                public void DeleteFile(string path) => GetFile(path).Delete();
                public void CreateFile(string path) => GetFile(path).Create();
                public void DownloadFile(string src, string dest) => GetFile(src).Download(dest);
                public void UploadFile(string src, string dest) => GetFile(dest).UploadFile(src);
                public void MoveFile(string path, string newPath)
                {
                    var file = GetFile(path);
                    file.Move(newPath);
                }
                public string[] ListDirectory(string path = "/")
                {
                    return GetDirectory(path).List();
                }
                public FTPDirectory GetDirectory(string path = "/")
                {
                    return new FTPDirectory(this, path);
                }
                public FTPFile GetFile(string path)
                {
                    return new FTPFile(this, path);
                }
            }
        }
        public static class Strings
        {
            public static string StrReverse(string str)
            {
                string rev = "";
                for (int i = str.Length - 1; i >= 0; i--)
                {
                    rev += str[i].ToString();
                }
                return rev;
            }
            public static char AddToChar(char Character, int Ammount = 1)
            {
                if (Ammount < 1)
                {
                    return Character;
                }
                string newChar = Character.ToString();
                for (int i = 0; i < Ammount; i++)
                {
                    byte btValue = Convert.ToByte(char.Parse(newChar));
                    var arr = new byte[1];
                    arr[0] = Convert.ToByte(btValue + 1);
                    newChar = ASCIIEncoding.ASCII.GetString(arr);
                    if (newChar == "{") { newChar = "a"; }
                    if (newChar == "[") { newChar = "A"; }
                }
                return char.Parse(newChar);
            }
            public static string AddToChars(string chars, int Ammount = 1)
            {
                if (Ammount < 1)
                {
                    return chars;
                }
                string newStr = "";
                foreach (char chr in chars.ToCharArray())
                {
                    newStr += AddToChar(chr, Ammount);
                }
                return newStr;
            }
            public static string AddToChars(char[] chars, int Ammount = 1)
            {
                string Str = "";
                foreach (char chr in chars)
                {
                    Str += chr.ToString();
                }
                return AddToChars(Str, Ammount);
            }
        }
        namespace DataTypeExtensions
        {
            namespace IO
            {
                public static class IOClassExtensions
                {
                    public static string GetFileHash(this FileInfo file, HashingAlgorithm algorithm)
                    {
                        if (algorithm == HashingAlgorithm.MD5)
                        {
                            string filePath = file.FullName;
                            byte[] buffer;
                            int bytesRead;
                            long size;
                            long totalBytesRead = 0;
                            using (System.IO.Stream f = System.IO.File.OpenRead(filePath))
                            {
                                size = f.Length;
                                using (System.Security.Cryptography.HashAlgorithm hasher = System.Security.Cryptography.MD5.Create())
                                {
                                    do
                                    {
                                        buffer = new byte[4096];
                                        bytesRead = f.Read(buffer, 0, buffer.Length);
                                        totalBytesRead += bytesRead;
                                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    } while (bytesRead != 0);
                                    hasher.TransformFinalBlock(buffer, 0, 0);
                                    StringBuilder builder = new StringBuilder(40);
                                    foreach (byte b in hasher.Hash)
                                    {
                                        builder.Append(b.ToString("X2").ToLower());
                                    }
                                    return builder.ToString();
                                }
                            }
                        }
                        else if (algorithm == HashingAlgorithm.SHA1)
                        {
                            string filePath = file.FullName;
                            byte[] buffer;
                            int bytesRead;
                            long size;
                            long totalBytesRead = 0;
                            using (System.IO.Stream f = System.IO.File.OpenRead(filePath))
                            {
                                size = f.Length;
                                using (System.Security.Cryptography.HashAlgorithm hasher = System.Security.Cryptography.SHA1.Create())
                                {
                                    do
                                    {
                                        buffer = new byte[4096];
                                        bytesRead = f.Read(buffer, 0, buffer.Length);
                                        totalBytesRead += bytesRead;
                                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    } while (bytesRead != 0);
                                    hasher.TransformFinalBlock(buffer, 0, 0);
                                    StringBuilder builder = new StringBuilder(40);
                                    foreach (byte b in hasher.Hash)
                                    {
                                        builder.Append(b.ToString("X2").ToLower());
                                    }
                                    return builder.ToString();
                                }
                            }
                        }
                        else if (algorithm == HashingAlgorithm.SHA256)
                        {
                            string filePath = file.FullName;
                            byte[] buffer;
                            int bytesRead;
                            long size;
                            long totalBytesRead = 0;
                            using (System.IO.Stream f = System.IO.File.OpenRead(filePath))
                            {
                                size = f.Length;
                                using (System.Security.Cryptography.HashAlgorithm hasher = System.Security.Cryptography.SHA256.Create())
                                {
                                    do
                                    {
                                        buffer = new byte[4096];
                                        bytesRead = f.Read(buffer, 0, buffer.Length);
                                        totalBytesRead += bytesRead;
                                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    } while (bytesRead != 0);
                                    hasher.TransformFinalBlock(buffer, 0, 0);
                                    StringBuilder builder = new StringBuilder(40);
                                    foreach (byte b in hasher.Hash)
                                    {
                                        builder.Append(b.ToString("X2").ToLower());
                                    }
                                    return builder.ToString();
                                }
                            }
                        }
                        else if (algorithm == HashingAlgorithm.SHA384)
                        {
                            string filePath = file.FullName;
                            byte[] buffer;
                            int bytesRead;
                            long size;
                            long totalBytesRead = 0;
                            using (System.IO.Stream f = System.IO.File.OpenRead(filePath))
                            {
                                size = f.Length;
                                using (System.Security.Cryptography.HashAlgorithm hasher = System.Security.Cryptography.SHA384.Create())
                                {
                                    do
                                    {
                                        buffer = new byte[4096];
                                        bytesRead = f.Read(buffer, 0, buffer.Length);
                                        totalBytesRead += bytesRead;
                                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    } while (bytesRead != 0);
                                    hasher.TransformFinalBlock(buffer, 0, 0);
                                    StringBuilder builder = new StringBuilder(40);
                                    foreach (byte b in hasher.Hash)
                                    {
                                        builder.Append(b.ToString("X2").ToLower());
                                    }
                                    return builder.ToString();
                                }
                            }
                        }
                        else if (algorithm == HashingAlgorithm.SHA512)
                        {
                            string filePath = file.FullName;
                            byte[] buffer;
                            int bytesRead;
                            long size;
                            long totalBytesRead = 0;
                            using (System.IO.Stream f = System.IO.File.OpenRead(filePath))
                            {
                                size = f.Length;
                                using (System.Security.Cryptography.HashAlgorithm hasher = System.Security.Cryptography.SHA512.Create())
                                {
                                    do
                                    {
                                        buffer = new byte[4096];
                                        bytesRead = f.Read(buffer, 0, buffer.Length);
                                        totalBytesRead += bytesRead;
                                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    } while (bytesRead != 0);
                                    hasher.TransformFinalBlock(buffer, 0, 0);
                                    StringBuilder builder = new StringBuilder(40);
                                    foreach (byte b in hasher.Hash)
                                    {
                                        builder.Append(b.ToString("X2").ToLower());
                                    }
                                    return builder.ToString();
                                }
                            }
                        }
                        return null;
                    }
                }
            }
            namespace RegularExpressions
            {
                public static class RegexClassExtensions
                {

                    public static string[] RegexSplit(this string str, string regexPattern, RegexOptions options) => Regex.Split(str, regexPattern, options);
                    public static string[] RegexSplit(this string str, string regexPattern) => Regex.Split(str, regexPattern);
                    public static string RegexMatch(this string str, string regexPattern) { try { var m = Regex.Match(str, regexPattern); if (!m.Success) return null; else return m.Value; } catch { return null; } }
                    public static string RegexReplace(this string str, string regexFindPattern, string replaceWith) => Regex.Replace(str, regexFindPattern, replaceWith);
                    public static bool MatchRegex(this string str, string regexPattern) => Regex.IsMatch(str, regexPattern);
                    public static Match GetRegexMatch(this string str, string regexPattern) => Regex.Match(str, regexPattern);
                    public static Match[] GetRegexMatches(this string str, string regexPattern) { MatchCollection m = Regex.Matches(str, regexPattern); List<Match> tmp = new List<Match>(); for (int i = 0; i < m.Count; i++) tmp.Add(m[i]); return tmp.ToArray(); }
                    /// <summary>
                    /// Returns a search pattern for lower and uppercase letters on a literal
                    /// Example: username -> [uU][sS][eE][rR][nN][aA][mM][eE]
                    /// </summary>
                    public static string GetRegexLowerUpperPattern(this string str)
                    {
                        var ret = "";
                        foreach (var c in str)
                            if (c.ToString().ToLower() != c.ToString().ToUpper())
                                ret += "[" + c.ToString().ToLower() + c.ToString().ToUpper() + "]";
                            else
                                ret += c.ToString();
                        return ret;
                    }
                    public static string[] RegexMatches(this string str, string regexPattern)
                    {
                        var m = Regex.Matches(str, regexPattern);
                        List<string> tmp = new List<string>();
                        if (!(m.Count > 0))
                            return null;
                        foreach (Match match in m)
                            tmp.Add(match.Value);
                        return tmp.ToArray();
                    }
                }
            }
            public static class GeneralClassExtensions
            {
                #region PrivateMembers
                private static List<System.IO.FileInfo> Add(List<System.IO.FileInfo> src, List<System.IO.FileInfo> @new)
                {
                    foreach (var val in @new)
                        src.Add(val);
                    return src;
                }
                private static List<System.IO.DirectoryInfo> Add(List<System.IO.DirectoryInfo> src, List<System.IO.DirectoryInfo> @new)
                {
                    foreach (var val in @new)
                        src.Add(val);
                    return src;
                }
                #endregion
                #region GeneralClasses
                public static bool IsOdd(this int v) => v % 2 != 0;
                public static Enumerator GetEnum<Enumerator>(this string obj, Enumerator defaultReturn) where Enumerator : Enum
                {
                    foreach (var val in Enum.GetNames(typeof(Enumerator)))
                        if (val.ToString().ToLower() == obj.ToLower()) return (Enumerator)Enum.Parse(typeof(Enumerator), val);
                    return defaultReturn;
                }
                public static Enumerator GetEnum<Enumerator>(this int obj, Enumerator defaultReturn) where Enumerator : Enum
                {
                    foreach (var val in Enum.GetNames(typeof(Enumerator)))
                        if ((int)Enum.Parse(typeof(Enumerator), val) == obj)
                            return (Enumerator)Enum.Parse(typeof(Enumerator), val);
                    return defaultReturn;
                }
                public static void Add<T>(this List<T> list, List<T> add)
                {
                    foreach (var val in add)
                        list.Add(val);
                }
                public static bool ToBool(this string v) => (v == "true" || v == "1");
                public static T[] Search<T>(this T[] obj, string search, string propname) => obj.Search(search, propname, false);
                public static List<T> Search<T>(this List<T> obj, string search, string propname, bool caseSensitive) => obj.ToArray().Search(search, propname, caseSensitive).ToList();
                public static List<T> Search<T>(this List<T> obj, string search, string propname) => obj.ToList().Search(search, propname, false);
                public static List<string> Search(this List<string> obj, string search) => Search(obj.ToArray(), search).ToList();
                public static List<string> Search(this List<string> obj, string search, bool caseSensitive) => Search(obj.ToArray(), search, caseSensitive).ToList();
                public static string[] Search(this string[] obj, string search) => Search(obj, search, false);
                public static string[] Search(this string[] obj, string search, bool caseSensitive)
                {
                    if (caseSensitive)
                    {
                        List<string> ret = new List<string>();
                        foreach (var o in obj)
                            if (o.Contains(search))
                                ret.Add(o);
                        return ret.ToArray();
                    }
                    else
                    {
                        List<string> ret = new List<string>();
                        foreach (var o in obj)
                            if (o.ToLower().Contains(search.ToLower()))
                                ret.Add(o);
                        return ret.ToArray();
                    }
                }
                public static T[] Search<T>(this T[] obj, string search, string propname, bool caseSensitive)
                {
                    List<T> res = new List<T>();
                    foreach (var o in obj)
                        try { if (caseSensitive) if (o.GetType().GetProperty(propname).GetValue(o, null).ToString().Contains(search)) res.Add(o); else { } else if (o.GetType().GetProperty(propname).GetValue(o, null).ToString().ToLower().Contains(search.ToLower())) res.Add(o); }
                        catch { }
                    return res.ToArray();
                }
                public static ResultType[] GetPropertyArray<ResultType>(this object[] obj, string propname)
                {
                    List<ResultType> props = new List<ResultType>();
                    foreach (var o in obj)
                        props.Add((ResultType)o.GetType().GetProperty(propname).GetValue(o, null));
                    return props.ToArray();
                }
                public static object[] ToObjectArray<T>(this T[] obj)
                {
                    List<object> objs = new List<object>();
                    foreach (var o in obj)
                        objs.Add((object)o);
                    return objs.ToArray();
                }
                public static object ToObject<T>(this T obj) => (object)obj;
                #endregion
                #region IO   
                public static void SaveXML<T>(this T obj, string path)
                {
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    System.IO.TextWriter tw = new System.IO.StreamWriter(path);
                    serializer.Serialize(tw, obj);
                    tw.Close();
                }
                public static T LoadXML<T>(this T obj, string path)
                {
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    System.IO.TextReader tr = new System.IO.StreamReader(path);
                    var rd = serializer.Deserialize(tr);
                    tr.Close();
                    return (T)rd;
                }
                public static System.IO.FileInfo[] GetAllFiles(this System.IO.DirectoryInfo dir)
                {
                    List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
                    foreach (var file in dir.GetFiles())
                        files.Add(file);
                    foreach (var directory in dir.GetDirectories())
                        files = Add(files, directory.GetAllFiles().ToList());
                    return files.ToArray();
                }
                public static System.IO.FileInfo[] GetAllFiles(this System.IO.DirectoryInfo dir, string searchPattern)
                {
                    List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
                    foreach (var file in dir.GetFiles(searchPattern))
                        files.Add(file);
                    foreach (var directory in dir.GetDirectories())
                        files = Add(files, directory.GetAllFiles(searchPattern).ToList());
                    return files.ToArray();
                }
                public static System.IO.FileInfo[] GetAllFiles(this System.IO.DirectoryInfo dir, string searchPattern, System.IO.SearchOption searchOption)
                {
                    List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
                    foreach (var file in dir.GetFiles(searchPattern, searchOption))
                        files.Add(file);
                    foreach (var directory in dir.GetDirectories())
                        files = Add(files, directory.GetAllFiles(searchPattern, searchOption).ToList());
                    return files.ToArray();
                }
                public static System.IO.DirectoryInfo[] GetAllDirectories(this System.IO.DirectoryInfo dir)
                {
                    List<System.IO.DirectoryInfo> dirs = new List<System.IO.DirectoryInfo>();
                    foreach (var directory in dir.GetDirectories())
                    { dirs.Add(directory); dirs = Add(dirs, directory.GetAllDirectories().ToList()); }
                    return dirs.ToArray();
                }
                public static System.IO.DirectoryInfo[] GetAllDirectories(this System.IO.DirectoryInfo dir, string searchPattern)
                {
                    List<System.IO.DirectoryInfo> dirs = new List<System.IO.DirectoryInfo>();
                    foreach (var directory in dir.GetDirectories(searchPattern))
                        dirs.Add(directory);
                    foreach (var directory in dir.GetDirectories())
                        dirs = Add(dirs, directory.GetAllDirectories(searchPattern).ToList());
                    return dirs.ToArray();
                }
                public static System.IO.DirectoryInfo[] GetAllDirectories(this System.IO.DirectoryInfo dir, string searchPattern, System.IO.SearchOption searchOption)
                {
                    List<System.IO.DirectoryInfo> dirs = new List<System.IO.DirectoryInfo>();
                    foreach (var directory in dir.GetDirectories(searchPattern, searchOption))
                        dirs.Add(directory);
                    foreach (var directory in dir.GetDirectories())
                        dirs = Add(dirs, directory.GetAllDirectories(searchPattern, searchOption).ToList());
                    return dirs.ToArray();
                }
                #endregion
                #region Formatting
                public static string FormatAsXML(this string v)
                {
                    return Data.Format.XML(v);
                }
                public static string FormatAsJSON(this string v)
                {
                    return Data.Format.JSON(v);
                }
                public static string FormatAsJSON(this string v, string Indent)
                {
                    return Data.Format.JSON(v, Indent);
                }
                public static string FormatAsJS(this string v)
                {
                    return Data.Format.JS(v);
                }
                public static string FormatAsJS(this string v, char indent_char, int indent_size, int indent_level)
                {
                    return Data.Format.JS(v, indent_char, indent_size, indent_level);
                }
                public static string FormatAsJS(this string v, char indent_char)
                {
                    return Data.Format.JS(v, indent_char);
                }
                public static string FormatAsJS(this string v, char indent_char, int indent_size)
                {
                    return Data.Format.JS(v, indent_char, indent_size);
                }
                public static string FormatAsJS(this string v, int indent_size)
                {
                    return Data.Format.JS(v, indent_size);
                }
                public static string FormatAsJS(this string v, int indent_level, char indent_char = ' ')
                {
                    return Data.Format.JS(v, indent_level, indent_char);
                }
                #endregion
                #region Converter
                public static string ToHex(this int v)
                {
                    return Converter.DecimalToHexadecimal(v);
                }
                public static string ToBinary(this int v)
                {
                    return Converter.DecimalToBinary(v);
                }
                public static int ToOctal(this int v)
                {
                    return int.Parse(Converter.DecimalToOctal(v));
                }
                public static int ToInt(this string v)
                {
                    if (v.ToUpper().Contains("A") || v.ToUpper().Contains("B") || v.ToUpper().Contains("C") || v.ToUpper().Contains("D") || v.ToUpper().Contains("E") || v.ToUpper().Contains("F"))
                        return Converter.HexadecimalToDecimal(v);
                    else
                        return int.Parse(v);
                }
                public static string ToBinary(this string v)
                {
                    return Converter.TextToBinary(v);
                }
                public static string ToHex(this string v)
                {
                    return Converter.TextToHexadecimal(v);
                }
                #endregion
            }
        }
        namespace Data
        {
            using System;
            using System.Collections;
            using System.IO;

            public class IniFile
            {
                private Hashtable keyPairs = new Hashtable();
                private String iniFilePath;

                private struct SectionPair
                {
                    public String Section;
                    public String Key;
                }

                /// <summary>
                /// Opens the INI file at the given path and enumerates the values in the IniParser.
                /// </summary>
                /// <param name="iniPath">Full path to INI file.</param>
                public IniFile(String iniPath)
                {
                    TextReader iniFile = null;
                    String strLine = null;
                    String currentRoot = null;
                    String[] keyPair = null;

                    iniFilePath = iniPath;

                    if (File.Exists(iniPath))
                    {
                        try
                        {
                            iniFile = new StreamReader(iniPath);

                            strLine = iniFile.ReadLine();

                            while (strLine != null)
                            {
                                strLine = strLine.Trim().ToUpper();

                                if (strLine != "")
                                {
                                    if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                                    {
                                        currentRoot = strLine.Substring(1, strLine.Length - 2);
                                    }
                                    else
                                    {
                                        keyPair = strLine.Split(new char[] { '=' }, 2);

                                        SectionPair sectionPair;
                                        String value = null;

                                        if (currentRoot == null)
                                            currentRoot = "ROOT";

                                        sectionPair.Section = currentRoot;
                                        sectionPair.Key = keyPair[0];

                                        if (keyPair.Length > 1)
                                            value = keyPair[1];

                                        keyPairs.Add(sectionPair, value);
                                    }
                                }

                                strLine = iniFile.ReadLine();
                            }

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            if (iniFile != null)
                                iniFile.Close();
                        }
                    }
                    else
                        throw new FileNotFoundException("Unable to locate " + iniPath);

                }

                /// <summary>
                /// Returns the value for the given section, key pair.
                /// </summary>
                /// <param name="sectionName">Section name.</param>
                /// <param name="settingName">Key name.</param>
                public String GetSetting(String sectionName, String settingName)
                {
                    SectionPair sectionPair;
                    sectionPair.Section = sectionName.ToUpper();
                    sectionPair.Key = settingName.ToUpper();

                    return (String)keyPairs[sectionPair];
                }

                /// <summary>
                /// Enumerates all lines for given section.
                /// </summary>
                /// <param name="sectionName">Section to enum.</param>
                public String[] EnumSection(String sectionName)
                {
                    ArrayList tmpArray = new ArrayList();

                    foreach (SectionPair pair in keyPairs.Keys)
                    {
                        if (pair.Section == sectionName.ToUpper())
                            tmpArray.Add(pair.Key);
                    }

                    return (String[])tmpArray.ToArray(typeof(String));
                }

                /// <summary>
                /// Adds or replaces a setting to the table to be saved.
                /// </summary>
                /// <param name="sectionName">Section to add under.</param>
                /// <param name="settingName">Key name to add.</param>
                /// <param name="settingValue">Value of key.</param>
                public void AddSetting(String sectionName, String settingName, String settingValue)
                {
                    SectionPair sectionPair;
                    sectionPair.Section = sectionName.ToUpper();
                    sectionPair.Key = settingName.ToUpper();

                    if (keyPairs.ContainsKey(sectionPair))
                        keyPairs.Remove(sectionPair);

                    keyPairs.Add(sectionPair, settingValue);
                }

                /// <summary>
                /// Adds or replaces a setting to the table to be saved with a null value.
                /// </summary>
                /// <param name="sectionName">Section to add under.</param>
                /// <param name="settingName">Key name to add.</param>
                public void AddSetting(String sectionName, String settingName)
                {
                    AddSetting(sectionName, settingName, null);
                }

                /// <summary>
                /// Remove a setting.
                /// </summary>
                /// <param name="sectionName">Section to add under.</param>
                /// <param name="settingName">Key name to add.</param>
                public void DeleteSetting(String sectionName, String settingName)
                {
                    SectionPair sectionPair;
                    sectionPair.Section = sectionName.ToUpper();
                    sectionPair.Key = settingName.ToUpper();

                    if (keyPairs.ContainsKey(sectionPair))
                        keyPairs.Remove(sectionPair);
                }

                /// <summary>
                /// Save settings to new file.
                /// </summary>
                /// <param name="newFilePath">New file path.</param>
                public void SaveSettings(String newFilePath)
                {
                    ArrayList sections = new ArrayList();
                    String tmpValue = "";
                    String strToSave = "";

                    foreach (SectionPair sectionPair in keyPairs.Keys)
                    {
                        if (!sections.Contains(sectionPair.Section))
                            sections.Add(sectionPair.Section);
                    }

                    foreach (String section in sections)
                    {
                        strToSave += ("[" + section + "]\r\n");

                        foreach (SectionPair sectionPair in keyPairs.Keys)
                        {
                            if (sectionPair.Section == section)
                            {
                                tmpValue = (String)keyPairs[sectionPair];

                                if (tmpValue != null)
                                    tmpValue = "=" + tmpValue;

                                strToSave += (sectionPair.Key + tmpValue + "\r\n");
                            }
                        }

                        strToSave += "\r\n";
                    }

                    try
                    {
                        TextWriter tw = new StreamWriter(newFilePath);
                        tw.Write(strToSave);
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                /// <summary>
                /// Save settings back to ini file.
                /// </summary>
                public void SaveSettings()
                {
                    SaveSettings(iniFilePath);
                }
            }
            public class CommandProcessor
            {
                private static string ParseAspas(ref string thing)
                {
                    var t = thing;
                    t = t.Substring(1);
                    t = t.Substring(0, t.IndexOf("\""));
                    if (thing.Replace(t, "") == "\"\"")
                        thing = "";
                    else
                        thing = thing.Substring(t.Length + 3);
                    return t;
                }
                private static string ParseNormal(ref string thing)
                {
                    var t = thing.Split(' ')[0];
                    if (thing.Contains(" "))
                        thing = thing.Substring(t.Length + 1);
                    else
                        thing = "";
                    return t;
                }
                public static string[] ParseArgs(string args)
                {
                    List<string> cmds = new List<string>();
                    string a = args;
                    while (a != "")
                    {
                        if (a.Substring(0, 1) == "\"")
                            cmds.Add(ParseAspas(ref a));
                        else
                            cmds.Add(ParseNormal(ref a));
                    }
                    return cmds.ToArray();
                }
            }
            public static class Format
            {
                public static string CSharp(string source)
                {
                    return VB(source);
                }
                public static string VB(string source)
                {
                    return JS(source);
                }
                public static string XML(string xml)
                {
                    var document = System.Xml.Linq.XDocument.Parse(xml);
                    return document.ToString();
                }
                public static string JSON(string json)
                {
                    return Data.SimpleJSON.JSON.Format(json);
                }
                public static string JSON(string json, string Indent)
                {
                    return Data.SimpleJSON.JSON.Format(json, Indent);
                }
                public static string JS(string source, char indent_char, int indent_size, int indent_level)
                {
                    return JSFormatter.FormatJS(source, indent_char, indent_size, indent_level);
                }
                public static string JS(string source)
                {
                    return Format.JS(source, ' ', 4);
                }
                public static string JS(string source, char indent_char)
                {
                    return JSFormatter.FormatJS(source, indent_char, 4, 0);
                }
                public static string JS(string source, char indent_char, int indent_size)
                {
                    return JSFormatter.FormatJS(source, indent_char, indent_size, 0);
                }
                public static string JS(string source, int indent_size)
                {
                    return JSFormatter.FormatJS(source, ' ', indent_size, 0);
                }
                public static string JS(string source, int indent_level, char indent_char = ' ')
                {
                    return JSFormatter.FormatJS(source, indent_char, 4, indent_level);
                }
                private static class JSFormatter
                {
                    private static string last_word;
                    private static bool var_line;
                    private static bool var_line_tainted;
                    private static string[] line_starters;
                    private static bool in_case;
                    private static string token_type;
                    private static StringBuilder output;
                    private static string indent_string;
                    private static int indent_level;
                    private static string token_text;
                    private static Stack<string> modes;
                    private static string current_mode;
                    private static int opt_indent_size;
                    private static char opt_indent_char;
                    private static bool opt_preserve_newlines;
                    private static bool if_line_flag;
                    private static bool do_block_just_closed;
                    private static string input;

                    private static void Trim_output()
                    {
                        while ((output.Length > 0) && ((output[output.Length - 1] == ' ') || (output[output.Length - 1].ToString() == indent_string)))
                        {
                            output.Remove(output.Length - 1, 1);
                        }
                    }

                    private static void Print_newline(bool? ignore_repeated)
                    {
                        ignore_repeated = ignore_repeated ?? true;

                        if_line_flag = false;
                        Trim_output();

                        if (output.Length == 0)
                            return;

                        if ((output[output.Length - 1] != '\n') || !ignore_repeated.Value)
                        {
                            output.Append(Environment.NewLine);
                        }

                        for (var i = 0; i < indent_level; i++)
                        {
                            output.Append(indent_string);
                        }
                    }

                    private static void Print_space()
                    {
                        var last_output = " ";
                        if (output.Length > 0)
                            last_output = output[output.Length - 1].ToString();
                        if ((last_output != " ") && (last_output != "\n") && (last_output != indent_string))
                        {
                            output.Append(' ');
                        }
                    }


                    private static void Print_token()
                    {
                        output.Append(token_text);
                    }

                    private static void Indent()
                    {
                        indent_level++;
                    }

                    private static void Unindent()
                    {
                        if (indent_level > 0)
                            indent_level--;
                    }

                    private static void Remove_indent()
                    {
                        if ((output.Length > 0) && (output[output.Length - 1].ToString() == indent_string))
                        {
                            output.Remove(output.Length - 1, 1);
                        }
                    }

                    private static void Set_mode(string mode)
                    {
                        modes.Push(current_mode);
                        current_mode = mode;
                    }

                    private static void Restore_mode()
                    {
                        do_block_just_closed = (current_mode == "DO_BLOCK");
                        current_mode = modes.Pop();
                    }

                    private static bool Is_ternary_op()
                    {
                        int level = 0;
                        int colon_count = 0;
                        for (var i = output.Length - 1; i >= 0; i--)
                        {
                            switch (output[i])
                            {
                                case ':':
                                    if (level == 0)
                                        colon_count++;
                                    break;
                                case '?':
                                    if (level == 0)
                                    {
                                        if (colon_count == 0)
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            colon_count--;
                                        }
                                    }
                                    break;
                                case '{':
                                    if (level == 0) return false;
                                    level--;
                                    break;
                                case '(':
                                case '[':
                                    level--;
                                    break;
                                case ')':
                                case ']':
                                case '}':
                                    level++;
                                    break;
                            }
                        }
                        return false;
                    }

                    private static string whitespace;
                    private static string wordchar;
                    private static int parser_pos;
                    private static string last_type;
                    private static string last_text;
                    private static string digits;
                    private static string[] punct;
                    private static string prefix;

                    public static int Opt_indent_level { get; set; }

                    private static string[] Get_next_token(ref int parser_pos)
                    {
                        var n_newlines = 0;

                        if (parser_pos >= input.Length)
                        {
                            return new string[] { "", "TK_EOF" };
                        }

                        string c = input[parser_pos].ToString();
                        parser_pos++;

                        while (whitespace.Contains(c))
                        {
                            if (parser_pos >= input.Length)
                            {
                                return new string[] { "", "TK_EOF" };
                            }

                            if (c == "\n")
                                n_newlines++;

                            c = input[parser_pos].ToString();
                            parser_pos++;
                        }

                        var wanted_newline = false;

                        if (opt_preserve_newlines)
                        {
                            if (n_newlines > 1)
                            {
                                for (var i = 0; i < 2; i++)
                                {
                                    Print_newline(i == 0);
                                }
                            }
                            wanted_newline = (n_newlines == 1);

                        }

                        if (wordchar.Contains(c))
                        {
                            if (parser_pos < input.Length)
                            {
                                while (wordchar.Contains(input[parser_pos]))
                                {
                                    c += input[parser_pos];
                                    parser_pos++;
                                    if (parser_pos == input.Length)
                                        break;
                                }
                            }


                            if ((parser_pos != input.Length) && (System.Text.RegularExpressions.Regex.IsMatch(c, "^[0-9]+[Ee]$")) && ((input[parser_pos] == '-') || (input[parser_pos] == '+')))
                            {
                                var sign = input[parser_pos];
                                parser_pos++;

                                var t = Get_next_token(ref parser_pos);
                                c += sign + t[0];
                                return new string[] { c, "TK_WORD" };
                            }

                            if (c == "in")
                            {
                                return new string[] { c, "TK_OPERATOR" };
                            }

                            if (wanted_newline && last_type != "TK_OPERATOR" && !if_line_flag)
                            {
                                Print_newline(null);
                            }
                            return new string[] { c, "TK_WORD" };

                        }

                        if ((c == "(") || (c == "["))
                            return new string[] { c, "TK_START_EXPR" };

                        if (c == ")" || c == "]")
                        {
                            return new string[] { c, "TK_END_EXPR" };
                        }

                        if (c == "{")
                        {
                            return new string[] { c, "TK_START_BLOCK" };
                        }

                        if (c == "}")
                        {
                            return new string[] { c, "TK_END_BLOCK" };
                        }

                        if (c == ";")
                        {
                            return new string[] { c, "TK_SEMICOLON" };
                        }

                        if (c == "/")
                        {
                            var comment = "";
                            if (input[parser_pos] == '*')
                            {
                                parser_pos++;
                                if (parser_pos < input.Length)
                                {
                                    while (!((input[parser_pos] == '*') && (input[parser_pos + 1] > '\0') && (input[parser_pos + 1] == '/') && (parser_pos < input.Length)))
                                    {
                                        comment += input[parser_pos];
                                        parser_pos++;
                                        if (parser_pos >= input.Length)
                                        {
                                            break;
                                        }
                                    }
                                }

                                parser_pos += 2;
                                return new string[] { "/*" + comment + "*/", "TK_BLOCK_COMMENT" };
                            }

                            if (input[parser_pos] == '/')
                            {
                                comment = c;
                                while ((input[parser_pos] != '\x0d') && (input[parser_pos] != '\x0a'))
                                {
                                    comment += input[parser_pos];
                                    parser_pos++;
                                    if (parser_pos >= input.Length)
                                    {
                                        break;
                                    }
                                }

                                parser_pos++;
                                if (wanted_newline)
                                {
                                    Print_newline(null);
                                }
                                return new string[] { comment, "TK_COMMENT" };

                            }
                        }

                        if ((c == "'") || (c == "\"") || ((c == "/")
                                && ((last_type == "TK_WORD" && last_text == "return") || ((last_type == "TK_START_EXPR") || (last_type == "TK_START_BLOCK") || (last_type == "TK_END_BLOCK")
                                        || (last_type == "TK_OPERATOR") || (last_type == "TK_EOF") || (last_type == "TK_SEMICOLON"))))
                            )
                        {
                            var sep = c;
                            var esc = false;
                            var resulting_string = c;

                            if (parser_pos < input.Length)
                            {
                                if (sep == "/")
                                {
                                    var in_char_class = false;
                                    while ((esc) || (in_char_class) || (input[parser_pos].ToString() != sep))
                                    {
                                        resulting_string += input[parser_pos];
                                        if (!esc)
                                        {
                                            esc = input[parser_pos] == '\\';
                                            if (input[parser_pos] == '[')
                                            {
                                                in_char_class = true;
                                            }
                                            else if (input[parser_pos] == ']')
                                            {
                                                in_char_class = false;
                                            }
                                        }
                                        else
                                        {
                                            esc = false;
                                        }
                                        parser_pos++;
                                        if (parser_pos >= input.Length)
                                        {
                                            return new string[] { resulting_string, "TK_STRING" };
                                        }
                                    }
                                }
                                else
                                {
                                    while ((esc) || (input[parser_pos].ToString() != sep))
                                    {
                                        resulting_string += input[parser_pos];
                                        if (!esc)
                                        {
                                            esc = input[parser_pos] == '\\';
                                        }
                                        else
                                        {
                                            esc = false;
                                        }
                                        parser_pos++;
                                        if (parser_pos >= input.Length)
                                        {
                                            return new string[] { resulting_string, "TK_STRING" };
                                        }
                                    }
                                }
                            }

                            parser_pos += 1;

                            resulting_string += sep;

                            if (sep == "/")
                            {
                                // regexps may have modifiers /regexp/MOD , so fetch those, too
                                while ((parser_pos < input.Length) && (wordchar.Contains(input[parser_pos])))
                                {
                                    resulting_string += input[parser_pos];
                                    parser_pos += 1;
                                }
                            }
                            return new string[] { resulting_string, "TK_STRING" };


                        }

                        if (c == "#")
                        {
                            var sharp = "#";
                            if ((parser_pos < input.Length) && (digits.Contains(input[parser_pos])))
                            {
                                do
                                {
                                    c = input[parser_pos].ToString();
                                    sharp += c;
                                    parser_pos += 1;
                                } while ((parser_pos < input.Length) && (c != "#") && (c != "="));
                                if (c == "#")
                                {
                                    return new string[] { sharp, "TK_WORD" }; ;
                                }
                                else
                                {
                                    return new string[] { sharp, "TK_OPERATOR" }; ;
                                }
                            }
                        }


                        if ((c == "<") && (input.Substring(parser_pos - 1, 3) == "<!--"))
                        {
                            parser_pos += 3;
                            return new string[] { "<!--", "TK_COMMENT" }; ;
                        }

                        if ((c == "-") && (input.Substring(parser_pos - 1, 2) == "-->"))
                        {
                            parser_pos += 2;
                            if (wanted_newline)
                            {
                                Print_newline(null);
                            }
                            return new string[] { "-->", "TK_COMMENT" };
                        }

                        if (punct.Contains(c))
                        {
                            while ((parser_pos < input.Length) && (punct.Contains(c + input[parser_pos])))
                            {
                                c += input[parser_pos];
                                parser_pos += 1;
                                if (parser_pos >= input.Length)
                                {
                                    break;
                                }
                            }

                            return new string[] { c, "TK_OPERATOR" };
                        }

                        return new string[] { c, "TK_UNKNOWN" };


                    }
                    public static string FormatJS(string js_source_text, char indent_char, int indent_size, int indent_level)
                    {
                        bool add_script_tags = false;
                        opt_indent_size = indent_size;
                        opt_indent_char = indent_char;
                        Opt_indent_level = indent_level;
                        opt_preserve_newlines = true;
                        output = new StringBuilder();
                        modes = new Stack<string>();



                        indent_string = "";

                        while (opt_indent_size > 0)
                        {
                            indent_string += opt_indent_char;
                            opt_indent_size -= 1;
                        }

                        input = js_source_text.Replace("<script type=\"text/javascript\">", "").Replace("</script>", "");
                        if (input.Length != js_source_text.Length)
                        {
                            output.AppendLine("<script type=\"text/javascript\">");
                            add_script_tags = true;
                        }

                        last_word = ""; // last 'TK_WORD' passed
                        last_type = "TK_START_EXPR"; // last token type
                        last_text = ""; // last token text

                        do_block_just_closed = false;
                        var_line = false;         // currently drawing var .... ;
                        var_line_tainted = false; // false: var a = 5; true: var a = 5, b = 6

                        whitespace = "\n\r\t ";
                        wordchar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$";
                        digits = "0123456789";

                        // <!-- is a special case (ok, it's a minor hack actually)
                        punct = "+ - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ! !! , : ? ^ ^= |= ::".Split(' ');

                        // words which should always start on new line.
                        line_starters = "continue,try,throw,return,var,if,switch,case,default,for,while,break,function".Split(',');

                        // states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.
                        // some formatting depends on that.
                        current_mode = "BLOCK";
                        modes.Push(current_mode);

                        parser_pos = 0;
                        in_case = false;

                        while (true)
                        {
                            var t = Get_next_token(ref parser_pos);
                            token_text = t[0];
                            token_type = t[1];
                            if (token_type == "TK_EOF")
                            {
                                break;
                            }

                            switch (token_type)
                            {

                                case "TK_START_EXPR":
                                    var_line = false;
                                    Set_mode("EXPRESSION");
                                    if ((last_text == ";") || (last_type == "TK_START_BLOCK"))
                                    {
                                        Print_newline(null);
                                    }
                                    else if ((last_type == "TK_END_EXPR") || (last_type == "TK_START_EXPR"))
                                    {
                                        // do nothing on (( and )( and ][ and ]( ..
                                    }
                                    else if ((last_type != "TK_WORD") && (last_type != "TK_OPERATOR"))
                                    {
                                        Print_space();
                                    }
                                    else if (line_starters.Contains(last_word))
                                    {
                                        Print_space();
                                    }
                                    Print_token();
                                    break;

                                case "TK_END_EXPR":
                                    Print_token();
                                    Restore_mode();
                                    break;

                                case "TK_START_BLOCK":

                                    if (last_word == "do")
                                    {
                                        Set_mode("DO_BLOCK");
                                    }
                                    else
                                    {
                                        Set_mode("BLOCK");
                                    }
                                    if ((last_type != "TK_OPERATOR") && (last_type != "TK_START_EXPR"))
                                    {
                                        if (last_type == "TK_START_BLOCK")
                                        {
                                            Print_newline(null);
                                        }
                                        else
                                        {
                                            Print_space();
                                        }
                                    }
                                    Print_token();
                                    Indent();
                                    break;

                                case "TK_END_BLOCK":
                                    if (last_type == "TK_START_BLOCK")
                                    {
                                        // nothing
                                        Trim_output();
                                        Unindent();
                                    }
                                    else
                                    {
                                        Unindent();
                                        Print_newline(null);
                                    }
                                    Print_token();
                                    Restore_mode();
                                    break;

                                case "TK_WORD":

                                    if (do_block_just_closed)
                                    {
                                        // do {} ## while ()
                                        Print_space();
                                        Print_token();
                                        Print_space();
                                        do_block_just_closed = false;
                                        break;
                                    }

                                    if ((token_text == "case") || (token_text == "default"))
                                    {
                                        if (last_text == ":")
                                        {
                                            // switch cases following one another
                                            Remove_indent();
                                        }
                                        else
                                        {
                                            // case statement starts in the same line where switch
                                            Unindent();
                                            Print_newline(null);
                                            Indent();
                                        }
                                        Print_token();
                                        in_case = true;
                                        break;
                                    }

                                    prefix = "NONE";

                                    if (last_type == "TK_END_BLOCK")
                                    {
                                        if (!(new string[] { "else", "catch", "finally" }).Contains(token_text.ToLower()))
                                        {
                                            prefix = "NEWLINE";
                                        }
                                        else
                                        {
                                            prefix = "SPACE";
                                            Print_space();
                                        }
                                    }
                                    else if ((last_type == "TK_SEMICOLON") && ((current_mode == "BLOCK") || (current_mode == "DO_BLOCK")))
                                    {
                                        prefix = "NEWLINE";
                                    }
                                    else if ((last_type == "TK_SEMICOLON") && (current_mode == "EXPRESSION"))
                                    {
                                        prefix = "SPACE";
                                    }
                                    else if (last_type == "TK_STRING")
                                    {
                                        prefix = "NEWLINE";
                                    }
                                    else if (last_type == "TK_WORD")
                                    {
                                        prefix = "SPACE";
                                    }
                                    else if (last_type == "TK_START_BLOCK")
                                    {
                                        prefix = "NEWLINE";
                                    }
                                    else if (last_type == "TK_END_EXPR")
                                    {
                                        Print_space();
                                        prefix = "NEWLINE";
                                    }

                                    if ((last_type != "TK_END_BLOCK") && ((new string[] { "else", "catch", "finally" }).Contains(token_text.ToLower())))
                                    {
                                        Print_newline(null);
                                    }
                                    else if ((line_starters.Contains(token_text)) || (prefix == "NEWLINE"))
                                    {
                                        if (last_text == "else")
                                        {
                                            // no need to force newline on else break
                                            Print_space();
                                        }
                                        else if (((last_type == "TK_START_EXPR") || (last_text == "=") || (last_text == ",")) && (token_text == "function"))
                                        {
                                            // no need to force newline on "function": (function
                                            // DONOTHING
                                        }
                                        else if ((last_type == "TK_WORD") && ((last_text == "return") || (last_text == "throw")))
                                        {
                                            // no newline between "return nnn"
                                            Print_space();
                                        }
                                        else if (last_type != "TK_END_EXPR")
                                        {
                                            if (((last_type != "TK_START_EXPR") || (token_text != "var")) && (last_text != ":"))
                                            {
                                                // no need to force newline on "var": for (var x = 0...)
                                                if ((token_text == "if") && (last_type == "TK_WORD") && (last_word == "else"))
                                                {
                                                    // no newline for } else if {
                                                    Print_space();
                                                }
                                                else
                                                {
                                                    Print_newline(null);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if ((line_starters.Contains(token_text)) && (last_text != ")"))
                                            {
                                                Print_newline(null);
                                            }
                                        }
                                    }
                                    else if (prefix == "SPACE")
                                    {
                                        Print_space();
                                    }
                                    Print_token();
                                    last_word = token_text;

                                    if (token_text == "var")
                                    {
                                        var_line = true;
                                        var_line_tainted = false;
                                    }

                                    if (token_text == "if" || token_text == "else")
                                    {
                                        if_line_flag = true;
                                    }

                                    break;

                                case "TK_SEMICOLON":

                                    Print_token();
                                    var_line = false;
                                    break;

                                case "TK_STRING":

                                    if ((last_type == "TK_START_BLOCK") || (last_type == "TK_END_BLOCK") || (last_type == "TK_SEMICOLON"))
                                    {
                                        Print_newline(null);
                                    }
                                    else if (last_type == "TK_WORD")
                                    {
                                        Print_space();
                                    }
                                    Print_token();
                                    break;

                                case "TK_OPERATOR":

                                    var start_delim = true;
                                    var end_delim = true;
                                    if (var_line && (token_text != ","))
                                    {
                                        var_line_tainted = true;
                                        if (token_text == ":")
                                        {
                                            var_line = false;
                                        }
                                    }
                                    if (var_line && (token_text == ",") && (current_mode == "EXPRESSION"))
                                    {
                                        // do not break on comma, for(var a = 1, b = 2)
                                        var_line_tainted = false;
                                    }

                                    if (token_text == ":" && in_case)
                                    {
                                        Print_token(); // colon really asks for separate treatment
                                        Print_newline(null);
                                        in_case = false;
                                        break;
                                    }

                                    if (token_text == "::")
                                    {
                                        // no spaces around exotic namespacing syntax operator
                                        Print_token();
                                        break;
                                    }

                                    if (token_text == ",")
                                    {
                                        if (var_line)
                                        {
                                            if (var_line_tainted)
                                            {
                                                Print_token();
                                                Print_newline(null);
                                                var_line_tainted = false;
                                            }
                                            else
                                            {
                                                Print_token();
                                                Print_space();
                                            }
                                        }
                                        else if (last_type == "TK_END_BLOCK")
                                        {
                                            Print_token();
                                            Print_newline(null);
                                        }
                                        else
                                        {
                                            if (current_mode == "BLOCK")
                                            {
                                                Print_token();
                                                Print_newline(null);
                                            }
                                            else
                                            {
                                                // EXPR od DO_BLOCK
                                                Print_token();
                                                Print_space();
                                            }
                                        }
                                        break;
                                    }
                                    else if ((token_text == "--") || (token_text == "++"))
                                    { // unary operators special case
                                        if (last_text == ";")
                                        {
                                            if (current_mode == "BLOCK")
                                            {
                                                // { foo; --i }
                                                Print_newline(null);
                                                start_delim = true;
                                                end_delim = false;
                                            }
                                            else
                                            {
                                                // space for (;; ++i)
                                                start_delim = true;
                                                end_delim = false;
                                            }
                                        }
                                        else
                                        {
                                            if (last_text == "{")
                                            {
                                                // {--i
                                                Print_newline(null);
                                            }
                                            start_delim = false;
                                            end_delim = false;
                                        }
                                    }
                                    else if (((token_text == "!") || (token_text == "+") || (token_text == "-")) && ((last_text == "return") || (last_text == "case")))
                                    {
                                        start_delim = true;
                                        end_delim = false;
                                    }
                                    else if (((token_text == "!") || (token_text == "+") || (token_text == "-")) && (last_type == "TK_START_EXPR"))
                                    {
                                        // special case handling: if (!a)
                                        start_delim = false;
                                        end_delim = false;
                                    }
                                    else if (last_type == "TK_OPERATOR")
                                    {
                                        start_delim = false;
                                        end_delim = false;
                                    }
                                    else if (last_type == "TK_END_EXPR")
                                    {
                                        start_delim = true;
                                        end_delim = true;
                                    }
                                    else if (token_text == ".")
                                    {
                                        // decimal digits or object.property
                                        start_delim = false;
                                        end_delim = false;

                                    }
                                    else if (token_text == ":")
                                    {
                                        if (Is_ternary_op())
                                        {
                                            start_delim = true;
                                        }
                                        else
                                        {
                                            start_delim = false;
                                        }
                                    }
                                    if (start_delim)
                                    {
                                        Print_space();
                                    }

                                    Print_token();

                                    if (end_delim)
                                    {
                                        Print_space();
                                    }
                                    break;

                                case "TK_BLOCK_COMMENT":

                                    Print_newline(null);
                                    Print_token();
                                    Print_newline(null);
                                    break;

                                case "TK_COMMENT":

                                    // print_newline();
                                    Print_space();
                                    Print_token();
                                    Print_newline(null);
                                    break;

                                case "TK_UNKNOWN":
                                    Print_token();
                                    break;
                            }

                            last_type = token_type;
                            last_text = token_text;
                        }
                        if (add_script_tags)
                        {
                            output.AppendLine().AppendLine("</script>");
                        }
                        return output.ToString();
                    }
                }
            }
            namespace SimpleJSON
            {
                public enum JSONTextMode
                {
                    // Token: 0x040008AD RID: 2221
                    Compact,
                    // Token: 0x040008AE RID: 2222
                    Indent
                }
                // Token: 0x02000131 RID: 305
                public class JSONString : JSONNode
                {
                    // Token: 0x060009F5 RID: 2549 RVA: 0x00042E7B File Offset: 0x0004127B
                    public JSONString(string aData)
                    {
                        this.m_Data = aData;
                    }

                    // Token: 0x1700015A RID: 346
                    // (get) Token: 0x060009F6 RID: 2550 RVA: 0x00042E8A File Offset: 0x0004128A
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.String;
                        }
                    }

                    // Token: 0x1700015B RID: 347
                    // (get) Token: 0x060009F7 RID: 2551 RVA: 0x00042E8D File Offset: 0x0004128D
                    public override bool IsString
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x1700015C RID: 348
                    // (get) Token: 0x060009F8 RID: 2552 RVA: 0x00042E90 File Offset: 0x00041290
                    // (set) Token: 0x060009F9 RID: 2553 RVA: 0x00042E98 File Offset: 0x00041298
                    public override string Value
                    {
                        get
                        {
                            return this.m_Data;
                        }
                        set
                        {
                            this.m_Data = value;
                        }
                    }

                    // Token: 0x060009FA RID: 2554 RVA: 0x00042EA1 File Offset: 0x000412A1
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(3);
                        aWriter.Write(this.m_Data);
                    }

                    // Token: 0x060009FB RID: 2555 RVA: 0x00042EB6 File Offset: 0x000412B6
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append('"').Append(JSONNode.Escape(this.m_Data)).Append('"');
                    }

                    // Token: 0x060009FC RID: 2556 RVA: 0x00042ED8 File Offset: 0x000412D8
                    public override bool Equals(object obj)
                    {
                        if (base.Equals(obj))
                        {
                            return true;
                        }
                        string text = obj as string;
                        if (text != null)
                        {
                            return this.m_Data == text;
                        }
                        JSONString jsonstring = obj as JSONString;
                        return jsonstring != null && this.m_Data == jsonstring.m_Data;
                    }

                    // Token: 0x060009FD RID: 2557 RVA: 0x00042F33 File Offset: 0x00041333
                    public override int GetHashCode()
                    {
                        return this.m_Data.GetHashCode();
                    }

                    // Token: 0x040008B4 RID: 2228
                    private string m_Data;
                }
                // Token: 0x02000130 RID: 304
                public class JSONObject : JSONNode, IEnumerable
                {
                    // Token: 0x17000154 RID: 340
                    // (get) Token: 0x060009E6 RID: 2534 RVA: 0x000426FA File Offset: 0x00040AFA
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.Object;
                        }
                    }

                    // Token: 0x17000155 RID: 341
                    // (get) Token: 0x060009E7 RID: 2535 RVA: 0x000426FD File Offset: 0x00040AFD
                    public override bool IsObject
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x17000156 RID: 342
                    public override JSONNode this[string aKey]
                    {
                        get
                        {
                            if (this.m_Dict.ContainsKey(aKey))
                            {
                                return this.m_Dict[aKey];
                            }
                            return new JSONLazyCreator(this, aKey);
                        }
                        set
                        {
                            if (value == null)
                            {
                                value = new JSONNull();
                            }
                            if (this.m_Dict.ContainsKey(aKey))
                            {
                                this.m_Dict[aKey] = value;
                            }
                            else
                            {
                                this.m_Dict.Add(aKey, value);
                            }
                        }
                    }

                    // Token: 0x17000157 RID: 343
                    public override JSONNode this[int aIndex]
                    {
                        get
                        {
                            if (aIndex < 0 || aIndex >= this.m_Dict.Count)
                            {
                                return null;
                            }
                            return this.m_Dict.ElementAt(aIndex).Value;
                        }
                        set
                        {
                            if (value == null)
                            {
                                value = new JSONNull();
                            }
                            if (aIndex < 0 || aIndex >= this.m_Dict.Count)
                            {
                                return;
                            }
                            string key = this.m_Dict.ElementAt(aIndex).Key;
                            this.m_Dict[key] = value;
                        }
                    }

                    // Token: 0x17000158 RID: 344
                    // (get) Token: 0x060009EC RID: 2540 RVA: 0x0004280F File Offset: 0x00040C0F
                    public override int Count
                    {
                        get
                        {
                            return this.m_Dict.Count;
                        }
                    }

                    // Token: 0x060009ED RID: 2541 RVA: 0x0004281C File Offset: 0x00040C1C
                    public override void Add(string aKey, JSONNode aItem)
                    {
                        if (aItem == null)
                        {
                            aItem = new JSONNull();
                        }
                        if (!string.IsNullOrEmpty(aKey))
                        {
                            if (this.m_Dict.ContainsKey(aKey))
                            {
                                this.m_Dict[aKey] = aItem;
                            }
                            else
                            {
                                this.m_Dict.Add(aKey, aItem);
                            }
                        }
                        else
                        {
                            this.m_Dict.Add(Guid.NewGuid().ToString(), aItem);
                        }
                    }

                    // Token: 0x060009EE RID: 2542 RVA: 0x0004289C File Offset: 0x00040C9C
                    public override JSONNode Remove(string aKey)
                    {
                        if (!this.m_Dict.ContainsKey(aKey))
                        {
                            return null;
                        }
                        JSONNode result = this.m_Dict[aKey];
                        this.m_Dict.Remove(aKey);
                        return result;
                    }

                    // Token: 0x060009EF RID: 2543 RVA: 0x000428D8 File Offset: 0x00040CD8
                    public override JSONNode Remove(int aIndex)
                    {
                        if (aIndex < 0 || aIndex >= this.m_Dict.Count)
                        {
                            return null;
                        }
                        KeyValuePair<string, JSONNode> keyValuePair = this.m_Dict.ElementAt(aIndex);
                        this.m_Dict.Remove(keyValuePair.Key);
                        return keyValuePair.Value;
                    }

                    // Token: 0x060009F0 RID: 2544 RVA: 0x00042928 File Offset: 0x00040D28
                    public override JSONNode Remove(JSONNode aNode)
                    {
                        JSONNode result;
                        try
                        {
                            KeyValuePair<string, JSONNode> keyValuePair = (from k in this.m_Dict
                                                                           where k.Value == aNode
                                                                           select k).First<KeyValuePair<string, JSONNode>>();
                            this.m_Dict.Remove(keyValuePair.Key);
                            result = aNode;
                        }
                        catch
                        {
                            result = null;
                        }
                        return result;
                    }

                    // Token: 0x17000159 RID: 345
                    // (get) Token: 0x060009F1 RID: 2545 RVA: 0x00042998 File Offset: 0x00040D98
                    public override IEnumerable<JSONNode> Children
                    {
                        get
                        {
                            foreach (KeyValuePair<string, JSONNode> N in this.m_Dict)
                            {
                                yield return N.Value;
                            }
                            yield break;
                        }
                    }

                    // Token: 0x060009F2 RID: 2546 RVA: 0x000429BC File Offset: 0x00040DBC
                    public IEnumerator GetEnumerator()
                    {
                        foreach (KeyValuePair<string, JSONNode> N in this.m_Dict)
                        {
                            yield return N;
                        }
                        yield break;
                    }

                    // Token: 0x060009F3 RID: 2547 RVA: 0x000429D8 File Offset: 0x00040DD8
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(2);
                        aWriter.Write(this.m_Dict.Count);
                        foreach (string text in this.m_Dict.Keys)
                        {
                            aWriter.Write(text);
                            this.m_Dict[text].Serialize(aWriter);
                        }
                    }

                    // Token: 0x060009F4 RID: 2548 RVA: 0x00042A64 File Offset: 0x00040E64
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append('{');
                        bool flag = true;
                        if (this.inline)
                        {
                            aMode = JSONTextMode.Compact;
                        }
                        foreach (KeyValuePair<string, JSONNode> keyValuePair in this.m_Dict)
                        {
                            if (!flag)
                            {
                                aSB.Append(',');
                            }
                            flag = false;
                            if (aMode == JSONTextMode.Indent)
                            {
                                aSB.AppendLine();
                            }
                            if (aMode == JSONTextMode.Indent)
                            {
                                aSB.Append(' ', aIndent + aIndentInc);
                            }
                            aSB.Append('"').Append(JSONNode.Escape(keyValuePair.Key)).Append('"');
                            if (aMode == JSONTextMode.Compact)
                            {
                                aSB.Append(':');
                            }
                            else
                            {
                                aSB.Append(" : ");
                            }
                            keyValuePair.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
                        }
                        if (aMode == JSONTextMode.Indent)
                        {
                            aSB.AppendLine().Append(' ', aIndent);
                        }
                        aSB.Append('}');
                    }

                    // Token: 0x040008B2 RID: 2226
                    private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

                    // Token: 0x040008B3 RID: 2227
                    public bool inline;
                }
                // Token: 0x02000132 RID: 306
                public class JSONNumber : JSONNode
                {
                    // Token: 0x060009FE RID: 2558 RVA: 0x00042F40 File Offset: 0x00041340
                    public JSONNumber(double aData)
                    {
                        this.m_Data = aData;
                    }

                    // Token: 0x060009FF RID: 2559 RVA: 0x00042F4F File Offset: 0x0004134F
                    public JSONNumber(string aData)
                    {
                        this.Value = aData;
                    }

                    // Token: 0x1700015D RID: 349
                    // (get) Token: 0x06000A00 RID: 2560 RVA: 0x00042F5E File Offset: 0x0004135E
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.Number;
                        }
                    }

                    // Token: 0x1700015E RID: 350
                    // (get) Token: 0x06000A01 RID: 2561 RVA: 0x00042F61 File Offset: 0x00041361
                    public override bool IsNumber
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x1700015F RID: 351
                    // (get) Token: 0x06000A02 RID: 2562 RVA: 0x00042F64 File Offset: 0x00041364
                    // (set) Token: 0x06000A03 RID: 2563 RVA: 0x00042F78 File Offset: 0x00041378
                    public override string Value
                    {
                        get
                        {
                            return this.m_Data.ToString();
                        }
                        set
                        {
                            double data;
                            if (double.TryParse(value, out data))
                            {
                                this.m_Data = data;
                            }
                        }
                    }

                    // Token: 0x17000160 RID: 352
                    // (get) Token: 0x06000A04 RID: 2564 RVA: 0x00042F99 File Offset: 0x00041399
                    // (set) Token: 0x06000A05 RID: 2565 RVA: 0x00042FA1 File Offset: 0x000413A1
                    public override double AsDouble
                    {
                        get
                        {
                            return this.m_Data;
                        }
                        set
                        {
                            this.m_Data = value;
                        }
                    }

                    // Token: 0x06000A06 RID: 2566 RVA: 0x00042FAA File Offset: 0x000413AA
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(4);
                        aWriter.Write(this.m_Data);
                    }

                    // Token: 0x06000A07 RID: 2567 RVA: 0x00042FBF File Offset: 0x000413BF
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append(this.m_Data);
                    }

                    // Token: 0x06000A08 RID: 2568 RVA: 0x00042FD0 File Offset: 0x000413D0
                    private static bool IsNumeric(object value)
                    {
                        return value is int || value is uint || value is float || value is double || value is decimal || value is long || value is ulong || value is short || value is ushort || value is sbyte || value is byte;
                    }

                    // Token: 0x06000A09 RID: 2569 RVA: 0x00043058 File Offset: 0x00041458
                    public override bool Equals(object obj)
                    {
                        if (obj == null)
                        {
                            return false;
                        }
                        if (base.Equals(obj))
                        {
                            return true;
                        }
                        JSONNumber jsonnumber = obj as JSONNumber;
                        if (jsonnumber != null)
                        {
                            return this.m_Data == jsonnumber.m_Data;
                        }
                        return JSONNumber.IsNumeric(obj) && Convert.ToDouble(obj) == this.m_Data;
                    }

                    // Token: 0x06000A0A RID: 2570 RVA: 0x000430B8 File Offset: 0x000414B8
                    public override int GetHashCode()
                    {
                        return this.m_Data.GetHashCode();
                    }

                    // Token: 0x040008B5 RID: 2229
                    private double m_Data;
                }
                // Token: 0x02000134 RID: 308
                public class JSONNull : JSONNode
                {
                    // Token: 0x17000165 RID: 357
                    // (get) Token: 0x06000A18 RID: 2584 RVA: 0x000431AE File Offset: 0x000415AE
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.NullValue;
                        }
                    }

                    // Token: 0x17000166 RID: 358
                    // (get) Token: 0x06000A19 RID: 2585 RVA: 0x000431B1 File Offset: 0x000415B1
                    public override bool IsNull
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x17000167 RID: 359
                    // (get) Token: 0x06000A1A RID: 2586 RVA: 0x000431B4 File Offset: 0x000415B4
                    // (set) Token: 0x06000A1B RID: 2587 RVA: 0x000431BB File Offset: 0x000415BB
                    public override string Value
                    {
                        get
                        {
                            return "null";
                        }
                        set
                        {
                        }
                    }

                    // Token: 0x17000168 RID: 360
                    // (get) Token: 0x06000A1C RID: 2588 RVA: 0x000431BD File Offset: 0x000415BD
                    // (set) Token: 0x06000A1D RID: 2589 RVA: 0x000431C0 File Offset: 0x000415C0
                    public override bool AsBool
                    {
                        get
                        {
                            return false;
                        }
                        set
                        {
                        }
                    }

                    // Token: 0x06000A1E RID: 2590 RVA: 0x000431C2 File Offset: 0x000415C2
                    public override bool Equals(object obj)
                    {
                        return object.ReferenceEquals(this, obj) || obj is JSONNull;
                    }

                    // Token: 0x06000A1F RID: 2591 RVA: 0x000431DB File Offset: 0x000415DB
                    public override int GetHashCode()
                    {
                        return 0;
                    }

                    // Token: 0x06000A20 RID: 2592 RVA: 0x000431DE File Offset: 0x000415DE
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(5);
                    }

                    // Token: 0x06000A21 RID: 2593 RVA: 0x000431E7 File Offset: 0x000415E7
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append("null");
                    }
                }
                public enum JSONNodeType
                {
                    // Token: 0x040008A5 RID: 2213
                    Array = 1,
                    // Token: 0x040008A6 RID: 2214
                    Object,
                    // Token: 0x040008A7 RID: 2215
                    String,
                    // Token: 0x040008A8 RID: 2216
                    Number,
                    // Token: 0x040008A9 RID: 2217
                    NullValue,
                    // Token: 0x040008AA RID: 2218
                    Boolean,
                    // Token: 0x040008AB RID: 2219
                    None
                }
                public static class JSONNodeExtension
                {
                    public static JSONNode Locate(this JSONNode node, string path)
                    {
                        var n = node;
                        foreach (var v in path.Split('/'))
                            try { n = n[int.Parse(v)]; }
                            catch { n = n[v]; }
                        return n;
                    }
                    public static string Formatted(this JSONNode node)
                    {
                        return JSON.Format(node.ToString());
                    }
                }
                // Token: 0x0200012E RID: 302
                public abstract class JSONNode
                {
                    // Token: 0x1700013B RID: 315
                    public virtual JSONNode this[int aIndex]
                    {
                        get
                        {
                            return null;
                        }
                        set
                        {
                        }
                    }

                    // Token: 0x1700013C RID: 316
                    public virtual JSONNode this[string aKey]
                    {
                        get
                        {
                            return null;
                        }
                        set
                        {
                        }
                    }
                    public virtual string Value
                    {
                        get
                        {
                            return string.Empty;
                        }
                        set
                        {
                        }
                    }
                    public virtual int Count
                    {
                        get
                        {
                            return 0;
                        }
                    }
                    public virtual bool IsNumber
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual bool IsString
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual bool IsBoolean
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual bool IsNull
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual bool IsArray
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual bool IsObject
                    {
                        get
                        {
                            return false;
                        }
                    }
                    public virtual void Add(string aKey, JSONNode aItem)
                    {
                    }
                    public virtual void Add(JSONNode aItem)
                    {
                        this.Add(string.Empty, aItem);
                    }
                    public virtual JSONNode Remove(string aKey)
                    {
                        return null;
                    }
                    public virtual JSONNode Remove(int aIndex)
                    {
                        return null;
                    }
                    public virtual JSONNode Remove(JSONNode aNode)
                    {
                        return aNode;
                    }
                    public virtual IEnumerable<JSONNode> Children
                    {
                        get
                        {
                            yield break;
                        }
                    }
                    public IEnumerable<JSONNode> DeepChildren
                    {
                        get
                        {
                            foreach (JSONNode C in this.Children)
                            {
                                foreach (JSONNode D in C.DeepChildren)
                                {
                                    yield return D;
                                }
                            }
                            yield break;
                        }
                    }
                    public override string ToString()
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        this.WriteToStringBuilder(stringBuilder, 0, 0, JSONTextMode.Compact);
                        return stringBuilder.ToString();
                    }
                    public virtual string ToString(int aIndent)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        this.WriteToStringBuilder(stringBuilder, 0, aIndent, JSONTextMode.Indent);
                        return stringBuilder.ToString();
                    }
                    internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode);
                    public abstract JSONNodeType Tag { get; }
                    public virtual double AsDouble
                    {
                        get
                        {
                            double result = 0.0;
                            if (double.TryParse(this.Value, out result))
                            {
                                return result;
                            }
                            return 0.0;
                        }
                        set
                        {
                            this.Value = value.ToString();
                        }
                    }
                    public virtual int AsInt
                    {
                        get
                        {
                            return (int)this.AsDouble;
                        }
                        set
                        {
                            this.AsDouble = (double)value;
                        }
                    }
                    public virtual float AsFloat
                    {
                        get
                        {
                            return (float)this.AsDouble;
                        }
                        set
                        {
                            this.AsDouble = (double)value;
                        }
                    }
                    public virtual bool AsBool
                    {
                        get
                        {
                            bool result = false;
                            if (bool.TryParse(this.Value, out result))
                            {
                                return result;
                            }
                            return !string.IsNullOrEmpty(this.Value);
                        }
                        set
                        {
                            this.Value = ((!value) ? "false" : "true");
                        }
                    }
                    public virtual JSONArray AsArray
                    {
                        get
                        {
                            return this as JSONArray;
                        }
                    }
                    public virtual JSONObject AsObject
                    {
                        get
                        {
                            return this as JSONObject;
                        }
                    }
                    public static implicit operator JSONNode(string s)
                    {
                        return new JSONString(s);
                    }
                    public static implicit operator string(JSONNode d)
                    {
                        return (!(d == null)) ? d.Value : null;
                    }
                    public static implicit operator JSONNode(double n)
                    {
                        return new JSONNumber(n);
                    }
                    public static implicit operator double(JSONNode d)
                    {
                        return (!(d == null)) ? d.AsDouble : 0.0;
                    }
                    public static implicit operator JSONNode(float n)
                    {
                        return new JSONNumber((double)n);
                    }
                    public static implicit operator float(JSONNode d)
                    {
                        return (!(d == null)) ? d.AsFloat : 0f;
                    }
                    public static implicit operator JSONNode(int n)
                    {
                        return new JSONNumber((double)n);
                    }
                    public static implicit operator int(JSONNode d)
                    {
                        return (!(d == null)) ? d.AsInt : 0;
                    }
                    public static implicit operator JSONNode(bool b)
                    {
                        return new JSONBool(b);
                    }
                    public static implicit operator bool(JSONNode d)
                    {
                        return !(d == null) && d.AsBool;
                    }
                    public static bool operator ==(JSONNode a, object b)
                    {
                        if (object.ReferenceEquals(a, b))
                        {
                            return true;
                        }
                        bool flag = a is JSONNull || object.ReferenceEquals(a, null) || a is JSONLazyCreator;
                        bool flag2 = b is JSONNull || object.ReferenceEquals(b, null) || b is JSONLazyCreator;
                        return (flag && flag2) || a.Equals(b);
                    }
                    public static bool operator !=(JSONNode a, object b)
                    {
                        return !(a == b);
                    }
                    public override bool Equals(object obj)
                    {
                        return object.ReferenceEquals(this, obj);
                    }
                    public override int GetHashCode()
                    {
                        return base.GetHashCode();
                    }
                    internal static string Escape(string aText)
                    {
                        JSONNode.m_EscapeBuilder.Length = 0;
                        if (JSONNode.m_EscapeBuilder.Capacity < aText.Length + aText.Length / 10)
                        {
                            JSONNode.m_EscapeBuilder.Capacity = aText.Length + aText.Length / 10;
                        }
                        foreach (char c in aText)
                        {
                            switch (c)
                            {
                                case '\b':
                                    JSONNode.m_EscapeBuilder.Append("\\b");
                                    break;
                                case '\t':
                                    JSONNode.m_EscapeBuilder.Append("\\t");
                                    break;
                                case '\n':
                                    JSONNode.m_EscapeBuilder.Append("\\n");
                                    break;
                                default:
                                    if (c != '"')
                                    {
                                        if (c != '\\')
                                        {
                                            JSONNode.m_EscapeBuilder.Append(c);
                                        }
                                        else
                                        {
                                            JSONNode.m_EscapeBuilder.Append("\\\\");
                                        }
                                    }
                                    else
                                    {
                                        JSONNode.m_EscapeBuilder.Append("\\\"");
                                    }
                                    break;
                                case '\f':
                                    JSONNode.m_EscapeBuilder.Append("\\f");
                                    break;
                                case '\r':
                                    JSONNode.m_EscapeBuilder.Append("\\r");
                                    break;
                            }
                        }
                        string result = JSONNode.m_EscapeBuilder.ToString();
                        JSONNode.m_EscapeBuilder.Length = 0;
                        return result;
                    }

                    // Token: 0x060009C5 RID: 2501 RVA: 0x000417E4 File Offset: 0x0003FBE4
                    private static void ParseElement(JSONNode ctx, string token, string tokenName, bool quoted)
                    {
                        if (quoted)
                        {
                            ctx.Add(tokenName, token);
                            return;
                        }
                        string a = token.ToLower();
                        double n;
                        if (a == "false" || a == "true")
                        {
                            ctx.Add(tokenName, a == "true");
                        }
                        else if (a == "null")
                        {
                            ctx.Add(tokenName, null);
                        }
                        else if (double.TryParse(token, out n))
                        {
                            ctx.Add(tokenName, n);
                        }
                        else
                        {
                            ctx.Add(tokenName, token);
                        }
                    }

                    // Token: 0x060009C6 RID: 2502 RVA: 0x00041894 File Offset: 0x0003FC94
                    public static JSONNode Parse(string aJSON)
                    {
                        Stack<JSONNode> stack = new Stack<JSONNode>();
                        JSONNode jsonnode = null;
                        int i = 0;
                        StringBuilder stringBuilder = new StringBuilder();
                        string text = string.Empty;
                        bool flag = false;
                        bool flag2 = false;
                        while (i < aJSON.Length)
                        {
                            char c = aJSON[i];
                            switch (c)
                            {
                                case '\t':
                                    goto IL_275;
                                case '\n':
                                case '\r':
                                    break;
                                default:
                                    switch (c)
                                    {
                                        case '[':
                                            if (flag)
                                            {
                                                stringBuilder.Append(aJSON[i]);
                                                goto IL_371;
                                            }
                                            stack.Push(new JSONArray());
                                            if (jsonnode != null)
                                            {
                                                jsonnode.Add(text, stack.Peek());
                                            }
                                            text = string.Empty;
                                            stringBuilder.Length = 0;
                                            jsonnode = stack.Peek();
                                            goto IL_371;
                                        case '\\':
                                            i++;
                                            if (flag)
                                            {
                                                char c2 = aJSON[i];
                                                switch (c2)
                                                {
                                                    case 'r':
                                                        stringBuilder.Append('\r');
                                                        break;
                                                    default:
                                                        if (c2 != 'b')
                                                        {
                                                            if (c2 != 'f')
                                                            {
                                                                if (c2 != 'n')
                                                                {
                                                                    stringBuilder.Append(c2);
                                                                }
                                                                else
                                                                {
                                                                    stringBuilder.Append('\n');
                                                                }
                                                            }
                                                            else
                                                            {
                                                                stringBuilder.Append('\f');
                                                            }
                                                        }
                                                        else
                                                        {
                                                            stringBuilder.Append('\b');
                                                        }
                                                        break;
                                                    case 't':
                                                        stringBuilder.Append('\t');
                                                        break;
                                                    case 'u':
                                                        {
                                                            string s = aJSON.Substring(i + 1, 4);
                                                            stringBuilder.Append((char)int.Parse(s, NumberStyles.AllowHexSpecifier));
                                                            i += 4;
                                                            break;
                                                        }
                                                }
                                            }
                                            goto IL_371;
                                        case ']':
                                            break;
                                        default:
                                            switch (c)
                                            {
                                                case ' ':
                                                    goto IL_275;
                                                default:
                                                    switch (c)
                                                    {
                                                        case '{':
                                                            if (flag)
                                                            {
                                                                stringBuilder.Append(aJSON[i]);
                                                                goto IL_371;
                                                            }
                                                            stack.Push(new JSONObject());
                                                            if (jsonnode != null)
                                                            {
                                                                jsonnode.Add(text, stack.Peek());
                                                            }
                                                            text = string.Empty;
                                                            stringBuilder.Length = 0;
                                                            jsonnode = stack.Peek();
                                                            goto IL_371;
                                                        default:
                                                            if (c != ',')
                                                            {
                                                                if (c != ':')
                                                                {
                                                                    stringBuilder.Append(aJSON[i]);
                                                                    goto IL_371;
                                                                }
                                                                if (flag)
                                                                {
                                                                    stringBuilder.Append(aJSON[i]);
                                                                    goto IL_371;
                                                                }
                                                                text = stringBuilder.ToString();
                                                                stringBuilder.Length = 0;
                                                                flag2 = false;
                                                                goto IL_371;
                                                            }
                                                            else
                                                            {
                                                                if (flag)
                                                                {
                                                                    stringBuilder.Append(aJSON[i]);
                                                                    goto IL_371;
                                                                }
                                                                if (stringBuilder.Length > 0 || flag2)
                                                                {
                                                                    JSONNode.ParseElement(jsonnode, stringBuilder.ToString(), text, flag2);
                                                                }
                                                                text = string.Empty;
                                                                stringBuilder.Length = 0;
                                                                flag2 = false;
                                                                goto IL_371;
                                                            }
                                                        case '}':
                                                            break;
                                                    }
                                                    break;
                                                case '"':
                                                    flag ^= true;
                                                    flag2 = (flag2 || flag);
                                                    goto IL_371;
                                            }
                                            break;
                                    }
                                    if (flag)
                                    {
                                        stringBuilder.Append(aJSON[i]);
                                    }
                                    else
                                    {
                                        if (stack.Count == 0)
                                        {
                                            throw new Exception("JSON Parse: Too many closing brackets");
                                        }
                                        stack.Pop();
                                        if (stringBuilder.Length > 0 || flag2)
                                        {
                                            JSONNode.ParseElement(jsonnode, stringBuilder.ToString(), text, flag2);
                                            flag2 = false;
                                        }
                                        text = string.Empty;
                                        stringBuilder.Length = 0;
                                        if (stack.Count > 0)
                                        {
                                            jsonnode = stack.Peek();
                                        }
                                    }
                                    break;
                            }
                        IL_371:
                            i++;
                            continue;
                        IL_275:
                            if (flag)
                            {
                                stringBuilder.Append(aJSON[i]);
                            }
                            goto IL_371;
                        }
                        if (flag)
                        {
                            throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
                        }
                        return jsonnode;
                    }

                    // Token: 0x060009C7 RID: 2503 RVA: 0x00041C35 File Offset: 0x00040035
                    public virtual void Serialize(System.IO.BinaryWriter aWriter)
                    {
                    }

                    // Token: 0x060009C8 RID: 2504 RVA: 0x00041C38 File Offset: 0x00040038
                    public void SaveToStream(System.IO.Stream aData)
                    {
                        System.IO.BinaryWriter aWriter = new System.IO.BinaryWriter(aData);
                        this.Serialize(aWriter);
                    }

                    // Token: 0x060009C9 RID: 2505 RVA: 0x00041C53 File Offset: 0x00040053
                    public void SaveToCompressedStream(System.IO.Stream aData)
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009CA RID: 2506 RVA: 0x00041C5F File Offset: 0x0004005F
                    public void SaveToCompressedFile(string aFileName)
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009CB RID: 2507 RVA: 0x00041C6B File Offset: 0x0004006B
                    public string SaveToCompressedBase64()
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009CC RID: 2508 RVA: 0x00041C78 File Offset: 0x00040078
                    public void SaveToFile(string aFileName)
                    {
                        System.IO.Directory.CreateDirectory(new System.IO.FileInfo(aFileName).Directory.FullName);
                        using (System.IO.FileStream fileStream = System.IO.File.OpenWrite(aFileName))
                        {
                            this.SaveToStream(fileStream);
                        }
                    }

                    // Token: 0x060009CD RID: 2509 RVA: 0x00041CCC File Offset: 0x000400CC
                    public string SaveToBase64()
                    {
                        string result;
                        using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                        {
                            this.SaveToStream(memoryStream);
                            memoryStream.Position = 0L;
                            result = Convert.ToBase64String(memoryStream.ToArray());
                        }
                        return result;
                    }

                    // Token: 0x060009CE RID: 2510 RVA: 0x00041D20 File Offset: 0x00040120
                    public static JSONNode Deserialize(System.IO.BinaryReader aReader)
                    {
                        JSONNodeType jsonnodeType = (JSONNodeType)aReader.ReadByte();
                        switch (jsonnodeType)
                        {
                            case JSONNodeType.Array:
                                {
                                    int num = aReader.ReadInt32();
                                    JSONArray jsonarray = new JSONArray();
                                    for (int i = 0; i < num; i++)
                                    {
                                        jsonarray.Add(JSONNode.Deserialize(aReader));
                                    }
                                    return jsonarray;
                                }
                            case JSONNodeType.Object:
                                {
                                    int num2 = aReader.ReadInt32();
                                    JSONObject jsonobject = new JSONObject();
                                    for (int j = 0; j < num2; j++)
                                    {
                                        string aKey = aReader.ReadString();
                                        JSONNode aItem = JSONNode.Deserialize(aReader);
                                        jsonobject.Add(aKey, aItem);
                                    }
                                    return jsonobject;
                                }
                            case JSONNodeType.String:
                                return new JSONString(aReader.ReadString());
                            case JSONNodeType.Number:
                                return new JSONNumber(aReader.ReadDouble());
                            case JSONNodeType.NullValue:
                                return new JSONNull();
                            case JSONNodeType.Boolean:
                                return new JSONBool(aReader.ReadBoolean());
                            default:
                                throw new Exception("Error deserializing JSON. Unknown tag: " + jsonnodeType);
                        }
                    }

                    // Token: 0x060009CF RID: 2511 RVA: 0x00041E09 File Offset: 0x00040209
                    public static JSONNode LoadFromCompressedFile(string aFileName)
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009D0 RID: 2512 RVA: 0x00041E15 File Offset: 0x00040215
                    public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009D1 RID: 2513 RVA: 0x00041E21 File Offset: 0x00040221
                    public static JSONNode LoadFromCompressedBase64(string aBase64)
                    {
                        throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
                    }

                    // Token: 0x060009D2 RID: 2514 RVA: 0x00041E30 File Offset: 0x00040230
                    public static JSONNode LoadFromStream(System.IO.Stream aData)
                    {
                        JSONNode result;
                        using (System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(aData))
                        {
                            result = JSONNode.Deserialize(binaryReader);
                        }
                        return result;
                    }

                    // Token: 0x060009D3 RID: 2515 RVA: 0x00041E70 File Offset: 0x00040270
                    public static JSONNode LoadFromFile(string aFileName)
                    {
                        JSONNode result;
                        using (System.IO.FileStream fileStream = System.IO.File.OpenRead(aFileName))
                        {
                            result = JSONNode.LoadFromStream(fileStream);
                        }
                        return result;
                    }

                    // Token: 0x060009D4 RID: 2516 RVA: 0x00041EB0 File Offset: 0x000402B0
                    public static JSONNode LoadFromBase64(string aBase64)
                    {
                        byte[] buffer = Convert.FromBase64String(aBase64);
                        return JSONNode.LoadFromStream(new System.IO.MemoryStream(buffer)
                        {
                            Position = 0L
                        });
                    }

                    // Token: 0x040008AF RID: 2223
                    internal static StringBuilder m_EscapeBuilder = new StringBuilder();
                }
                // Token: 0x02000135 RID: 309
                internal class JSONLazyCreator : JSONNode
                {
                    // Token: 0x06000A22 RID: 2594 RVA: 0x000431F5 File Offset: 0x000415F5
                    public JSONLazyCreator(JSONNode aNode)
                    {
                        this.m_Node = aNode;
                        this.m_Key = null;
                    }

                    // Token: 0x06000A23 RID: 2595 RVA: 0x0004320B File Offset: 0x0004160B
                    public JSONLazyCreator(JSONNode aNode, string aKey)
                    {
                        this.m_Node = aNode;
                        this.m_Key = aKey;
                    }

                    // Token: 0x17000169 RID: 361
                    // (get) Token: 0x06000A24 RID: 2596 RVA: 0x00043221 File Offset: 0x00041621
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.None;
                        }
                    }

                    // Token: 0x06000A25 RID: 2597 RVA: 0x00043224 File Offset: 0x00041624
                    private void Set(JSONNode aVal)
                    {
                        if (this.m_Key == null)
                        {
                            this.m_Node.Add(aVal);
                        }
                        else
                        {
                            this.m_Node.Add(this.m_Key, aVal);
                        }
                        this.m_Node = null;
                    }

                    // Token: 0x1700016A RID: 362
                    public override JSONNode this[int aIndex]
                    {
                        get
                        {
                            return new JSONLazyCreator(this);
                        }
                        set
                        {
                            this.Set(new JSONArray
                {
                    value
                });
                        }
                    }

                    // Token: 0x1700016B RID: 363
                    public override JSONNode this[string aKey]
                    {
                        get
                        {
                            return new JSONLazyCreator(this, aKey);
                        }
                        set
                        {
                            this.Set(new JSONObject
                {
                    {
                        aKey,
                        value
                    }
                });
                        }
                    }

                    // Token: 0x06000A2A RID: 2602 RVA: 0x000432B4 File Offset: 0x000416B4
                    public override void Add(JSONNode aItem)
                    {
                        this.Set(new JSONArray
            {
                aItem
            });
                    }

                    // Token: 0x06000A2B RID: 2603 RVA: 0x000432D8 File Offset: 0x000416D8
                    public override void Add(string aKey, JSONNode aItem)
                    {
                        this.Set(new JSONObject
            {
                {
                    aKey,
                    aItem
                }
            });
                    }

                    // Token: 0x06000A2C RID: 2604 RVA: 0x000432FA File Offset: 0x000416FA
                    public static bool operator ==(JSONLazyCreator a, object b)
                    {
                        return b == null || object.ReferenceEquals(a, b);
                    }

                    // Token: 0x06000A2D RID: 2605 RVA: 0x0004330B File Offset: 0x0004170B
                    public static bool operator !=(JSONLazyCreator a, object b)
                    {
                        return !(a == b);
                    }

                    // Token: 0x06000A2E RID: 2606 RVA: 0x00043317 File Offset: 0x00041717
                    public override bool Equals(object obj)
                    {
                        return obj == null || object.ReferenceEquals(this, obj);
                    }

                    // Token: 0x06000A2F RID: 2607 RVA: 0x00043328 File Offset: 0x00041728
                    public override int GetHashCode()
                    {
                        return 0;
                    }

                    // Token: 0x1700016C RID: 364
                    // (get) Token: 0x06000A30 RID: 2608 RVA: 0x0004332C File Offset: 0x0004172C
                    // (set) Token: 0x06000A31 RID: 2609 RVA: 0x00043350 File Offset: 0x00041750
                    public override int AsInt
                    {
                        get
                        {
                            JSONNumber aVal = new JSONNumber(0.0);
                            this.Set(aVal);
                            return 0;
                        }
                        set
                        {
                            JSONNumber aVal = new JSONNumber((double)value);
                            this.Set(aVal);
                        }
                    }

                    // Token: 0x1700016D RID: 365
                    // (get) Token: 0x06000A32 RID: 2610 RVA: 0x0004336C File Offset: 0x0004176C
                    // (set) Token: 0x06000A33 RID: 2611 RVA: 0x00043394 File Offset: 0x00041794
                    public override float AsFloat
                    {
                        get
                        {
                            JSONNumber aVal = new JSONNumber(0.0);
                            this.Set(aVal);
                            return 0f;
                        }
                        set
                        {
                            JSONNumber aVal = new JSONNumber((double)value);
                            this.Set(aVal);
                        }
                    }

                    // Token: 0x1700016E RID: 366
                    // (get) Token: 0x06000A34 RID: 2612 RVA: 0x000433B0 File Offset: 0x000417B0
                    // (set) Token: 0x06000A35 RID: 2613 RVA: 0x000433DC File Offset: 0x000417DC
                    public override double AsDouble
                    {
                        get
                        {
                            JSONNumber aVal = new JSONNumber(0.0);
                            this.Set(aVal);
                            return 0.0;
                        }
                        set
                        {
                            JSONNumber aVal = new JSONNumber(value);
                            this.Set(aVal);
                        }
                    }

                    // Token: 0x1700016F RID: 367
                    // (get) Token: 0x06000A36 RID: 2614 RVA: 0x000433F8 File Offset: 0x000417F8
                    // (set) Token: 0x06000A37 RID: 2615 RVA: 0x00043414 File Offset: 0x00041814
                    public override bool AsBool
                    {
                        get
                        {
                            JSONBool aVal = new JSONBool(false);
                            this.Set(aVal);
                            return false;
                        }
                        set
                        {
                            JSONBool aVal = new JSONBool(value);
                            this.Set(aVal);
                        }
                    }

                    // Token: 0x17000170 RID: 368
                    // (get) Token: 0x06000A38 RID: 2616 RVA: 0x00043430 File Offset: 0x00041830
                    public override JSONArray AsArray
                    {
                        get
                        {
                            JSONArray jsonarray = new JSONArray();
                            this.Set(jsonarray);
                            return jsonarray;
                        }
                    }

                    // Token: 0x17000171 RID: 369
                    // (get) Token: 0x06000A39 RID: 2617 RVA: 0x0004344C File Offset: 0x0004184C
                    public override JSONObject AsObject
                    {
                        get
                        {
                            JSONObject jsonobject = new JSONObject();
                            this.Set(jsonobject);
                            return jsonobject;
                        }
                    }

                    // Token: 0x06000A3A RID: 2618 RVA: 0x00043467 File Offset: 0x00041867
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append("null");
                    }

                    // Token: 0x040008B7 RID: 2231
                    private JSONNode m_Node;

                    // Token: 0x040008B8 RID: 2232
                    private string m_Key;
                }
                // Token: 0x02000133 RID: 307
                public class JSONBool : JSONNode
                {
                    // Token: 0x06000A0B RID: 2571 RVA: 0x000430CB File Offset: 0x000414CB
                    public JSONBool(bool aData)
                    {
                        this.m_Data = aData;
                    }

                    // Token: 0x06000A0C RID: 2572 RVA: 0x000430DA File Offset: 0x000414DA
                    public JSONBool(string aData)
                    {
                        this.Value = aData;
                    }

                    // Token: 0x17000161 RID: 353
                    // (get) Token: 0x06000A0D RID: 2573 RVA: 0x000430E9 File Offset: 0x000414E9
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.Boolean;
                        }
                    }

                    // Token: 0x17000162 RID: 354
                    // (get) Token: 0x06000A0E RID: 2574 RVA: 0x000430EC File Offset: 0x000414EC
                    public override bool IsBoolean
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x17000163 RID: 355
                    // (get) Token: 0x06000A0F RID: 2575 RVA: 0x000430EF File Offset: 0x000414EF
                    // (set) Token: 0x06000A10 RID: 2576 RVA: 0x00043104 File Offset: 0x00041504
                    public override string Value
                    {
                        get
                        {
                            return this.m_Data.ToString();
                        }
                        set
                        {
                            bool data;
                            if (bool.TryParse(value, out data))
                            {
                                this.m_Data = data;
                            }
                        }
                    }

                    // Token: 0x17000164 RID: 356
                    // (get) Token: 0x06000A11 RID: 2577 RVA: 0x00043125 File Offset: 0x00041525
                    // (set) Token: 0x06000A12 RID: 2578 RVA: 0x0004312D File Offset: 0x0004152D
                    public override bool AsBool
                    {
                        get
                        {
                            return this.m_Data;
                        }
                        set
                        {
                            this.m_Data = value;
                        }
                    }

                    // Token: 0x06000A13 RID: 2579 RVA: 0x00043136 File Offset: 0x00041536
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(6);
                        aWriter.Write(this.m_Data);
                    }

                    // Token: 0x06000A14 RID: 2580 RVA: 0x0004314B File Offset: 0x0004154B
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append((!this.m_Data) ? "false" : "true");
                    }

                    // Token: 0x06000A15 RID: 2581 RVA: 0x0004316E File Offset: 0x0004156E
                    public override bool Equals(object obj)
                    {
                        return obj != null && obj is bool && this.m_Data == (bool)obj;
                    }

                    // Token: 0x06000A16 RID: 2582 RVA: 0x00043193 File Offset: 0x00041593
                    public override int GetHashCode()
                    {
                        return this.m_Data.GetHashCode();
                    }

                    // Token: 0x040008B6 RID: 2230
                    private bool m_Data;
                }
                // Token: 0x0200012F RID: 303
                public class JSONArray : JSONNode, IEnumerable
                {
                    // Token: 0x1700014E RID: 334
                    // (get) Token: 0x060009D7 RID: 2519 RVA: 0x0004218F File Offset: 0x0004058F
                    public override JSONNodeType Tag
                    {
                        get
                        {
                            return JSONNodeType.Array;
                        }
                    }

                    // Token: 0x1700014F RID: 335
                    // (get) Token: 0x060009D8 RID: 2520 RVA: 0x00042192 File Offset: 0x00040592
                    public override bool IsArray
                    {
                        get
                        {
                            return true;
                        }
                    }

                    // Token: 0x17000150 RID: 336
                    public override JSONNode this[int aIndex]
                    {
                        get
                        {
                            if (aIndex < 0 || aIndex >= this.m_List.Count)
                            {
                                return new JSONLazyCreator(this);
                            }
                            return this.m_List[aIndex];
                        }
                        set
                        {
                            if (value == null)
                            {
                                value = new JSONNull();
                            }
                            if (aIndex < 0 || aIndex >= this.m_List.Count)
                            {
                                this.m_List.Add(value);
                            }
                            else
                            {
                                this.m_List[aIndex] = value;
                            }
                        }
                    }

                    // Token: 0x17000151 RID: 337
                    public override JSONNode this[string aKey]
                    {
                        get
                        {
                            return new JSONLazyCreator(this);
                        }
                        set
                        {
                            if (value == null)
                            {
                                value = new JSONNull();
                            }
                            this.m_List.Add(value);
                        }
                    }

                    // Token: 0x17000152 RID: 338
                    // (get) Token: 0x060009DD RID: 2525 RVA: 0x00042243 File Offset: 0x00040643
                    public override int Count
                    {
                        get
                        {
                            return this.m_List.Count;
                        }
                    }

                    // Token: 0x060009DE RID: 2526 RVA: 0x00042250 File Offset: 0x00040650
                    public override void Add(string aKey, JSONNode aItem)
                    {
                        if (aItem == null)
                        {
                            aItem = new JSONNull();
                        }
                        this.m_List.Add(aItem);
                    }

                    // Token: 0x060009DF RID: 2527 RVA: 0x00042274 File Offset: 0x00040674
                    public override JSONNode Remove(int aIndex)
                    {
                        if (aIndex < 0 || aIndex >= this.m_List.Count)
                        {
                            return null;
                        }
                        JSONNode result = this.m_List[aIndex];
                        this.m_List.RemoveAt(aIndex);
                        return result;
                    }

                    // Token: 0x060009E0 RID: 2528 RVA: 0x000422B5 File Offset: 0x000406B5
                    public override JSONNode Remove(JSONNode aNode)
                    {
                        this.m_List.Remove(aNode);
                        return aNode;
                    }

                    // Token: 0x17000153 RID: 339
                    // (get) Token: 0x060009E1 RID: 2529 RVA: 0x000422C8 File Offset: 0x000406C8
                    public override IEnumerable<JSONNode> Children
                    {
                        get
                        {
                            foreach (JSONNode N in this.m_List)
                            {
                                yield return N;
                            }
                            yield break;
                        }
                    }

                    // Token: 0x060009E2 RID: 2530 RVA: 0x000422EC File Offset: 0x000406EC
                    public IEnumerator GetEnumerator()
                    {
                        foreach (JSONNode N in this.m_List)
                        {
                            yield return N;
                        }
                        yield break;
                    }

                    // Token: 0x060009E3 RID: 2531 RVA: 0x00042308 File Offset: 0x00040708
                    public override void Serialize(System.IO.BinaryWriter aWriter)
                    {
                        aWriter.Write(1);
                        aWriter.Write(this.m_List.Count);
                        for (int i = 0; i < this.m_List.Count; i++)
                        {
                            this.m_List[i].Serialize(aWriter);
                        }
                    }

                    // Token: 0x060009E4 RID: 2532 RVA: 0x0004235C File Offset: 0x0004075C
                    internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
                    {
                        aSB.Append('[');
                        int count = this.m_List.Count;
                        if (this.inline)
                        {
                            aMode = JSONTextMode.Compact;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                aSB.Append(',');
                            }
                            if (aMode == JSONTextMode.Indent)
                            {
                                aSB.AppendLine();
                            }
                            if (aMode == JSONTextMode.Indent)
                            {
                                aSB.Append(' ', aIndent + aIndentInc);
                            }
                            this.m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
                        }
                        if (aMode == JSONTextMode.Indent)
                        {
                            aSB.AppendLine().Append(' ', aIndent);
                        }
                        aSB.Append(']');
                    }

                    // Token: 0x040008B0 RID: 2224
                    private List<JSONNode> m_List = new List<JSONNode>();

                    // Token: 0x040008B1 RID: 2225
                    public bool inline;
                }
                // Token: 0x02000136 RID: 310
                public static class JSON
                {
                    // Token: 0x06000A3B RID: 2619 RVA: 0x00043475 File Offset: 0x00041875
                    public static JSONNode Parse(string aJSON)
                    {
                        return JSONNode.Parse(aJSON);
                    }
                    public static string Format(string input, string Indent = "    ")
                    {
                        var output = new StringBuilder(input.Length * 2);
                        char? quote = null;
                        int depth = 0;

                        for (int i = 0; i < input.Length; ++i)
                        {
                            char ch = input[i];

                            switch (ch)
                            {
                                case '{':
                                case '[':
                                    output.Append(ch);
                                    if (!quote.HasValue)
                                    {
                                        output.AppendLine();
                                        output.Append(Repeat(Indent, ++depth));
                                    }
                                    break;
                                case '}':
                                case ']':
                                    if (quote.HasValue)
                                        output.Append(ch);
                                    else
                                    {
                                        output.AppendLine();
                                        output.Append(Repeat(Indent, --depth));
                                        output.Append(ch);
                                    }
                                    break;
                                case '"':
                                case '\'':
                                    output.Append(ch);
                                    if (quote.HasValue)
                                    {
                                        if (!IsEscaped(output, i))
                                            quote = null;
                                    }
                                    else quote = ch;
                                    break;
                                case ',':
                                    output.Append(ch);
                                    if (!quote.HasValue)
                                    {
                                        output.AppendLine();
                                        output.Append(Repeat(Indent, depth));
                                    }
                                    break;
                                case ':':
                                    if (quote.HasValue) output.Append(ch);
                                    else output.Append(" : ");
                                    break;
                                default:
                                    if (quote.HasValue || !char.IsWhiteSpace(ch))
                                        output.Append(ch);
                                    break;
                            }
                        }

                        return output.ToString();
                    }
                    private static string Repeat(string str, int count)
                    {
                        return new StringBuilder().Insert(0, str, count).ToString();
                    }

                    private static bool IsEscaped(string str, int index)
                    {
                        bool escaped = false;
                        while (index > 0 && str[--index] == '\\') escaped = !escaped;
                        return escaped;
                    }

                    private static bool IsEscaped(StringBuilder str, int index)
                    {
                        return IsEscaped(str.ToString(), index);
                    }
                }
                internal static class Extensions
                {
                    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
                    {
                        foreach (var i in ie)
                        {
                            action(i);
                        }
                    }
                }
            }
            namespace Cryptography
            {
                public class XORAlgorithm
                {
                    public string Encrypt(string Text, string key)
                    {
                        byte[] data = UTF8Encoding.UTF8.GetBytes(Text);
                        using (System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                        {
                            byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                            using (System.Security.Cryptography.TripleDESCryptoServiceProvider tripDes = new System.Security.Cryptography.TripleDESCryptoServiceProvider() { Key = keys, Mode = System.Security.Cryptography.CipherMode.ECB, Padding = System.Security.Cryptography.PaddingMode.PKCS7 })
                            {
                                System.Security.Cryptography.ICryptoTransform transform = tripDes.CreateEncryptor();
                                byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                                return Convert.ToBase64String(results, 0, results.Length);
                            }
                        }
                    }
                    public string Decrypt(string Text, string key)
                    {
                        byte[] data = Convert.FromBase64String(Text);
                        using (System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
                        {
                            byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                            using (System.Security.Cryptography.TripleDESCryptoServiceProvider tripDes = new System.Security.Cryptography.TripleDESCryptoServiceProvider() { Key = keys, Mode = System.Security.Cryptography.CipherMode.ECB, Padding = System.Security.Cryptography.PaddingMode.PKCS7 })
                            {
                                System.Security.Cryptography.ICryptoTransform transform = tripDes.CreateDecryptor();
                                byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                                return UTF8Encoding.UTF8.GetString(results);
                            }
                        }
                    }
                }
                public class FileCryptor
                {
                    /// <summary>
                    /// Encrypt a file
                    /// </summary>
                    /// <param name="filePath">Self explanatory</param>
                    /// <param name="key">A 8 char lenght key that must contain at least 1 letter (containing only numbers whill cause the function to return false)</param>
                    /// <returns>True for success; False for fail</returns>
                    public bool EncryptFile(string filePath, string key = "??9V0g?8")
                    {
                        if (key.Length != 8)
                        {
                            return false;
                        }
                        try
                        {
                            byte[] plainContent = System.IO.File.ReadAllBytes(filePath);
                            using (var DES = new System.Security.Cryptography.DESCryptoServiceProvider())
                            {
                                DES.IV = Encoding.UTF8.GetBytes(key);
                                DES.Key = Encoding.UTF8.GetBytes(key);
                                DES.Mode = System.Security.Cryptography.CipherMode.CBC;
                                DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;


                                using (var memStream = new System.IO.MemoryStream())
                                {
                                    System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memStream, DES.CreateEncryptor(),
                                           System.Security.Cryptography.CryptoStreamMode.Write);

                                    cryptoStream.Write(plainContent, 0, plainContent.Length);
                                    cryptoStream.FlushFinalBlock();
                                    System.IO.File.WriteAllBytes(filePath, memStream.ToArray());
                                    return true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    /// <summary>
                    /// Decrypt a file
                    /// </summary>
                    /// <param name="filePath">Self explanatory</param>
                    /// <param name="key">A 8 char lenght key that must contain at least 1 letter (containing only numbers whill cause the function to return false)</param>
                    /// <returns>True for success; False for fail</returns>
                    public bool DecryptFile(string filePath, string key = "??9V0g?8")
                    {
                        if (key.Length != 8)
                        {
                            return false;
                        }
                        try
                        {
                            byte[] encrypted = System.IO.File.ReadAllBytes(filePath);
                            using (var DES = new System.Security.Cryptography.DESCryptoServiceProvider())
                            {
                                DES.IV = Encoding.UTF8.GetBytes(key);
                                DES.Key = Encoding.UTF8.GetBytes(key);
                                DES.Mode = System.Security.Cryptography.CipherMode.CBC;
                                DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;


                                using (var memStream = new System.IO.MemoryStream())
                                {
                                    System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memStream, DES.CreateDecryptor(),
                                    System.Security.Cryptography.CryptoStreamMode.Write);

                                    cryptoStream.Write(encrypted, 0, encrypted.Length);
                                    cryptoStream.FlushFinalBlock();
                                    System.IO.File.WriteAllBytes(filePath, memStream.ToArray());
                                    return true;
                                }
                            }

                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    /// <summary>
                    /// Generate a random DES key to be used with the functions: EncryptFile() ; DecryptFile()
                    /// </summary>
                    /// <returns>Returns a random DES key</returns>
                    public string GenerateDESKey()
                    {
                        System.Security.Cryptography.DESCryptoServiceProvider dESCrypto = (System.Security.Cryptography.DESCryptoServiceProvider)System.Security.Cryptography.DESCryptoServiceProvider.Create();
                        return ASCIIEncoding.ASCII.GetString(dESCrypto.Key);
                    }
                }
                public class Base64Cryptor
                {
                    public string DecryptUntilEnd(string StringToDecode)
                    {
                        if (StringToDecode == null)
                        {
                            throw new ArgumentNullException(nameof(StringToDecode));
                        }

                        string OldCrypt = "";
                        string CurCrypt = "";
                        while (CurCrypt != "String could not be decoded")
                        {
                            OldCrypt = CurCrypt;
                            CurCrypt = Decode(CurCrypt);
                        }
                        return OldCrypt;
                    }
                    public string Encode(string StringToEncode)
                    {
                        try
                        {
                            return Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(StringToEncode));
                        }
                        catch (Exception)
                        {
                            return "String could not be encoded!";
                        }
                    }
                    public string Decode(string StringToDecode)
                    {
                        try
                        {
                            return new System.Text.ASCIIEncoding().GetString(Convert.FromBase64String(StringToDecode));
                        }
                        catch (Exception)
                        {
                            return "String could not be decoded!";
                        }
                    }
                    public string MultiEncode(string StringToEncode, int EncryptionAmmount)
                    {
                        string crypted = StringToEncode;
                        int i = 0;
                        while (i != EncryptionAmmount)
                        {
                            crypted = Encode(crypted);
                            i++;
                        }
                        return crypted;
                    }
                    public string MultiDecode(string StringToDecode, int EncryptionAmmount)
                    {
                        string decrypted = StringToDecode;
                        int i = 0;
                        while (i != EncryptionAmmount)
                        {
                            decrypted = Decode(decrypted);
                            i++;
                        }
                        return decrypted;
                    }
                }
                public class CesarCodeCryptor
                {
                    public string Encode(string StringToEncode, int key)
                    {
                        string total = "";
                        int i;
                        for (i = 1; i == StringToEncode.Length; i++)
                        {
                            string tmp = StringToEncode[i].ToString();
                            tmp = Convert.ToChar(Convert.ToInt32(Convert.ToChar(Convert.ToByte(tmp))) + key).ToString();
                            total += tmp;
                        }
                        return total;
                    }
                    public string Decode(string StringToEncode, int key)
                    {
                        string total = "";
                        int i;
                        for (i = 1; i == StringToEncode.Length; i++)
                        {
                            string tmp = StringToEncode[i].ToString();
                            tmp = Convert.ToChar(Convert.ToInt32(Convert.ToChar(Convert.ToByte(tmp))) - key).ToString();
                            total += tmp;
                        }
                        return total;
                    }
                }
                public class Hasher
                {
                    //public string RSAEncrypt(string Text)
                    //{

                    //}
                    //public string RSADecrypt(string Text)
                    //{

                    //}
                    public string SHA1Hash(string StringToHash)
                    {
                        System.Security.Cryptography.SHA1CryptoServiceProvider ShaObj = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                        byte[] ByteString = Encoding.ASCII.GetBytes(StringToHash);
                        ByteString = ShaObj.ComputeHash(ByteString);
                        string FinalString = "";
                        foreach (byte bt in ByteString)
                        {
                            FinalString += bt.ToString("x2");
                        }
                        return FinalString;
                    }
                    public string SHA256Hash(string StringToHash)
                    {
                        System.Security.Cryptography.SHA256CryptoServiceProvider ShaObj = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                        byte[] ByteString = Encoding.ASCII.GetBytes(StringToHash);
                        ByteString = ShaObj.ComputeHash(ByteString);
                        string FinalString = "";
                        foreach (byte bt in ByteString)
                        {
                            FinalString += bt.ToString("x2");
                        }
                        return FinalString;
                    }
                    public string SHA384Hash(string StringToHash)
                    {
                        System.Security.Cryptography.SHA384CryptoServiceProvider ShaObj = new System.Security.Cryptography.SHA384CryptoServiceProvider();
                        byte[] ByteString = Encoding.ASCII.GetBytes(StringToHash);
                        ByteString = ShaObj.ComputeHash(ByteString);
                        string FinalString = "";
                        foreach (byte bt in ByteString)
                        {
                            FinalString += bt.ToString("x2");
                        }
                        return FinalString;
                    }
                    public string SHA512Hash(string StringToHash)
                    {
                        System.Security.Cryptography.SHA512CryptoServiceProvider ShaObj = new System.Security.Cryptography.SHA512CryptoServiceProvider();
                        byte[] ByteString = Encoding.ASCII.GetBytes(StringToHash);
                        ByteString = ShaObj.ComputeHash(ByteString);
                        string FinalString = "";
                        foreach (byte bt in ByteString)
                        {
                            FinalString += bt.ToString("x2");
                        }
                        return FinalString;
                    }
                    public string MD5Hash(string StringToHash)
                    {
                        System.Security.Cryptography.MD5CryptoServiceProvider HashObj = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        byte[] ByteString = Encoding.ASCII.GetBytes(StringToHash);
                        ByteString = HashObj.ComputeHash(ByteString);
                        string FinalString = "";
                        foreach (byte bt in ByteString)
                        {
                            FinalString += bt.ToString("x2");
                        }
                        return FinalString;
                    }
                }
            }
            public class DataManager
            {
                public DataManager()
                {

                }
                public DataManager(object data)
                {
                    Data = data;
                }
                public DataManager(object data, string file)
                {
                    Data = data; File = file;
                }
                public DataManager(string file)
                {
                    File = file;
                }
                public object Data { get; set; }
                public string File { get; set; }
                public void SaveDataAsXML()
                {
                    SaveObjectToXML(Data.GetType(), Data, File);
                }
                public object LoadDataAsXML()
                {
                    Data = LoadObjectFromXML(Data.GetType(), File);
                    return Data;
                }
                public void SaveDataAsBinary()
                {
                    SaveObjectToBinary(Data, File);
                }
                public object LoadDataAsBinary()
                {
                    Data = LoadObjectFromBinary(File);
                    return Data;
                }
                public static void SaveObjectToBinary(object data, string file)
                {
                    System.IO.Stream stream = System.IO.File.Open(file, System.IO.FileMode.Create);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, data);
                    stream.Close();
                }
                public static object LoadObjectFromBinary(string file)
                {
                    System.IO.Stream stream = System.IO.File.Open(file, System.IO.FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    object data;
                    try { data = bf.Deserialize(stream); } catch (Exception ex) { stream.Close(); throw ex; }
                    return data;
                }
                public static void SaveObjectToXML(Type type, object data, string file)
                {
                    System.IO.TextWriter tw = new System.IO.StreamWriter(file);
                    XmlSerializer serializer = new XmlSerializer(type);
                    try { serializer.Serialize(tw, data); }
                    catch (Exception ex)
                    {
                        tw.Close();
                        throw ex;
                    }
                    tw.Close();
                }
                public static object LoadObjectFromXML(Type type, string file)
                {
                    System.IO.TextReader tr = new System.IO.StreamReader(file);
                    XmlSerializer serializer = new XmlSerializer(type);
                    object data;
                    try { data = serializer.Deserialize(tr); }
                    catch (Exception ex)
                    {
                        tr.Close();
                        throw ex;
                    }
                    return data;
                }
            }
            public static class Resource
            {
                public static string[] ListResources() => Assembly.GetEntryAssembly().GetManifestResourceNames();
                public static string[] ListResources(Assembly assembly) => assembly.GetManifestResourceNames();
                public static string[] ListResources(string @namespace, string directory)
                {
                    List<string> s = new List<string>();
                    foreach (var res in ListResources())
                        if (res.Substring(0, (@namespace + "." + directory).Length) == @namespace + "." + directory)
                            s.Add(res);
                    return s.ToArray();
                }
                public static string[] ListResources(Assembly assembly, string @namespace, string directory)
                {
                    List<string> s = new List<string>();
                    foreach (var res in ListResources(assembly))
                        if (res.Substring(0, (@namespace + "." + directory).Length) == @namespace + "." + directory)
                            s.Add(res);
                    return s.ToArray();
                }
                public static System.Reflection.Assembly LoadAssemblyFromEmbeddedResource(string FilePath, string ResourcePath)
                {
                    return LoadAssemblyFromEmbeddedResource(LoadAssemblyFromFile(FilePath), ResourcePath);
                }
                public static System.Reflection.Assembly LoadAssemblyFromEmbeddedResource(System.Reflection.Assembly assembly, string ResourcePath)
                {
                    var str = assembly.GetManifestResourceStream(ResourcePath);
                    byte[] assemblyData = new byte[str.Length];
                    str.Read(assemblyData, 0, assemblyData.Length);
                    str.Close();
                    return System.Reflection.Assembly.Load(assemblyData);
                }
                public static System.Reflection.Assembly LoadAssemblyFromEmbeddedResource(string ResourcePath)
                {
                    var str = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(ResourcePath);
                    byte[] assemblyData = new byte[str.Length];
                    str.Read(assemblyData, 0, assemblyData.Length);
                    str.Close();
                    return System.Reflection.Assembly.Load(assemblyData);
                }
                public static System.Reflection.Assembly LoadAssemblyFromFile(string Path)
                {
                    System.IO.FileStream str = new System.IO.FileStream(Path, System.IO.FileMode.Open);
                    byte[] assemblyData = new byte[str.Length];
                    str.Read(assemblyData, 0, assemblyData.Length);
                    var ass = System.Reflection.Assembly.Load(assemblyData);
                    str.Close();
                    return ass;
                }
                public static string GetEmbeddedTextFileContent(string FilePath, string ResourcePath)
                {
                    return GetEmbeddedTextFileContent(LoadAssemblyFromFile(FilePath), ResourcePath);
                }
                public static string GetEmbeddedTextFileContent(System.Reflection.Assembly assembly, string ResourcePath)
                {
                    System.IO.Stream ass = assembly.GetManifestResourceStream(ResourcePath);
                    System.IO.BinaryReader r = new System.IO.BinaryReader(ass);
                    int pos = 0;
                    string FileText = null;
                    r.BaseStream.Position = 0;
                    byte[] bts = r.ReadBytes(3);
                    string chkBts = null;
                    foreach (byte bt in bts)
                    {
                        chkBts += bt.ToString("X2");
                    }
                    if (chkBts == "EFBBBF")
                    {
                        pos = 3;
                    }
                    while (pos < r.BaseStream.Length)
                    {
                        r.BaseStream.Position = pos;
                        string posText = Converter.HexadecimalToText(r.ReadByte().ToString("X2"));
                        FileText += posText;
                        pos++;
                    }
                    r.Close();
                    ass.Close();
                    return FileText;
                }
                public static string GetEmbeddedTextFileContent(string ResourcePath)
                {
                    System.IO.Stream ass = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(ResourcePath);
                    System.IO.BinaryReader r = new System.IO.BinaryReader(ass);
                    int pos = 0;
                    string FileText = null;
                    r.BaseStream.Position = 0;
                    byte[] bts = r.ReadBytes(3);
                    string chkBts = null;
                    foreach (byte bt in bts)
                    {
                        chkBts += bt.ToString("X2");
                    }
                    if (chkBts == "EFBBBF")
                    {
                        pos = 3;
                    }
                    while (pos < r.BaseStream.Length)
                    {
                        r.BaseStream.Position = pos;
                        string posText = Converter.HexadecimalToText(r.ReadByte().ToString("X2"));
                        FileText += posText;
                        pos++;
                    }
                    r.Close();
                    ass.Close();
                    return FileText;
                }
                public static void Export(string FilePath, string ResourcePath, string OutFile)
                {
                    Export(LoadAssemblyFromFile(FilePath), ResourcePath, OutFile);
                }
                public static void Export(System.Reflection.Assembly assembly, string ResourcePath, string OutFile)
                {
                    using (System.IO.Stream s = assembly.GetManifestResourceStream(ResourcePath))
                    using (System.IO.BinaryReader r = new System.IO.BinaryReader(s))
                    using (System.IO.FileStream fs = new System.IO.FileStream(OutFile, System.IO.FileMode.OpenOrCreate))
                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs))
                        w.Write(r.ReadBytes((int)s.Length));
                }
                /// <summary>
                /// Exports a embedded resource from your application (resource must have the compile propertie set to Embedded resource)
                /// </summary>
                /// <param name="ResourcePath">The embedded resource path such as 'NamespaceName.Folder1.file.exe' or 'NamespaceName.file.pdf'</param>
                /// <param name="OutFile">The place to save the file</param>
                public static void Export(string ResourcePath, string OutFile)
                {
                    Assembly assembly = Assembly.GetEntryAssembly();
                    using (System.IO.Stream s = assembly.GetManifestResourceStream(ResourcePath))
                    using (System.IO.BinaryReader r = new System.IO.BinaryReader(s))
                    using (System.IO.FileStream fs = new System.IO.FileStream(OutFile, System.IO.FileMode.OpenOrCreate))
                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs))
                        w.Write(r.ReadBytes((int)s.Length));
                }
            }
        }
        public static class Converter
        {
            public static string[] ByteStringToByteArray(string Bytes)
            {
                int Cur = 0;
                string[] ByteArray = new string[Bytes.Length / 2];
                string temp = null;
                int index = 0;
                for (int i = 0; i <= Bytes.Length; i++)
                {
                    if (Cur == 2)
                    {
                        ByteArray[index] = temp;
                        temp = null;
                        index++;
                        i--;
                        Cur = 0;
                    }
                    else
                    {
                        if (i == Bytes.Length)
                        {

                        }
                        else
                        {
                            temp += Bytes.Substring(i, 1);
                            Cur++;
                        }
                    }
                }
                return ByteArray;
            }
            public static string ByteArrayToByteString(string[] Bytes)
            {
                string Data = null;
                foreach (string bt in Bytes)
                {
                    Data += bt;
                }
                return Data;
            }
            public static string ReverseByteOrder(string Bytes)
            {
                string FinalResult = null;
                for (int Cur = ByteStringToByteArray(Bytes).Length - 1; Cur >= 0; Cur--)
                {
                    FinalResult += ByteStringToByteArray(Bytes)[Cur];
                }
                return FinalResult;
            }
            public static string HexadecimalToBinary(string HexNumber)
            {
                int dec = HexadecimalToDecimal(HexNumber);
                return DecimalToBinary(dec);
            }
            public static string DecimalToBinary(int Number)
            {
                return Convert.ToString(Number, 2);
            }
            public static string BinaryToHexadecimal(string BinaryNumber)
            {
                int dec = BinaryToDecimal(BinaryNumber);
                return DecimalToHexadecimal(dec);
            }
            public static int BinaryToDecimal(string BinaryNumber)
            {
                int binarynum = 0;
                for (int bitcount = 1; bitcount <= BinaryNumber.Length; bitcount++)
                {
                    binarynum = Convert.ToInt32(binarynum + (Convert.ToDouble(BinaryNumber.Substring(BinaryNumber.Length - bitcount, 1)) * (System.Math.Pow(2, bitcount - 1))));
                }
                return binarynum;
            }

            /// <summary>
            /// Swaps the given bytes (example "C0" = "0C")
            /// </summary>
            /// <param name="HexBytes">The bytes to swap</param>
            /// <returns>Returns the swaped bytes</returns>
            public static string SwapHexBytes(string HexBytes)
            {
                string l = HexBytes.Substring(0, 1);
                string r = HexBytes.Last().ToString();
                return r + l;
            }
            /// <summary>
            /// Swaps the given bytes (example "C054" = "54C0")
            /// </summary>
            /// <param name="HexBytes">The bytes to swap</param>
            /// <returns>Returns the swaped bytes</returns>
            public static string SwapHexBytesPair(string HexBytes)
            {
                if ((HexBytes.Length / 2) % 2 != 0 || HexBytes.Length <= 0) { return null; }
                string[] Bytes = ByteStringToByteArray(HexBytes);
                for (int i = 0; i <= Bytes.Length - 1; i += 2)
                {
                    string bt = Bytes[i];
                    Bytes[i] = Bytes[i + 1];
                    Bytes[i + 1] = bt;
                }
                return ByteArrayToByteString(Bytes);
            }
            /// <summary>
            /// Inverts the given text (example "55" = "ÊÊ")
            /// </summary>
            /// <param name="Text">The text to invert</param>
            /// <returns>Returns the inverted text</returns>
            public static string InvertASCValue(string Text)
            {
                string hexV = TextToHexadecimal(Text);
                return HexadecimalToText(InvertHexBytes(hexV));
            }
            /// <summary>
            /// Inverts the given bytes (example "C0" = "3F")
            /// </summary>
            /// <param name="HexBytes">The bytes to invert</param>
            /// <returns>Returns the inverted bytes</returns>
            public static string InvertHexBytes(string HexBytes)
            {
                string InvertedBytes = null;
                foreach (var HexByte in HexBytes)
                {
                    string var = HexByte.ToString().ToUpper();
                    switch (var)
                    {
                        case "0":
                            var = "F";
                            break;
                        case "1":
                            var = "E";
                            break;
                        case "2":
                            var = "D";
                            break;
                        case "3":
                            var = "C";
                            break;
                        case "4":
                            var = "B";
                            break;
                        case "5":
                            var = "A";
                            break;
                        case "6":
                            var = "9";
                            break;
                        case "7":
                            var = "8";
                            break;
                        case "8":
                            var = "7";
                            break;
                        case "9":
                            var = "6";
                            break;
                        case "A":
                            var = "5";
                            break;
                        case "B":
                            var = "4";
                            break;
                        case "C":
                            var = "3";
                            break;
                        case "D":
                            var = "2";
                            break;
                        case "E":
                            var = "1";
                            break;
                        case "F":
                            var = "0";
                            break;
                    }
                    InvertedBytes += var.ToString();
                }
                return InvertedBytes;
            }
            public static string DecimalToOctal(int Number)
            {
                return Convert.ToString(Number, 8);
            }
            public static int OctalToDecimal(int OctalNumber)
            {
                var octal = OctalNumber;
                var dec = 0;
                var remain = octal;
                var i = 0;
                while (remain > 0)
                {
                    dec += remain % 10 * (int)System.Math.Pow(8, i);
                    remain /= 10;
                    i++;
                }
                return dec;
            }
            public static string DecimalToHexadecimal(int Number)
            {
                return Convert.ToString(Number, 16).ToUpper();
            }
            public static int HexadecimalToDecimal(string HexNumber)
            {
                return Convert.ToInt32(HexNumber, 16);
            }
            public static string TextToBinary(string Text)
            {
                StringBuilder Result = new StringBuilder();
                foreach (byte Character in System.Text.ASCIIEncoding.ASCII.GetBytes(Text))
                {
                    Result.Append(Convert.ToString(Character, 2).PadLeft(8, '0'));
                    Result.Append(" ");
                }
                string Val = Result.ToString().Substring(0, Result.ToString().Length - 1);
                return Val;
            }
            public static string BinaryToText(string BinaryString)
            {
                string Characters = System.Text.RegularExpressions.Regex.Replace(BinaryString, "[^01]", "");
                byte[] ByteArray = new byte[Characters.Length / 8 - 1];
                int index;
                for (index = 0; index == ByteArray.Length - 1; index++)
                {
                    ByteArray[index] = Convert.ToByte(Characters.Substring(index * 8, 8), 2);
                }
                return System.Text.ASCIIEncoding.ASCII.GetString(ByteArray);
            }
            public static string TextToHexadecimal(string Text)
            {
                string outp = string.Empty;
                char[] value = Text.ToCharArray();
                foreach (char L in value)
                {
                    int V = Convert.ToInt32(L);
                    outp += string.Format("{0:x}", V);
                }
                return outp;
            }
            public static string HexadecimalToText(string HexNumber)
            {
                System.Text.StringBuilder text = new System.Text.StringBuilder(HexNumber.Length / 2);
                for (int i = 0; i <= HexNumber.Length - 2; i += 2)
                {
                    text.Append((char)(Convert.ToByte(HexNumber.Substring(i, 2), 16)));
                }
                return text.ToString();
            }
        }
        public static class Generate
        {
            public static string GUID()
            {
                string[] bts = Converter.ByteStringToByteArray(RandomHexBytes(16, ""));
                return bts[3] + bts[2] + bts[1] + bts[0] + "-" + bts[5] + bts[4] + "-" + bts[6] + bts[7] + "-" + bts[8] + bts[9] + " " + bts[10] + bts[11] + bts[12] + bts[13] + bts[14] + bts[15];
            }
            public static string GUID(string HexadecimalBytes)
            {
                string[] bts = Converter.ByteStringToByteArray(HexadecimalBytes);
                return bts[3] + bts[2] + bts[1] + bts[0] + "-" + bts[5] + bts[4] + "-" + bts[6] + bts[7] + "-" + bts[8] + bts[9] + " " + bts[10] + bts[11] + bts[12] + bts[13] + bts[14] + bts[15];
            }
            /// <summary>
            /// Generates random Hexadecimal Bytes
            /// </summary>
            /// <param name="AmmountOfBytes">The ammount of bytes to be generated</param>
            public static string RandomHexBytes(int AmmountOfBytes, string spliter = " ")
            {
                int lenght = AmmountOfBytes * 2;
                int casing = 1;
                int content = 0;
                int CurLenght = 0;
                char[] LowAlph = "abcdef".ToCharArray();
                char[] UppAlph = "ABCDEF".ToCharArray();
                _ = "1234567890".ToCharArray();
                char addLetter = 'a';
                string gString = "";
                _ = "abcdef01234567890".ToCharArray();
                Random r = new Random();
                Random r1 = new Random();
                _ = new Random();
                while (gString.Length != lenght)
                {
                    switch (casing)
                    {
                        case 0:
                            switch (r.Next(0, 2))
                            {
                                case 0:
                                    addLetter = LowAlph[r1.Next(0, 5)];
                                    break;
                                case 1:
                                    addLetter = UppAlph[r1.Next(0, 5)];
                                    break;
                            }
                            break;
                        case 1:
                            addLetter = UppAlph[r1.Next(0, 5)];
                            break;
                        case 2:
                            addLetter = LowAlph[r1.Next(0, 5)];
                            break;
                    }
                    switch (content)
                    {
                        case 0:
                            switch (r.Next(0, 2))
                            {
                                case 0:
                                    gString += addLetter;
                                    CurLenght++;
                                    break;
                                case 1:
                                    gString += r1.Next(0, 9);
                                    CurLenght++;
                                    break;
                            }
                            break;
                        case 1:
                            gString += r1.Next(0, 9);
                            CurLenght++;
                            break;
                        case 2:
                            gString += addLetter;
                            CurLenght++;
                            break;
                    }
                    if (CurLenght == 2)
                    {
                        gString += spliter;
                        CurLenght = 0;
                        lenght++;
                    }
                }
                return gString.Substring(0, gString.Length - 1);
            }
            /// <summary>
            /// Generates a random string
            /// </summary>
            /// <param name="lenght">The string lenght</param>
            /// <param name="content">0 = Numbers and Letters; 1 = Only Numbers; 2 = Only Letters</param>
            /// <param name="casing">0 = Random casing; 1 = Only Uppercase; 2 = Only Lowercase</param>
            public static string RandomString(int lenght, int content, int casing)
            {
                char[] LowAlph = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                char[] UppAlph = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                _ = "1234567890".ToCharArray();
                char addLetter = 'a';
                string gString = "";
                _ = "abcdefghijklmnopqrstuvwxyz01234567890".ToCharArray();
                Random r = new Random();
                Random r1 = new Random();
                _ = new Random();
                while (gString.Length != lenght)
                {
                    switch (casing)
                    {
                        case 0:
                            switch (r.Next(0, 2))
                            {
                                case 0:
                                    addLetter = LowAlph[r1.Next(0, 25)];
                                    break;
                                case 1:
                                    addLetter = UppAlph[r1.Next(0, 25)];
                                    break;
                            }
                            break;
                        case 1:
                            addLetter = UppAlph[r1.Next(0, 25)];
                            break;
                        case 2:
                            addLetter = LowAlph[r1.Next(0, 25)];
                            break;
                    }
                    switch (content)
                    {
                        case 0:
                            switch (r.Next(0, 2))
                            {
                                case 0:
                                    gString += addLetter;
                                    break;
                                case 1:
                                    gString += r1.Next(0, 9);
                                    break;
                            }
                            break;
                        case 1:
                            gString += r1.Next(0, 9);
                            break;
                        case 2:
                            gString += addLetter;
                            break;
                    }
                }
                return gString;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="lenghtPerPart"> The lenght of each key part (total of 3 parts)</param>
            /// <param name="keyContent">0 = Numbers and letters; 1 = Numbers Only; 2 = Letters Only</param>
            /// <returns></returns>
            public static string LicenseKey(int lenghtPerPart, int keyContent)
            {
                string part1 = RandomString(lenghtPerPart, keyContent, 1);
                string rd1 = RandomString(50, keyContent, 1);
                string rd2 = RandomString(120, keyContent, 1);
                string part2 = rd1.Substring(rd1.Length - lenghtPerPart);
                string part3 = rd1.Substring(rd1.Length - lenghtPerPart);
                return part1 + "-" + part2 + "-" + part3;
            }
        }
        public static class Maths
        {
            public static string GetFraction(double val)
            {
                for (double i = 1; i <= (int)(val * 50 + 1); i++)
                    for (double j = 1; j <= (int)(val * 50 + 1); j++)
                        if (j / i == val)
                            return j.ToString() + "/" + i.ToString();
                return "0";
            }
            public static string[] GetFraction(double val, bool _)
            {
                List<string> rets = new List<string>();
                for (double i = 1; i <= 5000; i++)
                    for (double j = 1; j <= 5000; j++)
                        if (j / i == val)
                            rets.Add(j.ToString() + "/" + i.ToString());
                return rets.ToArray();
            }
            public static string[] GetFraction(double val, int rangeMultiplier = 50)
            {
                List<string> rets = new List<string>();
                for (double i = 1; i <= (int)(val * rangeMultiplier); i++)
                    for (double j = 1; j <= (int)(val * rangeMultiplier); j++)
                        if (j / i == val)
                            rets.Add(j.ToString() + "/" + i.ToString());
                return rets.ToArray();
            }
            public static class RelativeFunction
            {
                private static bool IsCondition(int x, int y, string condition)
                {
                    int xs = (int)Maths.MathParser.Parse(condition.Split('=')[0].Replace("x", x.ToString()));
                    int ys = (int)Maths.MathParser.Parse(condition.Split('=')[1].Replace("y", y.ToString()));
                    return xs == ys;
                }
                public static string Parse(Point[] groups)
                {
                    string r = "{";
                    foreach (var g in groups)
                        r += " (" + g.X.ToString() + ", " + g.Y.ToString() + ")";
                    return r + " }";
                }
                public static int[] Parse(string RelativeFunction)
                {
                    List<int> arr = new List<int>();
                    foreach (var v in RelativeFunction.Replace("{", "").Replace("}", "").Split(','))
                        arr.Add(int.Parse(v.Replace(" ", "")));
                    return arr.ToArray();
                }
                public static Point[] Solve(int[] A, int[] B, string condition)
                {
                    List<Point> groups = new List<Point>();
                    foreach (var av in A)
                        foreach (var bv in B)
                            if (IsCondition(av, bv, condition))
                                groups.Add(new Point(av, bv));
                    return groups.ToArray();
                }
            }
            public static double[] Baskara(int a, int b, int c)
            {
                return Baskara(double.Parse(a.ToString()), double.Parse(b.ToString()), double.Parse(c.ToString()));
            }
            public static double[] Baskara(double a, double b, double c)
            {
                // Delta = bº - 4 * a * c
                // Baskara = ( - b +- sqrt(delta)) / 2

                var delta = Math.Pow(b, 2) - 4 * a * c;
                if (delta > 0)
                {
                    var l = (new double[] { double.Parse(((-b + Math.Sqrt(delta)) / (2 * a)).ToString()), double.Parse(((-b - Math.Sqrt(delta)) / (2 * a)).ToString()) }).ToList();
                    l.Sort();
                    return l.ToArray() as double[];
                }
                else if (delta == 0)
                {
                    var l = (new double[] { double.Parse(((-b + Math.Sqrt(delta)) / (2 * a)).ToString()) }).ToList();
                    l.Sort();
                    return l.ToArray() as double[];
                }
                else
                {
                    return new double[0];
                }
            }
#pragma warning disable IDE0060 // Remover o parâmetro não utilizado
            public static object Baskara(int a, int b, int c, bool AsString)
#pragma warning restore IDE0060 // Remover o parâmetro não utilizado
            {
                return Baskara(double.Parse(a.ToString()), double.Parse(b.ToString()), double.Parse(c.ToString()));
            }
            public static object Baskara(double a, double b, double c, bool AsString)
            {
                var bask = Baskara(a, b, c);
                if (AsString)
                {
                    var bRes = "S = {";
                    foreach (var br in bask)
                    {
                        bRes += " " + br.ToString() + "; ";
                    }
                    bRes = bRes.Substring(0, bRes.Length - 2);
                    return bRes + " }";
                }
                else
                {
                    return bask;
                }
            }
            public static string XOR(string[] bytes, byte initial = 0x00)
            {
                Byte _CheckSum = initial;
                for (int i = 0; i < bytes.Length; i++)
                {
                    var cB = Convert.ToByte(Converter.HexadecimalToDecimal(bytes[i]));
                    _CheckSum ^= cB;
                }
                return Converter.DecimalToHexadecimal(int.Parse(_CheckSum.ToString()));
            }
            public static class Percentage
            {
                public static double NumberToPercent(double TotalNumber, double Number)
                {
                    return TotalNumber / Number * 100.0;
                }
                public static double PercentToNumber(double TotalNumber, double Percentage)
                {
                    return TotalNumber * Percentage / 100.0;
                }
            }
            /// <summary>
            /// Goes through each number in the given number and multiplies them
            /// </summary>
            /// <param name="Number">The given number</param>
            /// <returns>Returns the result</returns>
            public static long MultiplyChars(long Number)
            {
                long result = 1;
                for (long i = 0; i < Number.ToString().Length; i++)
                {
                    int m = int.Parse(Number.ToString().ToCharArray()[i].ToString());
                    result *= m;
                }
                return result;
            }
            /// <summary>
            /// Gets the multiplicative persistence of a 'x' number and returns a list wich index 0 = the number of steps and the rest are the results
            /// </summary>
            /// <param name="Number">The number to get the multiplicative persistence.</param>
            /// <returns></returns>
            public static List<ulong> GetMultiplicativePersistence(ulong Number)
            {
                var res = new List<ulong>
            {
                0
            };
                ulong steps = 0;
                ulong results = Number;
                while (results.ToString().Length != 1)
                {
                    ulong tmpRes = 1;
                    for (ulong i = 0; i < ulong.Parse(results.ToString().Length.ToString()); i++)
                    {
                        ulong m = ulong.Parse(results.ToString().ToCharArray()[i].ToString());
                        tmpRes *= m;
                    }
                    results = tmpRes;
                    res.Add(results);
                    steps++;
                }
                res[0] = steps;
                return res;
            }
            public static int[] GetMultipliers(int number)
            {
                List<int> mults = new List<int>();
                for (int i = 1; i <= number; i++)
                {
                    if (number % i == 0)
                    {
                        mults.Add(i);
                    }
                }
                return mults.ToArray();
            }
            public static bool IsPrime(int number)
            {
                int[] a = GetPrimeNumbers(number);
                return a.Contains(number);
            }
            public static int[] GetPrimeNumbers(int number)
            {
                List<int> arr = new List<int>();
                for (int j = 2; j < number; j++)
                {
                    for (int i = 2; i < j && j % i != 0; i++)
                    {
                        if (j == i + 1)
                        {
                            arr.Add(j);
                        }
                    }
                }
                return arr.ToArray();
            }
            public class MathParser
            {
                #region Properties

                /// <summary>
                /// All operators that you want to define should be inside this property.
                /// </summary>
                public Dictionary<string, Func<double, double, double>> Operators { get; set; }

                /// <summary>
                /// All functions that you want to define should be inside this property.
                /// </summary>
                public Dictionary<string, Func<double[], double>> LocalFunctions { get; set; }

                /// <summary>
                /// All variables that you want to define should be inside this property.
                /// </summary>
                public Dictionary<string, double> LocalVariables { get; set; }

                /// <summary>
                /// When converting the result from the Parse method or ProgrammaticallyParse method ToString(),
                /// please use this culture info.
                /// </summary>
                public CultureInfo CultureInfo { get; }

                #endregion

                /// <summary>
                /// Initializes a new instance of the MathParser class, and optionally with
                /// predefined functions, operators, and variables.
                /// </summary>
                /// <param name="loadPreDefinedFunctions">This will load abs, cos, cosh, arccos, sin, sinh, arcsin, tan, tanh, arctan, sqrt, rem, and round.</param>
                /// <param name="loadPreDefinedOperators">This will load %, *, :, /, +, -, >, &lt;, and =</param>
                /// <param name="loadPreDefinedVariables">This will load pi, tao, e, phi, major, minor, pitograd, and piofgrad.</param>
                /// <param name="cultureInfo">The culture info to use when parsing. If null, defaults to invariant culture.</param>
                public MathParser(bool loadPreDefinedFunctions = true, bool loadPreDefinedOperators = true, bool loadPreDefinedVariables = true, CultureInfo cultureInfo = null)
                {
                    if (loadPreDefinedOperators)
                    {
                        Operators = new Dictionary<string, Func<double, double, double>>(10)
                        {
                            ["^"] = System.Math.Pow,
                            ["%"] = (a, b) => a % b,
                            [":"] = (a, b) => a / b,
                            ["/"] = (a, b) => a / b,
                            ["*"] = (a, b) => a * b,
                            ["-"] = (a, b) => a - b,
                            ["+"] = (a, b) => a + b,

                            [">"] = (a, b) => a > b ? 1 : 0,
                            ["<"] = (a, b) => a < b ? 1 : 0,
                            ["="] = (a, b) => System.Math.Abs(a - b) < 0.00000001 ? 1 : 0
                        };
                    }
                    else
                        Operators = new Dictionary<string, Func<double, double, double>>();

                    if (loadPreDefinedFunctions)
                    {
                        LocalFunctions = new Dictionary<string, Func<double[], double>>(26)
                        {
                            ["abs"] = inputs => System.Math.Abs(inputs[0]),

                            ["cos"] = inputs => System.Math.Cos(inputs[0]),
                            ["cosh"] = inputs => System.Math.Cosh(inputs[0]),
                            ["acos"] = inputs => System.Math.Acos(inputs[0]),
                            ["arccos"] = inputs => System.Math.Acos(inputs[0]),

                            ["sin"] = inputs => System.Math.Sin(inputs[0]),
                            ["sinh"] = inputs => System.Math.Sinh(inputs[0]),
                            ["asin"] = inputs => System.Math.Asin(inputs[0]),
                            ["arcsin"] = inputs => System.Math.Asin(inputs[0]),

                            ["tan"] = inputs => System.Math.Tan(inputs[0]),
                            ["tanh"] = inputs => System.Math.Tanh(inputs[0]),
                            ["atan"] = inputs => System.Math.Atan(inputs[0]),
                            ["arctan"] = inputs => System.Math.Atan(inputs[0]),

                            ["sqrt"] = inputs => System.Math.Sqrt(inputs[0]),
                            ["pow"] = inputs => System.Math.Pow(inputs[0], inputs[1]),
                            ["root"] = inputs => System.Math.Pow(inputs[0], 1 / inputs[1]),
                            ["rem"] = inputs => System.Math.IEEERemainder(inputs[0], inputs[1]),

                            ["sign"] = inputs => System.Math.Sign(inputs[0]),
                            ["exp"] = inputs => System.Math.Exp(inputs[0]),

                            ["floor"] = inputs => System.Math.Floor(inputs[0]),
                            ["ceil"] = inputs => System.Math.Ceiling(inputs[0]),
                            ["ceiling"] = inputs => System.Math.Ceiling(inputs[0]),
                            ["round"] = inputs => System.Math.Round(inputs[0]),
                            ["truncate"] = inputs => inputs[0] < 0 ? -System.Math.Floor(-inputs[0]) : System.Math.Floor(inputs[0]),

                            ["log"] = inputs =>
                            {
                                switch (inputs.Length)
                                {
                                    case 1:
                                        return System.Math.Log10(inputs[0]);
                                    case 2:
                                        return System.Math.Log(inputs[0], inputs[1]);
                                    default:
                                        return 0;
                                }
                            },

                            ["ln"] = inputs => System.Math.Log(inputs[0])
                        };
                    }
                    else
                        LocalFunctions = new Dictionary<string, Func<double[], double>>();

                    if (loadPreDefinedVariables)
                    {
                        LocalVariables = new Dictionary<string, double>(8)
                        {
                            ["pi"] = 3.14159265358979,
                            ["tao"] = 6.28318530717959,

                            ["e"] = 2.71828182845905,
                            ["phi"] = 1.61803398874989,
                            ["major"] = 0.61803398874989,
                            ["minor"] = 0.38196601125011,

                            ["pitograd"] = 57.2957795130823,
                            ["piofgrad"] = 0.01745329251994
                        };
                    }
                    else
                        LocalVariables = new Dictionary<string, double>();

                    CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture;
                }

                /// <summary>
                /// Enter the math expression in form of a string.
                /// </summary>
                /// <param name="mathExpression">The math expression to parse.</param>
                /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
                public static double Parse(string mathExpression)
                {
                    var parser = new MathParser();
                    return parser.MathParserLogic(parser.Lexer(mathExpression));
                }

                /// <summary>
                /// Enter the math expression in form of a list of tokens.
                /// </summary>
                /// <param name="mathExpression">The math expression to parse.</param>
                /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
                public static double Parse(ReadOnlyCollection<string> mathExpression)
                {
                    var parser = new MathParser();
                    return parser.MathParserLogic(new List<string>(mathExpression));
                }

                /// <summary>
                /// Enter the math expression in form of a string. You might also add/edit variables using "let" keyword.
                /// For example, "let sampleVariable = 2+2".
                /// 
                /// Another way of adding/editing a variable is to type "varName := 20"
                /// 
                /// Last way of adding/editing a variable is to type "let varName be 20"
                /// </summary>
                /// <param name="mathExpression">The math expression to parse.</param>
                /// <param name="correctExpression">If true, correct <paramref name="correctExpression"/> of any typos.</param>
                /// <param name="identifyComments">If true, treat "#", "#{", and "}#" as comments.</param>
                /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
                public double ProgrammaticallyParse(string mathExpression, bool correctExpression = true, bool identifyComments = true)
                {
                    if (identifyComments)
                    {
                        // Delete Comments #{Comment}#
                        mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#\\{.*?\\}#", "");

                        // Delete Comments #Comment
                        mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#.*$", "");
                    }

                    if (correctExpression)
                    {
                        // this refers to the Correction function which will correct stuff like artn to arctan, etc.
                        mathExpression = Correction(mathExpression);
                    }

                    string varName;
                    double varValue;

                    if (mathExpression.Contains("let"))
                    {
                        if (mathExpression.Contains("be"))
                        {
                            varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                                mathExpression.IndexOf("be", StringComparison.Ordinal) -
                                mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                            mathExpression = mathExpression.Replace(varName + "be", "");
                        }
                        else
                        {
                            varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                                mathExpression.IndexOf("=", StringComparison.Ordinal) -
                                mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                            mathExpression = mathExpression.Replace(varName + "=", "");
                        }

                        varName = varName.Replace(" ", "");
                        mathExpression = mathExpression.Replace("let", "");

                        varValue = Parse(mathExpression);

                        if (LocalVariables.ContainsKey(varName))
                            LocalVariables[varName] = varValue;
                        else
                            LocalVariables.Add(varName, varValue);

                        return varValue;
                    }

                    if (!mathExpression.Contains(":="))
                        return Parse(mathExpression);

                    //mathExpression = mathExpression.Replace(" ", ""); // remove white space
                    varName = mathExpression.Substring(0, mathExpression.IndexOf(":=", StringComparison.Ordinal));
                    mathExpression = mathExpression.Replace(varName + ":=", "");

                    varValue = Parse(mathExpression);
                    varName = varName.Replace(" ", "");

                    if (LocalVariables.ContainsKey(varName))
                        LocalVariables[varName] = varValue;
                    else
                        LocalVariables.Add(varName, varValue);

                    return varValue;
                }

                /// <summary>
                /// This will convert a string expression into a list of tokens that can be later executed by Parse or ProgrammaticallyParse methods.
                /// </summary>
                /// <param name="mathExpression">The math expression to tokenize.</param>
                /// <returns>The resulting tokens of <paramref name="mathExpression"/>.</returns>
                public ReadOnlyCollection<string> GetTokens(string mathExpression)
                {
                    return Lexer(mathExpression).AsReadOnly();
                }

                #region Core

                /// <summary>
                /// This will correct sqrt() and arctan() written in different ways only.
                /// </summary>
                /// <param name="input"></param>
                /// <returns></returns>
                private string Correction(string input)
                {
                    // Word corrections

                    input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(sqr|sqrt)\\b", "sqrt",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(atan2|arctan2)\\b", "arctan2",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    //... and more

                    return input;
                }

                /// <summary>
                /// Tokenizes <paramref name="expr"/>.
                /// </summary>
                /// <param name="expr">The expression to tokenize.</param>
                /// <returns>The tokens.</returns>
                private List<string> Lexer(string expr)
                {
                    var token = "";
                    var tokens = new List<string>();

                    expr = expr.Replace("+-", "-");
                    expr = expr.Replace("-+", "-");
                    expr = expr.Replace("--", "+");

                    for (var i = 0; i < expr.Length; i++)
                    {
                        var ch = expr[i];

                        if (char.IsWhiteSpace(ch))
                            continue;

                        if (char.IsLetter(ch))
                        {
                            if (i != 0 && (char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                                tokens.Add("*");

                            token += ch;

                            while (i + 1 < expr.Length && char.IsLetterOrDigit(expr[i + 1]))
                                token += expr[++i];

                            tokens.Add(token);
                            token = "";

                            continue;
                        }

                        if (char.IsDigit(ch))
                        {
                            token += ch;

                            while (i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.'))
                                token += expr[++i];

                            tokens.Add(token);
                            token = "";

                            continue;
                        }

                        if (ch == '.')
                        {
                            token += ch;

                            while (i + 1 < expr.Length && char.IsDigit(expr[i + 1]))
                                token += expr[++i];

                            tokens.Add(token);
                            token = "";

                            continue;
                        }

                        if (i + 1 < expr.Length && (ch == '-' || ch == '+') && char.IsDigit(expr[i + 1]) &&
                            (i == 0 || Operators.ContainsKey(expr[i - 1].ToString(
#if !NETSTANDARD1_4
                        CultureInfo
#endif
                        )) ||
                             i - 1 > 0 && expr[i - 1] == '('))
                        {
                            // if the above is true, then the token for that negative number will be "-1", not "-","1".
                            // to sum up, the above will be true if the minus sign is in front of the number, but
                            // at the beginning, for example, -1+2, or, when it is inside the brakets (-1).
                            // NOTE: this works for + as well!

                            token += ch;

                            while (i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.'))
                                token += expr[++i];

                            tokens.Add(token);
                            token = "";

                            continue;
                        }

                        if (ch == '(')
                        {
                            if (i != 0 && (char.IsDigit(expr[i - 1]) || char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                            {
                                tokens.Add("*");
                                tokens.Add("(");
                            }
                            else
                                tokens.Add("(");
                        }
                        else
                            tokens.Add(ch.ToString());
                    }

                    return tokens;
                }

                private double MathParserLogic(List<string> tokens)
                {
                    // Variables replacement
                    for (var i = 0; i < tokens.Count; i++)
                    {
                        if (LocalVariables.Keys.Contains(tokens[i]))
                            tokens[i] = LocalVariables[tokens[i]].ToString(CultureInfo);
                    }

                    while (tokens.IndexOf("(") != -1)
                    {
                        // getting data between "(" and ")"
                        var open = tokens.LastIndexOf("(");
                        var close = tokens.IndexOf(")", open); // in case open is -1, i.e. no "(" // , open == 0 ? 0 : open - 1

                        if (open >= close)
                            throw new ArithmeticException("No closing bracket/parenthesis. Token: " + open.ToString(CultureInfo));

                        var roughExpr = new List<string>();

                        for (var i = open + 1; i < close; i++)
                            roughExpr.Add(tokens[i]);

                        double tmpResult;

                        var args = new List<double>();
                        var functionName = tokens[open == 0 ? 0 : open - 1];

                        if (LocalFunctions.Keys.Contains(functionName))
                        {
                            if (roughExpr.Contains(","))
                            {
                                // converting all arguments into a decimal array
                                for (var i = 0; i < roughExpr.Count; i++)
                                {
                                    var defaultExpr = new List<string>();
                                    var firstCommaOrEndOfExpression =
                                        roughExpr.IndexOf(",", i) != -1
                                            ? roughExpr.IndexOf(",", i)
                                            : roughExpr.Count;

                                    while (i < firstCommaOrEndOfExpression)
                                        defaultExpr.Add(roughExpr[i++]);

                                    args.Add(defaultExpr.Count == 0 ? 0 : BasicArithmeticalExpression(defaultExpr));
                                }

                                // finally, passing the arguments to the given function
                                tmpResult = double.Parse(LocalFunctions[functionName](args.ToArray()).ToString(CultureInfo), CultureInfo);
                            }
                            else
                            {
                                // but if we only have one argument, then we pass it directly to the function
                                tmpResult = double.Parse(LocalFunctions[functionName](new[]
                                {
                            BasicArithmeticalExpression(roughExpr)
                        }).ToString(CultureInfo), CultureInfo);
                            }
                        }
                        else
                        {
                            // if no function is need to execute following expression, pass it
                            // to the "BasicArithmeticalExpression" method.
                            tmpResult = BasicArithmeticalExpression(roughExpr);
                        }

                        // when all the calculations have been done
                        // we replace the "opening bracket with the result"
                        // and removing the rest.
                        tokens[open] = tmpResult.ToString(CultureInfo);
                        tokens.RemoveRange(open + 1, close - open);

                        if (LocalFunctions.Keys.Contains(functionName))
                        {
                            // if we also executed a function, removing
                            // the function name as well.
                            tokens.RemoveAt(open - 1);
                        }
                    }

                    // at this point, we should have replaced all brackets
                    // with the appropriate values, so we can simply
                    // calculate the expression. it's not so complex
                    // any more!
                    return BasicArithmeticalExpression(tokens);
                }

                private double BasicArithmeticalExpression(List<string> tokens)
                {
                    // PERFORMING A BASIC ARITHMETICAL EXPRESSION CALCULATION
                    // THIS METHOD CAN ONLY OPERATE WITH NUMBERS AND OPERATORS
                    // AND WILL NOT UNDERSTAND ANYTHING BEYOND THAT.

                    switch (tokens.Count)
                    {
                        case 1:
                            return double.Parse(tokens[0], CultureInfo);
                        case 2:
                            var op = tokens[0];

                            if (op == "-" || op == "+")
                            {
                                var first = op == "+" ? "" : (tokens[1].Substring(0, 1) == "-" ? "" : "-");

                                return double.Parse(first + tokens[1], CultureInfo);
                            }

                            return Operators[op](0, double.Parse(tokens[1], CultureInfo));
                        case 0:
                            return 0;
                    }

                    foreach (var op in Operators)
                    {
                        int opPlace;

                        while ((opPlace = tokens.IndexOf(op.Key)) != -1)
                        {
                            var rhs = double.Parse(tokens[opPlace + 1], CultureInfo);

                            if (op.Key == "-" && opPlace == 0)
                            {
                                var result = op.Value(0.0, rhs);
                                tokens[0] = result.ToString(CultureInfo);
                                tokens.RemoveRange(opPlace + 1, 1);
                            }
                            else
                            {
                                var result = op.Value(double.Parse(tokens[opPlace - 1], CultureInfo), rhs);
                                tokens[opPlace - 1] = result.ToString(CultureInfo);
                                tokens.RemoveRange(opPlace, 2);
                            }
                        }
                    }

                    return double.Parse(tokens[0], CultureInfo);
                }

                #endregion
            }
            public static double SolveEquation(string expression, string result, int limit = 100)
            {
                double addN = 0.01f;
                for (double i = 0; i < limit; i += addN)
                {
                    string newExp = expression.ToLower().Replace("x", System.Math.Round(i, 2).ToString());
                    double res = System.Math.Round(MathParser.Parse((newExp.Replace(",", "."))), 2);
                    if (res == System.Math.Round(MathParser.Parse(result), 2))
                    {
                        return System.Math.Round(i, 5);
                    }
                }
                return 0f;
            }
            public static bool IsPair(int Number)
            {
                var res = Number % 2;
                if (res != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public static bool IsPair(double Number)
            {
                var res = Number % 2;
                if (res != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public static bool IsPair(float Number)
            {
                var res = Number % 2;
                if (res != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            /// <summary>Returns a specified number raised to the specified power.</summary>
            /// <returns>The number <paramref name="x" /> raised to the power <paramref name="y" />.</returns>
            /// <param name="x">A double-precision floating-point number to be raised to a power. </param>
            /// <param name="y">A double-precision floating-point number that specifies a power. </param>
            public static double Pow(double x, double y)
            {
                double result = 1d;
                for (int i = 1; i <= y; i++)
                {
                    result = x * result;
                }
                return result;
            }
            public static double RootOf(int NumberToGetRoot, int Index = 2, int DecimalPlaces = 0)
            {
                if (NumberToGetRoot < 0 && Index % 2 == 0)
                {
                    return 0d;
                }
                string Root = "0";
                int dPlaces = 0;
                //double addN = 0;
                string saddN = "0,1";
                for (int i = 0; i <= NumberToGetRoot; i++)
                {
                    if (Math.Pow(i + 1, Index) > NumberToGetRoot)
                    {
                        Root = i.ToString();
                        if (DecimalPlaces != 0) { Root += ","; }
                        break;
                    }
                }
                if (Math.Pow(double.Parse(Root), Index) == NumberToGetRoot)
                    return double.Parse(Root.Replace(",", ""));
                while (dPlaces < DecimalPlaces)
                {
                    for (double i = double.Parse(saddN); i < NumberToGetRoot; i += double.Parse(saddN))
                    {
                        var next = i + double.Parse(saddN);
                        var sum = next + double.Parse(Root);
                        var power = Math.Pow(sum, Index);
                        if (power > NumberToGetRoot)
                        {
                            Root += (sum - double.Parse(saddN)).ToString().Last().ToString();
                            saddN = saddN.Replace("1", "01");
                            dPlaces++;
                            break;
                        }
                    }
                }
                return double.Parse(Root);
            }
            public static string RootOfAsString(int NumberToGetRoot, int Index = 2, int DecimalPlaces = 0)
            {
                if (NumberToGetRoot < 0 && Index % 2 == 0)
                {
                    return "0";
                }
                string Root = "0";
                int dPlaces = 0;
                //double addN = 0;
                string saddN = "0,1";
                for (int i = 0; i <= NumberToGetRoot; i++)
                {
                    if (Math.Pow(i + 1, Index) > NumberToGetRoot)
                    {
                        Root = i.ToString();
                        if (DecimalPlaces != 0) { Root += ","; }
                        break;
                    }
                }
                if (Math.Pow(double.Parse(Root), Index) == NumberToGetRoot)
                    return Root.Replace(",", "");
                while (dPlaces < DecimalPlaces)
                {
                    for (double i = double.Parse(saddN); i < NumberToGetRoot; i += double.Parse(saddN))
                    {
                        var next = i + double.Parse(saddN);
                        var sum = next + double.Parse(Root);
                        var power = Math.Pow(sum, Index);
                        if (power > NumberToGetRoot)
                        {
                            Root += (sum - double.Parse(saddN)).ToString().Last().ToString();
                            saddN = saddN.Replace("1", "01");
                            dPlaces++;
                            break;
                        }
                    }
                }
                return Root;
            }
            public static string XOR(string HexNumber1, string HexNumber2)
            {
                string[] Hbin1;
                string[] Hbin2;
                if (Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length > Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length)
                {
                    Hbin1 = new string[Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length];
                    Hbin2 = new string[Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length];
                }
                else
                {
                    Hbin1 = new string[Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length];
                    Hbin2 = new string[Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length];
                }
                int i = 0;
                foreach (char bit in Converter.HexadecimalToBinary(HexNumber1).ToCharArray())
                {
                    Hbin1[i] = bit.ToString();
                    i++;
                }
                i = 0;
                foreach (char bit in Converter.HexadecimalToBinary(HexNumber2).ToCharArray())
                {
                    if (i == Hbin2.Length)
                    {

                    }
                    else
                    {
                        Hbin2[i] = bit.ToString();
                        i++;
                    }
                }
                i = 0;
                string BinaryResult = null;
                string Hbin1Str = null;
                string Hbin2Str = null;
                foreach (string bit in Hbin1)
                {
                    Hbin1Str += bit;
                }
                foreach (string bit in Hbin2)
                {
                    Hbin2Str += bit;
                }
                string count1 = "0";
                foreach (string bit in Hbin1)
                {
                    if (bit != null)
                    {
                        count1 = Convert.ToString(Convert.ToInt64(count1) + 1);
                    }
                }
                string count2 = "0";
                foreach (string bit in Hbin2)
                {
                    if (bit != null)
                    {
                        count2 = Convert.ToString(Convert.ToInt64(count2) + 1);
                    }
                }
                if (Convert.ToInt32(count1) > Convert.ToInt32(count2))
                {
                    while (Hbin2Str.Length != Hbin1Str.Length)
                    {
                        Hbin2Str = Hbin2Str.Insert(0, "0");
                    }
                }
                else
                {
                    while (Hbin1Str.Length != Hbin2Str.Length)
                    {
                        Hbin1Str = Hbin1Str.Insert(0, "0");
                    }
                }
                foreach (char bit in Hbin1Str.ToCharArray())
                {
                    if (bit.ToString() == Hbin2Str.ToCharArray()[i].ToString())
                    {
                        string temp = Hbin1Str;
                        string tmp = Hbin2Str;
                        string tp = Hbin2Str.ToCharArray()[i].ToString();
                        BinaryResult += "0";
                        string z = temp + tmp + BinaryResult + bit + tp;
                    }
                    else
                    {
                        string temp = Hbin1Str;
                        string tmp = Hbin2Str;
                        string tp = Hbin2Str.ToCharArray()[i].ToString();
                        BinaryResult += "1";
                        string z = temp + tmp + BinaryResult + bit + tp;
                    }
                    i++;
                }
                return Converter.BinaryToHexadecimal(BinaryResult);
            }
            private static string FromRight(string str, int lenght) => str.Substring(str.Length - lenght);
            public static int MakeNumberPositive(int Number)
            {
                return int.Parse(FromRight(Number.ToString(), Number.ToString().Length - 1));
            }
            public static int GetNumberFactorial(int Number)
            {
                int Factorial = 1;
                for (int i = Number; i > 1; i--)
                    Factorial *= i;
                return Factorial;
            }
        }
        namespace IO
        {
            public class DirectoryCopy
            {
                public DirectoryCopy(string src, string dest)
                {
                    Src = src;
                    Dest = dest;
                    new Thread(IndexFiles).Start();
                    new Thread(IndexDirectories).Start();
                }
                List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
                List<System.IO.DirectoryInfo> directories = new List<System.IO.DirectoryInfo>();
                bool doneFiles = false;
                bool doneDirs = false;
                public event ProgressChangedEventHandler CopyComplete;
                public event ProgressChangedEventHandler CopyProgressChanged;
                public string CurrentFileName { get; private set; }
                public int MaxValue = -1;
                string GetNewPath(System.IO.FileInfo src)
                {
                    var sdir = new System.IO.DirectoryInfo(Src);
                    return System.IO.Path.Combine(new System.IO.DirectoryInfo(Dest).FullName, src.FullName.Substring(sdir.FullName.Length + 1));
                }
                string GetNewPath(System.IO.DirectoryInfo src)
                {
                    var sdir = new System.IO.DirectoryInfo(Src);
                    return System.IO.Path.Combine(new System.IO.DirectoryInfo(Dest).FullName, src.FullName.Substring(sdir.FullName.Length + 1));
                }
                void Update(int percentage)
                {
                    currentProgress = percentage;
                    CopyProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, CurrentFileName));
                }
                void Complete()
                {
                    CopyComplete?.Invoke(this, new ProgressChangedEventArgs(100, "complete"));
                }
                void IndexFiles()
                {
                    files = new System.IO.DirectoryInfo(Src).GetAllFiles().ToList();
                    doneFiles = true;
                }

                void IndexDirectories()
                {
                    directories = new System.IO.DirectoryInfo(Src).GetAllDirectories().ToList();
                    doneDirs = true;
                }

                void CopyDir(object recursive)
                {
                    while (doneDirs == false || doneFiles == false) { Thread.Sleep(50); }
                    MaxValue = directories.Count + files.Count * 100;
                    if (!System.IO.Directory.Exists(Dest))
                        System.IO.Directory.CreateDirectory(Dest);
                    else
                    {
                        if ((bool)recursive == true)
                        { System.IO.Directory.Delete(Dest, true); System.IO.Directory.CreateDirectory(Dest); }
                        else
                            try { System.IO.Directory.Delete(Dest); } catch { throw; }
                    }
                    currentProgress = 0;
                    foreach (var dir in directories)
                    {
                        System.IO.Directory.CreateDirectory(GetNewPath(dir));
                        currentProgress++;
                        Update(currentProgress);
                    }
                    System.Threading.Thread.Sleep(500);
                    var fixedProgress = 0;
                    foreach (var file in files)
                    {
                        fixedProgress = currentProgress;
                        System.IO.FileStream fsrc = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open);
                        System.IO.FileStream fout = new System.IO.FileStream(GetNewPath(file), System.IO.FileMode.Create);
                        byte[] bt = new byte[1048756];
                        int readByte;
                        while ((readByte = fsrc.Read(bt, 0, bt.Length)) > 0)
                        {
                            fout.Write(bt, 0, readByte);
                            Update(fixedProgress + (int)(fsrc.Position * 100 / fsrc.Length));
                        }
                        fsrc.Close();
                        fout.Close();
                    }
                    CopyComplete?.Invoke(this, new ProgressChangedEventArgs(currentProgress, "complete"));
                }
                int currentProgress = 0;
                private void File_CopyProgressChanged(object sender, ProgressChangedEventArgs e)
                {
                    Update(currentProgress + e.ProgressPercentage);
                }

                void CopyDir() => CopyDir(false);
                public void CopyAsync() => new Thread(new ThreadStart(CopyDir)).Start();
                public void CopyAsync(bool recursive) => new Thread(new ParameterizedThreadStart(CopyDir)).Start(recursive);
                public string Src { get; }
                public string Dest { get; }
            }
            public class FileCopy
            {
                public FileCopy(string src, string dest)
                {
                    Src = src;
                    Dest = dest;
                }
                public event ProgressChangedEventHandler CopyProgressChanged;
                public event ProgressChangedEventHandler CopyComplete;
                public void CopyAsync() => new Thread(CopyFile).Start();
                void CopyFile()
                {
                    System.IO.FileStream fout = new System.IO.FileStream(Dest, System.IO.FileMode.Create);
                    System.IO.FileStream fsrc = new System.IO.FileStream(Src, System.IO.FileMode.Open);
                    byte[] bt = new byte[1048756];
                    int readByte;
                    while ((readByte = fsrc.Read(bt, 0, bt.Length)) > 0)
                    {
                        fout.Write(bt, 0, readByte);
                        Update((int)(fsrc.Position * 100 / fsrc.Length));
                    }
                    fsrc.Close();
                    fout.Close();
                    Done();
                }
                private void Update(int percentage)
                {
                    CopyProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, "copying"));
                }
                private void Done()
                {
                    CopyComplete?.Invoke(this, new ProgressChangedEventArgs(100, "complete"));
                }
                //public string SrcName { get => new System.IO.FileInfo(Src).Name; }
                //public string DestName { get => new System.IO.FileInfo(Dest).Name; }
                public string Src { get; }
                public string Dest { get; }
            }
            public class HexDumpStream
            {
                readonly string file = null;
                private System.IO.FileStream OpenRead => System.IO.File.Open(file, System.IO.FileMode.Open);
                private System.IO.FileStream OpenWrite => System.IO.File.Open(file, System.IO.FileMode.OpenOrCreate);
                public HexDumpStream(string File)
                {
                    file = File;
                }
                public string Checksum(int type, int StartAddress, int EndAddress)
                {
                    string Data = ReadMultipleHexValues(StartAddress, EndAddress);
                    int result = 0;
                    foreach (string bt in Data.Split('-'))
                    {
                        int Dec = Converter.HexadecimalToDecimal(bt);
                        result += Dec;
                    }
                    string hexResult = Converter.DecimalToHexadecimal(result).PadLeft(type / 4, '0');
                    if (type == 64)
                    {
                        string FinalResult = null;
                        string[] Arr = Converter.ByteStringToByteArray(hexResult);
                        for (int Cur = Arr.Length - 1; Cur >= 0; Cur--)
                        {
                            FinalResult += Arr[Cur];
                        }
                        return FinalResult;
                    }
                    else
                    {
                        return hexResult.Substring(hexResult.Length - type / 4);
                    }
                }
                public static string Checksum(int type, string ByteData)
                {
                    int result = 0;
                    foreach (string bt in Converter.ByteStringToByteArray(ByteData))
                    {
                        int Dec = Converter.HexadecimalToDecimal(bt);
                        result += Dec;
                    }
                    string hexResult = Converter.DecimalToHexadecimal(result).PadLeft(type / 4, '0');
                    if (type == 64)
                    {
                        string FinalResult = null;
                        string[] Arr = Converter.ByteStringToByteArray(hexResult);
                        for (int Cur = Arr.Length - 1; Cur >= 0; Cur--)
                            FinalResult += Arr[Cur];
                        return FinalResult;
                    }
                    else
                    {
                        return hexResult.Substring(hexResult.Length - type / 4);
                    }
                }
                public static string XOR(string HexNumber1, string HexNumber2)
                {
                    string[] Hbin1;
                    string[] Hbin2;
                    if (Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length > Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length)
                    {
                        Hbin1 = new string[Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length];
                        Hbin2 = new string[Converter.HexadecimalToBinary(HexNumber2).ToCharArray().Length];
                    }
                    else
                    {
                        Hbin1 = new string[Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length];
                        Hbin2 = new string[Converter.HexadecimalToBinary(HexNumber1).ToCharArray().Length];
                    }
                    int i = 0;
                    foreach (char bit in Converter.HexadecimalToBinary(HexNumber1).ToCharArray())
                    {
                        Hbin1[i] = bit.ToString();
                        i++;
                    }
                    i = 0;
                    foreach (char bit in Converter.HexadecimalToBinary(HexNumber2).ToCharArray())
                    {
                        if (i == Hbin2.Length)
                        {

                        }
                        else
                        {
                            Hbin2[i] = bit.ToString();
                            i++;
                        }
                    }
                    i = 0;
                    string BinaryResult = null;
                    string Hbin1Str = null;
                    string Hbin2Str = null;
                    foreach (string bit in Hbin1)
                    {
                        Hbin1Str += bit;
                    }
                    foreach (string bit in Hbin2)
                    {
                        Hbin2Str += bit;
                    }
                    string count1 = "0";
                    foreach (string bit in Hbin1)
                    {
                        if (bit != null)
                        {
                            count1 = Convert.ToString(Convert.ToInt64(count1) + 1);
                        }
                    }
                    string count2 = "0";
                    foreach (string bit in Hbin2)
                    {
                        if (bit != null)
                        {
                            count2 = Convert.ToString(Convert.ToInt64(count2) + 1);
                        }
                    }
                    if (Convert.ToInt32(count1) > Convert.ToInt32(count2))
                    {
                        while (Hbin2Str.Length != Hbin1Str.Length)
                        {
                            Hbin2Str = Hbin2Str.Insert(0, "0");
                        }
                    }
                    else
                    {
                        while (Hbin1Str.Length != Hbin2Str.Length)
                        {
                            Hbin1Str = Hbin1Str.Insert(0, "0");
                        }
                    }
                    foreach (char bit in Hbin1Str.ToCharArray())
                    {
                        if (bit.ToString() == Hbin2Str.ToCharArray()[i].ToString())
                        {
                            string temp = Hbin1Str;
                            string tmp = Hbin2Str;
                            string tp = Hbin2Str.ToCharArray()[i].ToString();
                            BinaryResult += "0";
                            string z = temp + tmp + BinaryResult + bit + tp;
                        }
                        else
                        {
                            string temp = Hbin1Str;
                            string tmp = Hbin2Str;
                            string tp = Hbin2Str.ToCharArray()[i].ToString();
                            BinaryResult += "1";
                            string z = temp + tmp + BinaryResult + bit + tp;
                        }
                        i++;
                    }
                    return Converter.BinaryToHexadecimal(BinaryResult);
                }
                /// <summary>
                /// Fills a block of bytes with the given Pattern (only 2 chars)
                /// </summary>
                /// <param name="StartAddress">Block start address</param>
                /// <param name="EndAddress">Block end address</param>
                /// <param name="Value">Value pattern (2 chars)</param>
                /// <returns>Returns "true" for success and "false" for failure</returns>
                public bool FillBlock(int StartAddress, int EndAddress, string Value)
                {
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(OpenWrite);
                    try
                    {
                        int length = EndAddress - StartAddress + 1;
                        var v = "";
                        for (int i = 0; i < length; i++)
                            v += Value;
                        writer.BaseStream.Position = StartAddress;
                        writer.Write(GetBuffer(v));
                        writer.Close();
                        return true;
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }
                /// <summary>
                /// Checks if a block of bytes is all the given pattern (only 2 chars)
                /// </summary>
                /// <param name="StartAddress">Block start address</param>
                /// <param name="EndAddress">Block end address</param>
                /// <param name="Value">Value pattern (2 chars)</param>
                /// <returns></returns>
                public bool CheckBlock(int StartAddress, int EndAddress, string Value)
                {
                    int CurAddress = StartAddress;
                    if (Value.ToCharArray().Length != 2) { return false; }
                    else
                    {
                        while (CurAddress != EndAddress + 1)
                        {
                            if (ReadHexValue(CurAddress) != Value)
                            {
                                return false;
                            }
                            CurAddress++;
                        }
                    }
                    return true;
                }
                /// <summary>
                /// Read a value from the indicated address
                /// </summary>
                /// <param name="Address">The address of the byte to be readed (as 0x00000)</param>
                /// <returns>Returns the value of the bytes in Hexadecimal</returns>
                public string ReadHexValue(int Address, string Side = "Both")
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(file));
                    reader.BaseStream.Position = Address;
                    string thing = reader.ReadByte().ToString("X2");
                    reader.Close();
                    switch (Side)
                    {
                        case "Left":
                            return thing.Substring(0, 1);
                        case "Right":
                            return thing.Substring(1);
                        default:
                            return thing;
                    }
                }
                /// <summary>
                /// Read a value from the indicated address and inverts it
                /// </summary>
                /// <param name="Address">The address of the byte to be readed (as 0x00000)</param>
                /// <returns>Returns the value of the bytes in Hexadecimal</returns>
                public string ReadInvertedHexValue(int Address, string Side = "Both")
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(file));
                    reader.BaseStream.Position = Address;
                    string thing = reader.ReadByte().ToString("X2");
                    reader.Close();
                    switch (Side)
                    {
                        case "Left":
                            return Converter.InvertHexBytes(thing.Substring(0, 1));
                        case "Right":
                            return Converter.InvertHexBytes(thing.Substring(1));
                        default:
                            return Converter.InvertHexBytes(thing);
                    }
                }
                /// <summary>
                /// Reads multiple values from a start address to a end address
                /// </summary>
                /// <param name="StartAddress">The reading start address</param>
                /// <param name="EndAddress">The reading end address</param>
                /// <param name="spliter">(Optional) The char inserted to separed each byte (default = "-")</param>
                /// <returns></returns>
                public string ReadMultipleHexValues(int StartAddress, int EndAddress, string spliter = "-")
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(OpenRead);
                    reader.BaseStream.Position = StartAddress;
                    int length = EndAddress - StartAddress + 1;
                    var buffer = reader.ReadBytes(length); reader.Close();
                    return ByteBufferToString(buffer);
                }
                /// <summary>
                /// Reads multiple values from a start address to a end address and inverts the values
                /// </summary>
                /// <param name="StartAddress">The reading start address</param>
                /// <param name="EndAddress">The reading end address</param>
                /// <param name="spliter">(Optional) The char inserted to separed each byte (default = "-")</param>
                /// <returns></returns>
                public string ReadMultipleInvertedHexValues(int StartAddress, int EndAddress, string spliter = "-") => ReadMultipleInvertedASCValues(StartAddress, EndAddress, spliter).ToHex();
                /// <summary>
                /// Read the text value form of the indicated address
                /// </summary>
                /// <param name="Address">The address of the byte to be readed (as 0x00000)</param>
                /// <returns>Returns the value of the bytes in Hexadecimal</returns>
                public string ReadASCValue(int Address)
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(OpenRead);
                    reader.BaseStream.Position = Address;
                    var asc = Converter.HexadecimalToText(Converter.DecimalToHexadecimal((int)reader.ReadByte()));
                    reader.Close();
                    return asc;
                }
                /// <summary>
                /// Read the text value form of the indicated address as inverts it
                /// </summary>
                /// <param name="Address">The address of the byte to be readed (as 0x00000)</param>
                /// <returns>Returns the inverted value of the bytes in Hexadecimal</returns>
                public string ReadInvertedASCValue(int Address)
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(file));
                    reader.BaseStream.Position = Address;
                    string thing = reader.ReadByte().ToString("X2");
                    reader.Close();
                    return Converter.InvertASCValue(Converter.HexadecimalToText(thing));
                }
                private string ByteBufferToString(byte[] buffer)
                {
                    var str = "";
                    foreach (var b in buffer)
                        str += b.ToString("X2");
                    return str;
                }
                /// <summary>
                /// Reads multiple text values from a start address to a end address
                /// </summary>
                /// <param name="StartAddress">The reading start address</param>
                /// <param name="EndAddress">The reading end address</param>
                /// <param name="spliter">(Optional) The char inserted to separed each char (default = null)</param>
                /// <returns></returns>
                public string ReadMultipleASCValues(int StartAddress, int EndAddress, string spliter = "")
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(OpenRead);
                    reader.BaseStream.Position = StartAddress;
                    int length = EndAddress - StartAddress + 1;
                    var buffer = reader.ReadBytes(length); reader.Close();
                    return Converter.HexadecimalToText(ByteBufferToString(buffer));
                }
                /// <summary>
                /// Reads multiple text values from a start address to a end address
                /// </summary>
                /// <param name="StartAddress">The reading start address</param>
                /// <param name="EndAddress">The reading end address</param>
                /// <param name="spliter">(Optional) The char inserted to separed each char (default = null)</param>
                /// <returns></returns>
                public string ReadMultipleInvertedASCValues(int StartAddress, int EndAddress, string spliter = "") => Converter.InvertASCValue(ReadMultipleASCValues(StartAddress, EndAddress, spliter));
                /// <summary>
                /// Writes a Hexadecimal value to a file
                /// </summary>
                /// <param name="Address">The address in the file to writen</param>
                /// <param name="Value">(Optional) The value to be writen (default = 0)</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteHexValue(int Address, string Value = "0", string Side = "Both")
                {
                    try
                    {
                        if (Side == "Right")
                        {
                            string LeftSide = Converter.HexadecimalToDecimal(ReadHexValue(Address, "Left")).ToString();
                            string RightSide = Converter.HexadecimalToDecimal(Value).ToString();
                            string Temp = LeftSide + RightSide;
                            int writeValue = Int32.Parse(Temp);
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(writeValue));
                            writer.Close();
                        }
                        if (Side == "Left")
                        {
                            string RightSide = Converter.HexadecimalToDecimal(ReadHexValue(Address, "Right")).ToString();
                            string LeftSide = Converter.HexadecimalToDecimal(Value).ToString();
                            string Temp = LeftSide + RightSide;
                            int writeValue = Int32.Parse(Temp);
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(writeValue));
                            writer.Close();
                        }
                        else
                        {
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Value)));
                            writer.Close();
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                /// <summary>
                /// Writes a inverted Hexadecimal value to a file
                /// </summary>
                /// <param name="Address">The address in the file to writen</param>
                /// <param name="Value">(Optional) The value to be writen (default = 0)</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteInvertedHexValue(int Address, string Value = "0", string Side = "Both")
                {
                    try
                    {
                        if (Side == "Right")
                        {
                            string LeftSide = Converter.HexadecimalToDecimal(ReadHexValue(Address, "Left")).ToString();
                            string RightSide = Converter.HexadecimalToDecimal(Value).ToString();
                            string Temp = LeftSide + RightSide;
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Converter.InvertHexBytes(Temp))));
                            writer.Close();
                        }
                        if (Side == "Left")
                        {
                            string RightSide = Converter.HexadecimalToDecimal(ReadHexValue(Address, "Right")).ToString();
                            string LeftSide = Converter.HexadecimalToDecimal(Value).ToString();
                            string Temp = LeftSide + RightSide;
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Converter.InvertHexBytes(Temp))));
                            writer.Close();
                        }
                        else
                        {
                            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                            writer.BaseStream.Position = Address;
                            writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Converter.InvertHexBytes(Value))));
                            writer.Close();
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                /// <summary>
                /// Writes a ASCII value to a file
                /// </summary>
                /// <param name="Address">The address in the file to writen</param>
                /// <param name="Value">(Optional) The value to be writen (default = 0)</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteASCValue(int Address, string Value = "0")
                {
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                    try
                    {
                        writer.BaseStream.Position = Address;
                        writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Converter.TextToHexadecimal(Value))));
                        writer.Close();
                        return true;
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }
                /// <summary>
                /// Writes a inverted ASCII value to a file
                /// </summary>
                /// <param name="Address">The address in the file to writen</param>
                /// <param name="Value">(Optional) The value to be writen (default = 0)</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteInvertedASCValue(int Address, string Value = "0")
                {
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                    try
                    {
                        writer.BaseStream.Position = Address;
                        writer.Write(Convert.ToByte(Converter.HexadecimalToDecimal(Converter.TextToHexadecimal(Converter.InvertASCValue(Value)))));
                        writer.Close();
                        return true;
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }
                /// <summary>
                /// Gets the total ammount of addresses
                /// </summary>
                /// <returns>Returns the total ammount of address in hexadecimal</returns>
                public string TotalAddresses
                {
                    get
                    {
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(OpenRead))
                            return Convert.ToString(reader.BaseStream.Length - 1, 16).ToUpper();
                    }
                }
                /// <summary>
                /// Swaps the byte in the specified address or if address not defined swaps every single byte in the archive
                /// </summary>
                /// <param name="Address">The byte address to swap</param>
                /// <returns>Returns the swaped bytes</returns>
                public string SwapBytes(int Address = -1)
                {
                    string SwapedBytes = null;
                    int CurAddress = 0;
                    if (Address != -1)
                    {
                        string bts = ReadHexValue(Address);
                        WriteHexValue(Address, Converter.SwapHexBytes(bts));
                        return Converter.SwapHexBytes(bts);
                    }
                    foreach (string bt in ReadAll().Split(' '))
                    {
                        string SwapedByte = Converter.SwapHexBytes(bt);
                        SwapedBytes += Converter.SwapHexBytes(bt) + " ";
                        WriteHexValue(CurAddress, SwapedByte);
                        CurAddress++;
                    }
                    return SwapedBytes;
                }
                /// <summary>
                /// If addresses not defined swaps every single byte in the archive
                /// </summary>
                /// <param name="Address">The byte address to swap</param>
                /// <returns>Returns the swaped bytes</returns>
                public void SwapBytesPair(int Address = -1, int SecondAddress = -1)
                {
                    int CurAddress = 0;
                    if (Address != -1 && SecondAddress != -1)
                    {
                        string bts = ReadHexValue(SecondAddress) + ReadHexValue(Address);
                        string[] swBts = Converter.ByteStringToByteArray(bts);
                        WriteHexValue(Address, swBts[0]);
                        WriteHexValue(SecondAddress, swBts[1]);
                    }
                    else
                    {
                        foreach (string bt in ReadAll(16).Split('\n'))
                        {
                            string SwapedByte = Converter.SwapHexBytesPair(bt.Replace(" ", ""));
                            WriteMultipleHexValues(CurAddress, SwapedByte);
                            CurAddress += 16;
                        }
                    }
                }
                public string InvertBytes(int StartAddress, int EndAddress)
                {
                    string ret = "";
                    for (int i = StartAddress; i <= EndAddress; i++)
                    {
                        ret += InvertBytes(i);
                    }
                    return ret;
                }
                /// <summary>
                /// Inverts the byte in the specified address or if address not defined inverts every single byte in the archive
                /// </summary>
                /// <param name="Address">The byte address to invert</param>
                /// <returns>Returns the inverted bytes (Optional)</returns>
                public string InvertBytes(int Address = -1)
                {
                    string InvBytes = null;
                    int CurAddress = 0;
                    if (Address != -1)
                    {
                        string bts = ReadHexValue(Address);
                        WriteHexValue(Address, Converter.InvertHexBytes(bts));
                        return Converter.InvertHexBytes(bts);
                    }
                    foreach (string bt in ReadAll().Split(' '))
                    {
                        string InvByte = Converter.InvertHexBytes(bt);
                        InvBytes += Converter.InvertHexBytes(bt) + " ";
                        WriteHexValue(CurAddress, InvByte);
                        CurAddress++;
                    }
                    return InvBytes;
                }
                public string ReadAll(int bytesPerColumn, string spliter = " ")
                {
                    var bts = ReadAll(spliter);
                    if (bts.Length % 2 != 0)
                        bts = "0" + bts;
                    var newBytes = "";
                    var cc = 1;
                    for (int i = 0; i < bts.Length; i += 2)
                    { newBytes += bts.Substring(i, 2); if (cc == bytesPerColumn) { newBytes += '\n'; cc = 1; } else { newBytes += spliter; cc++; } }
                    return newBytes;
                }
                /// <summary>
                /// Reads all bytes in the archive
                /// </summary>
                /// <param name="spliter">The spliter between each byte (default = ' ')</param>
                /// <returns>Returns all bytes read</returns>
                public string ReadAll(string spliter = " ")
                {
                    System.IO.BinaryReader reader = new System.IO.BinaryReader(OpenRead);
                    var bytes = reader.ReadBytes(reader.BaseStream.Length.ToString().ToInt());
                    reader.Close();
                    var bts = "";
                    foreach (var b in bytes)
                        bts += b.ToString("X2") + spliter;
                    return bts.Substring(0, bts.Length - 1);
                }
                private byte[] GetBuffer(string bytes)
                {
                    List<byte> bts = new List<byte>();
                    var b = bytes.Replace(" ", "").Replace("-", "").Replace("\r", "");
                    if (b.Length % 2 != 0)
                        b = "0" + b;
                    for (int i = 0; i < b.Length; i += 2)
                        bts.Add((byte)Convert.ToInt32(b.Substring(i, 2), 16));
                    return bts.ToArray();
                }
                /// <summary>
                /// Writes a string of Hexadecimal values to a file
                /// </summary>
                /// <param name="Address">The write address</param>
                /// <param name="Value">The hex string value to be writen</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteMultipleHexValues(int Address, string Value)
                {
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(OpenWrite);
                    try
                    {

                        writer.BaseStream.Position = Address;
                        writer.Write(GetBuffer(Value));
                        writer.Close();
                        return true;
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }
                /// <summary>
                /// Writes a string of inverted Hexadecimal values to a file
                /// </summary>
                /// <param name="StartAddress">The write start address</param>
                /// <param name="EndAddress">The write end address</param>
                /// <param name="Value">The hex string value to be writen</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteMultipleInvertedHexValues(int Address, string Value)
                {
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(file));
                    try
                    {
                        writer.BaseStream.Position = Address;
                        writer.Write(GetBuffer(Converter.InvertHexBytes(Value)));
                        writer.Close();
                        return true;
                    }
                    catch
                    {
                        writer.Close();
                        return false;
                    }
                }
                public bool WriteMultipleASCValues(int Address, string Value)
                {
                    try
                    {
                        WriteMultipleHexValues(Address, Converter.TextToHexadecimal(Value));
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                /// <summary>
                /// Writes a string of inverted ASCII values to a file
                /// </summary>
                /// <param name="StartAddress">The write start address</param>
                /// <param name="EndAddress">The write end address</param>
                /// <param name="Value">The ASCII string value to be writen</param>
                /// <returns>Returns "true" for success; "false" for fail</returns>
                public bool WriteMultipleInvertedASCValues(int Address, string Value)
                {
                    try
                    {
                        WriteMultipleInvertedHexValues(Address, Converter.TextToHexadecimal(Value));
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            class ProccessMemoryReaderApi
            {
                // constants information can be found in <winnt.h>
                [Flags]
                public enum ProcessAccessType
                {
                    PROCESS_TERMINATE = (0x0001),
                    PROCESS_CREATE_THREAD = (0x0002),
                    PROCESS_SET_SESSIONID = (0x0004),
                    PROCESS_VM_OPERATION = (0x0008),
                    PROCESS_VM_READ = (0x0010),
                    PROCESS_VM_WRITE = (0x0020),
                    PROCESS_DUP_HANDLE = (0x0040),
                    PROCESS_CREATE_PROCESS = (0x0080),
                    PROCESS_SET_QUOTA = (0x0100),
                    PROCESS_SET_INFORMATION = (0x0200),
                    PROCESS_QUERY_INFORMATION = (0x0400),
                    PROCESS_QUERY_LIMITED_INFORMATION = (0x1000)
                }

                // function declarations are found in the MSDN and in <winbase.h> 

                //		HANDLE OpenProcess(
                //			DWORD dwDesiredAccess,  // access flag
                //			BOOL bInheritHandle,    // handle inheritance option
                //			DWORD dwProcessId       // process identifier
                //			);
                [DllImport("kernel32.dll")]
                public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

                //		BOOL CloseHandle(
                //			HANDLE hObject   // handle to object
                //			);
                [DllImport("kernel32.dll")]
                public static extern Int32 CloseHandle(IntPtr hObject);

                //		BOOL ReadProcessMemory(
                //			HANDLE hProcess,              // handle to the process
                //			LPCVOID lpBaseAddress,        // base of memory area
                //			LPVOID lpBuffer,              // data buffer
                //			SIZE_T nSize,                 // number of bytes to read
                //			SIZE_T * lpNumberOfBytesRead  // number of bytes read
                //			);
                [DllImport("kernel32.dll")]
                public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

                //		BOOL WriteProcessMemory(
                //			HANDLE hProcess,                // handle to process
                //			LPVOID lpBaseAddress,           // base of memory area
                //			LPCVOID lpBuffer,               // data buffer
                //			SIZE_T nSize,                   // count of bytes to write
                //			SIZE_T * lpNumberOfBytesWritten // count of bytes written
                //			);
                [DllImport("kernel32.dll")]
                public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
            }
            public class Memory
            {
                public uint ReadMemory(string ProccessName, int Address, uint BytesToRead, int ProccessModule = 0)
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(ProccessName).ToList().FirstOrDefault();
                    ProccessMemoryReader mreader = new ProccessMemoryReader();
                    if (process != null)
                    {
                        mreader.ReadProcess = process;
                        mreader.OpenProccess();
                        uint result = BitConverter.ToUInt32(mreader.ReadMemory((IntPtr)(Address + (uint)process.Modules[ProccessModule].BaseAddress), BytesToRead, out _), 0);
                        mreader.CloseHandle();
                        return result;
                    }
                    else
                    {
                        return 0;
                    }
                }
                public string WriteMemory(int Address, string ProccessName, int BytesToWrite, int ProccessModule = 0)
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(ProccessName).ToList().FirstOrDefault();
                    ProccessMemoryReader mreader = new ProccessMemoryReader();
                    if (process != null)
                    {
                        mreader.ReadProcess = process;
                        mreader.OpenProccess();
                        mreader.WriteMemory((IntPtr)(Address + (uint)process.Modules[ProccessModule].BaseAddress), BitConverter.GetBytes(BytesToWrite), out int bytesWritten);
                        mreader.CloseHandle();
                        return bytesWritten.ToString();
                    }
                    else
                    {
                        return "Failed";
                    }
                }
            }
            public class ProccessMemoryReader
            {
                public System.Diagnostics.Process ReadProcess { get; set; }

                private IntPtr handle;
                public void OpenProccess()
                {
                    ProccessMemoryReaderApi.ProcessAccessType access = ProccessMemoryReaderApi.ProcessAccessType.PROCESS_QUERY_INFORMATION |
                        ProccessMemoryReaderApi.ProcessAccessType.PROCESS_VM_READ |
                        ProccessMemoryReaderApi.ProcessAccessType.PROCESS_VM_WRITE |
                        ProccessMemoryReaderApi.ProcessAccessType.PROCESS_VM_OPERATION;
                    handle = ProccessMemoryReaderApi.OpenProcess((uint)access, 1, (uint)ReadProcess.Id);
                }
                public void CloseHandle()
                {
                    int returnValue = ProccessMemoryReaderApi.CloseHandle(handle);
                    if (returnValue != 0)
                        throw new Exception("Closing handle failed.");
                }
                public byte[] ReadMemory(IntPtr memoryAddress, uint bytesToRead, out int bytesRead)
                {
                    byte[] buffer = new byte[bytesToRead];
                    ProccessMemoryReaderApi.ReadProcessMemory(handle, memoryAddress, buffer, bytesToRead, out var pBytesRead);
                    bytesRead = pBytesRead.ToInt32();
                    return buffer;
                }
                public void WriteMemory(IntPtr memoryAddress, byte[] buffer, out int bytesWritten)
                {
                    ProccessMemoryReaderApi.WriteProcessMemory(handle, memoryAddress, buffer, (uint)buffer.Length, out IntPtr pBytesWritten);
                    bytesWritten = pBytesWritten.ToInt32();
                }
            }
        }
        namespace Info
        {
            public static class UserInfo
            {
                public static bool IsAuthenticated()
                {
                    return System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated;
                }
                public static string UserName()
                {
                    return System.Threading.Thread.CurrentPrincipal.Identity.Name;
                }
            }
            public static class HDInfo
            {
                public static long GetAvaliableFreeStorage(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.AvailableFreeSpace;
                }
                public static string GetDriveFormat(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.DriveFormat;
                }
                public static System.IO.DriveType GetDriveType(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.DriveType;
                }
                public static bool IsReady(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.IsReady;
                }
                public static string GetName(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.Name;
                }
                public static System.IO.DirectoryInfo GetRootDirectory(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.RootDirectory;
                }
                public static long GetTotalFreeSpace(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.TotalFreeSpace;
                }
                public static long GetTotalSize(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.TotalSize;
                }
                public static string GetVolumeLabel(string DriveLetter)
                {
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
                    return HD.VolumeLabel;
                }
                public static void SetVolumeLabel(string DriveLetter, string VolumeLabel)
                {
#pragma warning disable IDE0017 // Simplificar a inicialização de objeto
                    System.IO.DriveInfo HD = new System.IO.DriveInfo(DriveLetter);
#pragma warning restore IDE0017 // Simplificar a inicialização de objeto
                    HD.VolumeLabel = VolumeLabel;
                }
            }
            public enum HashingAlgorithm
            {
                MD5 = 1,
                SHA1 = 2,
                SHA256 = 3,
                SHA384 = 4,
                SHA512 = 5
            }
            public static partial class AndroidInfo
            {
                public static string Name { get => Environment.MachineName; }

                public static IPAddress[] LocalAddressList
                {
                    get => System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
                }
                public static IPAddress ExternalIpAddress
                {
                    get
                    {
                        try
                        {
                            var req = new WebClient().DownloadString("http://ifconfig.me");
                            if (IPAddress.TryParse(req, out IPAddress _))
                                return IPAddress.Parse(req);
                            else
                                return null;
                        }
                        catch { return null; }
                    }
                }
            }
        }
    }
}
