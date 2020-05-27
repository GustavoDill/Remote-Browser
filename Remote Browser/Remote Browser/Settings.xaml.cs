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
    }
}