using System;
using System.IO;
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
        public const string DEFAULT_CONNECTION_IP = "187.45.106.187";
        public Settings()
        {
            InitializeComponent();
        }
        private void OnSave(object sender, EventArgs e)
        {
            Saved?.Invoke(this, e);
        }
        public event EventHandler Saved;
        public string SaveDirectory { get => savedir.Text; set => savedir.Text = value; }
        public string HostIp { get => hostIp.Text; set => hostIp.Text = value; }
        public ushort HostPort
        {
            get
            {
                if (ushort.TryParse(hostPort.Text, out ushort p))
                    return p;
                else
                {
                    HostPort = DEFAULT_CONNECTION_PORT;
                    return DEFAULT_CONNECTION_PORT;
                }
            }
            set => hostPort.Text = value.ToString();
        }
        public void SetPort(string p)
        {
            if (ushort.TryParse(hostIp.Text, out ushort v))
                HostPort = v;
        }
    }
}