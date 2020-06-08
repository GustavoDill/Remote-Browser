using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Remote_Browser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : ContentPage
    {
        public static string SettingsFile { get => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "settings.conf"); }
        public const string DEFAULT_SAVE_DIRECTORY = "/storage/emulated/0/RemoteBrowser";
        public const ushort DEFAULT_CONNECTION_PORT = 4782;
        public MainPage ParentPage { get; set; }
        public List<ConnectionServer> AvaliableServers { get; set; }

        public Settings()
        {
            InitializeComponent();
            //AvaliableServers = new List<ConnectionServer>()
            //{
            //    new ConnectionServer("SOS-PC", "ubtwebserver.ddns.net", IPAddress.Parse("192.168.0.106"), 4782),
            //    new ConnectionServer("SOS-PC", "ubtwebserver.ddns.net", IPAddress.Parse("192.168.0.106"), 4782),
            //    new ConnectionServer("SOS-PC", "ubtwebserver.ddns.net", IPAddress.Parse("192.168.0.106"), 4782)
            //};
            BindingContext = this;
        }
        public void UpdateList()
        {
            Device.BeginInvokeOnMainThread(() => { servers.ItemsSource = null; servers.ItemsSource = AvaliableServers; removeBtn.IsEnabled = AvaliableServers.Count > 0; ParentPage.ConnectButtonEnabled = SelectedHost != null; });
        }
        private void OnSave(object sender, EventArgs e)
        {
            Saved?.Invoke(this, e);
        }
        public event EventHandler Saved;
        public ConnectionServer SelectedHost { get; private set; }
        public string SaveDirectory { get => savedir.Text; set => savedir.Text = value; }
        void LoadServer(ConnectionServer server)
        {
            serverName.Text = server.ServerName;
            publicHost.Text = server.PublicHost.ToString();
            lanHost.Text = server.LanIp.ToString();
            hostPort.Text = server.Port.ToString();
        }
        private void servers_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LoadServer(AvaliableServers[e.ItemIndex]);
            try { SelectedHost = AvaliableServers[e.ItemIndex]; } catch { SelectedHost = null; }
            ParentPage.ConnectButtonEnabled = SelectedHost != null;
            editBtn.IsEnabled = SelectedHost != null;
            removeBtn.IsEnabled = SelectedHost != null;
        }

        private void addBtn_Clicked(object sender, EventArgs e)
        {
            if (AvaliableServers == null)
                AvaliableServers = new List<ConnectionServer>();
            AvaliableServers.Add(new ConnectionServer(serverName.Text, publicHost.Text, IPAddress.Parse(lanHost.Text), ushort.Parse(hostPort.Text)));
            new System.Threading.Thread(ClearStuff).Start();
            UpdateList();
        }
        void ClearStuff()
        {
            Device.BeginInvokeOnMainThread(() => { hostPort.Text = ""; publicHost.Text = ""; lanHost.Text = ""; serverName.Text = ""; });
        }
        private void removeBtn_Clicked(object sender, EventArgs e)
        {
            AvaliableServers.Remove(SelectedHost);
            UpdateList();
            SelectedHost = null;
            removeBtn.IsEnabled = false;
        }
        void UpdateLittleButton()
        {
            addBtn.IsEnabled = !string.IsNullOrEmpty(serverName.Text) && !string.IsNullOrEmpty(publicHost.Text) && !string.IsNullOrEmpty(lanHost.Text) && !string.IsNullOrEmpty(hostPort.Text);
            editBtn.IsEnabled = SelectedHost != null && !string.IsNullOrEmpty(serverName.Text) && !string.IsNullOrEmpty(publicHost.Text) && !string.IsNullOrEmpty(lanHost.Text) && !string.IsNullOrEmpty(hostPort.Text);
        }
        private void serverName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLittleButton();
        }

        private void publicHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLittleButton();
        }

        private void lanHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLittleButton();
        }

        private void hostPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLittleButton();
        }

        private void modBtn_Clicked(object sender, EventArgs e)
        {
            for (int i = 0; i < AvaliableServers.Count; i++)
                if (AvaliableServers[i] == SelectedHost)
                    AvaliableServers[i] = new ConnectionServer(serverName.Text, publicHost.Text, IPAddress.Parse(lanHost.Text), ushort.Parse(hostPort.Text));
            UpdateList();
        }
    }
}