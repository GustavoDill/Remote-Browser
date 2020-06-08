
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Remote_Browser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailDialog : ContentPage
    {
        public MainPage ParentPage { get; set; }
        public DetailDialog()
        {
            InitializeComponent();
        }
        public string Type { get => type.Text; set => Device.BeginInvokeOnMainThread(() => type.Text = $"Type: {value}"); }
        public string Size { get => sz.Text; set => Device.BeginInvokeOnMainThread(() => sz.Text = $"Size: {value}"); }
        public string Name { get => itemName.Text; set => Device.BeginInvokeOnMainThread(() => itemName.Text = $"Name: {value}"); }
        public string FullPath { get => itemFullName.Text; set => Device.BeginInvokeOnMainThread(() => itemFullName.Text = $"FullName: {value}"); }
        public string Directory { get => itemDirectory.Text; set => Device.BeginInvokeOnMainThread(() => itemDirectory.Text = $"ParentDir: {value}"); }
        public bool DownloadButtonEnabled { get => downloadBtn.IsEnabled; set => Device.BeginInvokeOnMainThread(() => downloadBtn.IsEnabled = value); }
        private void Button_Clicked(object sender, System.EventArgs e)
        {
            ParentPage.RetrieveFile(FullPath.Substring("FullPath: ".Length));
        }
    }
}