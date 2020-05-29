using Android.App;
using AndroidExtendedCommands;
using AndroidExtendedCommands.CSharp.Data.SimpleJSON;
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
        public const string AUTH_CODE = "RemoteBrowser#CODE#";
        public const string SERVER_IP = "192.168.0.106";
        public const int MAX_DOWNLOAD_QUEUE = 1;
        public const long MAX_FILE_SIZE = 104857600;
        public string SettingsFile { get => Settings.SettingsFile; }
        public Settings Settings { get; set; }
        public DownloadQueu Downloads { get; set; }
        public MainPage()
        {
            InitializeComponent();
            DisplayList = new List<DisplayItem>();
            //File.Delete(SettingsFile);
            path.Completed += Path_Completed;
            Settings = new Settings();
            Downloads = new DownloadQueu() { Settings = Settings, Client = Client };
            Downloads.DownloadFinished += Downloads_DownloadFinished;
            new Thread(LoadSettings).Start();
            new Thread(() => settingsImg.Source = ImageSource.FromResource("Remote_Browser.settings.png")).Start();
            new Thread(() => downloadsImg.Source = ImageSource.FromResource("Remote_Browser.downloads.ico")).Start();
#if DEBUG
            debug_connection.IsVisible = true;
#elif PERSONAL
            debug_connection.IsVisible = true;
#elif RELEASE
            debug_connection.IsVisible = false;
            ConnectToServer();
#endif
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
            var json = JSON.Parse("{\"SaveDirectory\" : \"" + Settings.SaveDirectory + "\"}");
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
                    Settings.SaveDirectory = config["SaveDirectory"].Value;
                else
                    Settings.SaveDirectory = Settings.DEFAULT_SAVE_DIRECTORY;
            }
            else
            {
                using (var writer = new StreamWriter(SettingsFile))
                    writer.Write($"{{\"SaveDirectory\" : \"{Settings.DEFAULT_SAVE_DIRECTORY}\"}}");
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
        void ConnectToServer()
        {
#if DEBUG
            var ip = hostIp.Text;
#elif PERSONAL
            var ip = hostIp.Text;
#elif RELEASE
            var ip = SERVER_IP;
#endif
            Client = new RemoteBrowserClient(ip, 4782, path.Text) { Activity = Activity };
            Downloads.Client = Client;
            if (connectionThread != null)
                try { connectionThread.Abort(); } catch { }
            connectionThread = new Thread(ConnectServer);
            connectionThread.Start();
        }
        void ConnectServer()
        {
            Client.Connect();
            Client.SendString(AUTH_CODE);
            Client.Navigated += Client_Navigated;
            Client.DirectorySet += Client_DirectorySet;
            Device.BeginInvokeOnMainThread(() => BindingContext = this);
            new Thread(new ParameterizedThreadStart(InitialList)).Start(path.Text);
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

        }
        async void RetrieveFile(object itemText)
        {
            var p = await Device.InvokeOnMainThreadAsync(() => { return Client.CurrentDirectory; });
            if (p.Length == 2)
                p += "\\";
            var pt = System.IO.Path.Combine(p, itemText.ToString()).Replace("/", "\\");
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
        public void ItemClick(object param)
        {
            var item = (DisplayItem)param;
            if (item.Text == "..")
                Client.NavigateBack();
            else if (item.typeString == "Directory")
                Client.Navigate(item.Text);
            else
                new Thread(new ParameterizedThreadStart(RetrieveFile)).Start(item.Text);
        }

        private void setHost_Clicked(object sender, System.EventArgs e)
        {
            ConnectToServer();
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
    }
    public class DisplayItem
    {
        public DisplayItem(string text, string type)
        {
            Text = text;
            this.typeString = type;
        }
        public string typeString = "";
        public string Text { get; }
        public ImageSource Type
        {
            get
            {
                switch (typeString)
                {
                    case "Directory": return ImageSource.FromResource("Remote_Browser.folder.png");
                    default: return ImageSource.FromResource("Remote_Browser.copy-icon.png");
                }
            }
        }
    }
}
