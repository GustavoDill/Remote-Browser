using Android.App;
using AndroidExtendedCommands;
using AndroidExtendedCommands.CSharp.Data.SimpleJSON;
using Java.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Xamarin.Forms;

namespace Remote_Browser
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public RemoteBrowserClient Client { get; private set; }
        public static Activity Activity { get; set; }
        public static ImageSource ReturnButtonImg { get; set; }
        public const string AUTH_CODE = "RemoteBrowser#CODE#";
        public const int MAX_DOWNLOAD_QUEUE = 1;
        public const long MAX_FILE_SIZE = 104857600;
        public string SettingsFile { get => Settings.SettingsFile; }
        public Settings Settings { get; set; }
        public DownloadQueu Downloads { get; set; }
        public Search Search { get; set; }
        public MainPage()
        {
            InitializeComponent();
            DisplayList = new List<DisplayItem>();

            //File.Delete(SettingsFile);
            path.Completed += Path_Completed;
            new Thread(() =>
            {
                ReturnButtonImg = ImageSource.FromResource("Remote_Browser.Icons.return.ico");
                settingsImg.Source = ImageSource.FromResource("Remote_Browser.Icons.settings.png");
                downloadsImg.Source = ImageSource.FromResource("Remote_Browser.Icons.downloads.ico");
                searchImg.Source = ImageSource.FromResource("Remote_Browser.Icons.search.ico");
            }).Start();
            Settings = new Settings() { ParentPage = this };
            Downloads = new DownloadQueu() { Settings = Settings, Client = Client };
            Search = new Search() { ParentPage = this };
            Downloads.DownloadFinished += Downloads_DownloadFinished;
            new Thread(LoadSettings).Start();
            //ConnectToServer();
        }
        public bool ConnectButtonEnabled
        {
            get => connectBtn.IsEnabled;
            set => Device.BeginInvokeOnMainThread(() => connectBtn.IsEnabled = value);
        }
        private void Downloads_DownloadFinished(object sender, DownloadQueu.DownloadFinishArgs e)
        {
            Activity.RunOnUiThread(() => DisplayAlert("Download finished", "Download of the file has finished", "OK"));
            //Activity.RunOnUiThread(() => Android.Widget.Toast.MakeText(Activity, e.ToString(), Android.Widget.ToastLength.Short));
        }

        private void Settings_Saved(object sender, System.EventArgs e)
        {
            OnSettingsSave();

        }

        void OnSettingsSave()
        {
            Navigation.PopModalAsync(true);
            var json = JSON.Parse($"{{\"SaveDirectory\" : \"{Settings.SaveDirectory}\", \"Servers\" : []}}");
            foreach (var server in Settings.AvaliableServers)
                json["Servers"].Add(server.GetJSON());
            using (var writer = new StreamWriter(SettingsFile))
                writer.Write(json.ToString());
        }
        void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                JSONNode config;
                using (var reader = new StreamReader(SettingsFile))
                    config = JSON.Parse(reader.ReadToEnd());
                if (config != null)
                {
                    List<ConnectionServer> avaliableServers = new List<ConnectionServer>();
                    if (!(config["Servers"] == null))
                        for (int i = 0; i < config["Servers"].Count; i++)
                            avaliableServers.Add(config["Servers"][i]);
                    Settings.AvaliableServers = avaliableServers;
                    Settings.UpdateList();
                    Settings.SaveDirectory = !string.IsNullOrEmpty(config["SaveDirectory"].Value) ? config["SaveDirectory"].Value : Settings.DEFAULT_SAVE_DIRECTORY;
                }
                else
                {
                    Settings.SaveDirectory = Settings.DEFAULT_SAVE_DIRECTORY;
                }

            }
            else
            {
                using (var writer = new StreamWriter(SettingsFile))
                    writer.Write($"{{\"SaveDirectory\" : \"{Settings.DEFAULT_SAVE_DIRECTORY}\", \"Servers\" : []}}");
                Settings.SaveDirectory = Settings.DEFAULT_SAVE_DIRECTORY;
            }
            Settings.Saved += Settings_Saved;
        }
        private void Path_Completed(object sender, System.EventArgs e)
        {
            if (Client != null)
            {
                if (Client.Connected)
                {
                    if (Client.SetDirectory(path.Text))
                        path.Text = Client.CurrentDirectory;
                    else
                    {
                        path.Text = Client.CurrentDirectory;
                        Device.BeginInvokeOnMainThread(() => DisplayAlert("Navigate Directory", "Directory does not exist!", "OK"));
                    }
                }
            }
            else
                Device.BeginInvokeOnMainThread(() => DisplayAlert("Navigate Directory", "Please connect to server first!", "OK"));
        }
        Thread connectionThread;
        void CreateConnection()
        {
            Client = Settings.SelectedHost.CreateConnection(path.Text);
            Client.Activity = Activity;
            //Client = new RemoteBrowserClient(Settings.HostIp, Settings.HostPort, path.Text, 0) { Activity = Activity };
            Downloads.Client = Client;
            if (connectionThread != null)
                try { connectionThread.Abort(); } catch { }
            connectionThread = new Thread(ConnectToServer);
            connectionThread.Start();
        }
        void ConnectToServer()
        {
            try
            {
                Client.Connect();
                Client.SendString(AUTH_CODE);
                Client.Navigated += Client_Navigated;
                Client.DirectorySet += Client_DirectorySet;
                Device.BeginInvokeOnMainThread(() => BindingContext = this);
                new Thread(new ParameterizedThreadStart(InitialList)).Start(path.Text);
                Device.BeginInvokeOnMainThread(() => connectBtn.Text = "Disconnect");
            }
            catch (SocketException ex) { Device.BeginInvokeOnMainThread(() => DisplayAlert("Connect to Server", "Failed to connect:\n\n" + ex.Message, "OK")); }
        }
        void InitialList(object p)
        {
            string dir = p.ToString();
            Device.BeginInvokeOnMainThread(() =>
            {
                list.ItemsSource = null;
                DisplayList.Clear();
                var l = Client.ListDirectory();
                if (l.Files[0] == "ACCESS DENIED")
                { Client.NavigateBack(); return; }
                if (l.Files[0] == "Code rejected")
                {
                    Client.Shutdown();
                    Dialogs.ShowDialog(Activity, "Connect", "Connection code was rejected", (sender, e) =>
                    {
                        //navigateBtn.IsEnabled = false;
                    }, neutralButton: "OK");
                    return;
                }
                foreach (var d in l.Directories)
                    DisplayList.Add(new DisplayItem(d, "Directory"));
                foreach (var f in l.Files)
                    DisplayList.Add(new DisplayItem(f, "File"));
                //Device.BeginInvokeOnMainThread(() =>
                //{
                list.ItemsSource = DisplayList;
                list.SeparatorVisibility = SeparatorVisibility.Default;
                list.SeparatorColor = Color.DarkCyan;
                path.Text = Client.CurrentDirectory;
                //});
            });
        }
        private void Client_DirectorySet(object sender, RemoteBrowserClient.ClientNavigateEventArgs e)
        {
            UpdateListing(e);
        }
        public void UpdateListing(RemoteBrowserClient.ClientNavigateEventArgs e)
        {
            Thread.Sleep(100);
            var l = Client.ListDirectory();
            try
            {
                if (l.Files[0] == "CONNECTION CLOSED")
                { Device.BeginInvokeOnMainThread(() => { list.ItemsSource = null; DisplayAlert("List directory", "Connection Closed\n\nExpected reason:\n Server shutdown", "OK"); }); return; }
            }
            catch { }
            try
            {
                if (l.Files[0] == "ACCESS DENIED")
                { DisplayAlert("Navigate directory", "Access to directory denied", "OK"); return; }
            }
            catch
            {
                if (l.Directories[0] == "ACCESS DENIED")
                { DisplayAlert("Navigate directory", "Access to directory denied", "OK"); return; }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                list.ItemsSource = null;
                DisplayList.Clear();
                foreach (var d in l.Directories)
                    DisplayList.Add(new DisplayItem(d, "Directory"));
                foreach (var f in l.Files)
                    DisplayList.Add(new DisplayItem(f, "File"));

                list.ItemsSource = DisplayList;
                list.ScrollTo(DisplayList[0], ScrollToPosition.Start, true);
                path.Text = Client.CurrentDirectory;
            });
        }
        private void Client_Navigated(object sender, RemoteBrowserClient.ClientNavigateEventArgs e)
        {
            UpdateListing(e);
        }
        public int CountChar(string v, char cc)
        {
            int count = 0;
            foreach (char c in v)
                if (c == cc)
                    count++;
            return count;
        }
        public List<DisplayItem> DisplayList { get; set; }
        public object oldItem;
        private void list_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (oldItem == null)
                oldItem = e.Item;
            else if (oldItem != e.Item)
                oldItem = e.Item;
            else
                new Thread(new ParameterizedThreadStart(ItemClick)).Start(e.Item);
            var details = new DisplayItemDetails(Client, "D:\\.ProgramsVS\\Esquema rele.png");
            details.RequestDetails();
        }
        public async void RetrieveFile(object itemPath)
        {
            var pt = itemPath.ToString();
            if (Downloads.Items.Count < MAX_DOWNLOAD_QUEUE)
            {
                var fileSize = Client.GetFileSize(pt);
                if (fileSize.Contains("ERROR_FILE_INEXISTENT"))
                    Device.BeginInvokeOnMainThread(() => DisplayAlert("Download file", "File does not exist", "OK"));
                else
                {
                    if (!(Convert.ToInt64(fileSize, 16) > MAX_FILE_SIZE))
                        await Downloads.AddDownloadToQueue(pt);
                    else
                        Device.BeginInvokeOnMainThread(() => DisplayAlert("Download file", "File too big!\n (File Size: " + (Convert.ToInt64(fileSize, 16) / 1024 / 1024).ToString() + "MB - Max size: " + (MAX_FILE_SIZE / 1024 / 1024).ToString() + "MB)", "OK"));
                }
            }
            else
                Device.BeginInvokeOnMainThread(() => DisplayAlert("Download file", "Max download queue already achieved! Wait for a file to download and then add this file to the queue", "OK"));
        }
        public async void ItemClick(object param)
        {
            var item = (DisplayItem)param;
            if (item.Text == "..")
                Client.NavigateBack();
            else if (item.typeString == "Directory")
                Client.Navigate(item.Text);
            else
            {
                var p = await Device.InvokeOnMainThreadAsync(() => { return Client.CurrentDirectory; });
                if (p.Length == 2)
                    p += "\\";
                var pt = System.IO.Path.Combine(p, item.Text).Replace("/", "\\");
                new Thread(new ParameterizedThreadStart(RetrieveFile)).Start(pt);
            }

        }

        private void connect_Clicked(object sender, System.EventArgs e)
        {
            if (((Button)sender).Text.ToUpper() == "CONNECT")
                CreateConnection();
            else
                CloseConnection();
        }
        public void CloseConnection()
        {
            if (Client.Connected)
            {
                Client.SendCommand("CLOSE-CONNECTION");
                Thread.Sleep(50);
                Client.Disconnect();
            }
            else
                Client.SendCommand("CLOSE-CONNECTION");
            Device.BeginInvokeOnMainThread(() => connectBtn.Text = "Connect");
        }
        private void searchImg_Tapped(object sender, EventArgs e)
        {
            if (Client != null)
                if (Client.Connected)
                    ShowSearch();
        }
        void ShowSearch()
        {
            Navigation.PushModalAsync(Search, true);
        }
        private void settingsImg_Tapped(object sender, System.EventArgs e)
        {
            ShowSettings();
        }
        void ShowSettings()
        {
            Navigation.PushModalAsync(Settings, true);
        }

        private void downloadsImg_Tapped(object sender, System.EventArgs e)
        {
            Navigation.PushModalAsync(Downloads, true);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }
    }
    public class DisplayItem
    {
        public DisplayItem(string text, string type)
        {
            Text = text;
            this.typeString = type;
        }
        public DisplayItem() { }
        public string typeString = "";
        public string Text { get; internal set; }
        public ImageSource Type
        {
            get
            {
                if (typeString == "Directory")
                    return ImageSource.FromResource("Remote_Browser.Icons.folder.png");
                else if (typeString == typeof(DirectoryInfo).Name)
                    return ImageSource.FromResource("Remote_Browser.Icons.folder.png");
                else
                    return ImageSource.FromResource("Remote_Browser.Icons.copy-icon.png");
            }
        }
    }
}
