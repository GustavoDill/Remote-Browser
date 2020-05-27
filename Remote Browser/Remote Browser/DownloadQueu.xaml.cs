using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Remote_Browser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadQueu : ContentPage
    {
        public static List<CustomProgressBar> Bars { get; set; } = new List<CustomProgressBar>();
        public List<QueueDownload> Items { get; set; }
        public RemoteBrowserClient Client { get; set; }
        public Settings Settings { get; set; }

        public DownloadQueu()
        {
            InitializeComponent();

            Items = new List<QueueDownload>();
            list.ItemsSource = Items;
        }
        Grid CreateCell(QueueDownload queue)
        {
            var grid = new Grid()
            {
                ColumnDefinitions = {
                    new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) },
                },
                RowDefinitions = {
                    new RowDefinition {Height = new GridLength(10, GridUnitType.Star) },
                }
            };
            var lbl = new Label() { Text = queue.Text };
            var pgr = new ProgressBar() { Progress = Items.Count * 25 };
            grid.Children.Add(lbl);
            grid.Children.Add(pgr);
            return grid;
        }
        public void DownloadItems()
        {

        }
        void AddQueueItem(string text)
        {
            Items.Add(text);
            Device.InvokeOnMainThreadAsync(() => { list.ItemsSource = null; list.ItemsSource = Items; });
        }
        void RemoveQueueItem(string text)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Text == text)
                { Items.Remove(Items[i]); break; }
            Device.InvokeOnMainThreadAsync(() => { list.ItemsSource = null; list.ItemsSource = Items; });
        }
        Thread t;
        async void Download(object path)
        {
            var name = new FileInfo(path.ToString().Replace("\\", "/")).Name;
            var file = Client.RetrieveFile(path.ToString());
            var r = new BinaryReader(file);
            var buffer = r.ReadBytes((int)r.BaseStream.Length);
            r.Close();
            if (buffer.Length == "ACCESS DENIED".Length)
                if (System.Text.Encoding.ASCII.GetString(buffer) == "ACCESS DENIED")
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        RemoveQueueItem(name);
                        await DisplayAlert("Retrieve File - Access denied", "Access to file '" + name + "' denied!", "OK");
                    });
                    return;
                }
            if (!Directory.Exists(Settings.SaveDirectory))
                Directory.CreateDirectory(Settings.SaveDirectory);
            var f = new BinaryWriter(File.OpenWrite(Path.Combine(Settings.SaveDirectory, name)));
            f.Write(buffer);
            f.Close();
            await Device.InvokeOnMainThreadAsync(() =>
            {
                RemoveQueueItem(name);
                Android.Widget.Toast.MakeText(MainPage.Activity, "Download '" + name + "' finished", Android.Widget.ToastLength.Long).Show();
                //await DisplayAlert("Retrieve File - Success", "File '" + name + "' retrieved successfully!", "OK");
            });
        }
        public async Task AddDownloadToQueue(string path)
        {
            if (t == null)
                t = new Thread(new ParameterizedThreadStart(Download));
            while (t.IsAlive)
            { await Task.Delay(100); }
            var name = new FileInfo(path.Replace("\\", "/")).Name;
            Items.Add(name);
            Device.BeginInvokeOnMainThread(() => { AddQueueItem(name); });
            t.Start();
        }
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            List<Grid> cells = ((ListView)sender).ItemsSource as List<Grid>;

            ((ListView)sender).SelectedItem = null;
        }
        public static /*async*/ void ProgressBarMarquee(object bar)
        {
            var progressBar = bar as ProgressBar;
            progressBar.Animate("SetProgress", (arg) => { progressBar.Progress = arg; }, 100, 1000, Easing.Linear, null, () => true);
        }
    }
    public class QueueDownload
    {
        public QueueDownload(string text)
        {
            Text = text;
        }
        public static implicit operator QueueDownload(string v)
        {
            return new QueueDownload(v);
        }
        public string Text { get; }
    }
    public class CustomProgressBar : Xamarin.Forms.ProgressBar
    {
        [Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
        public string Item { get; set; }
        public CustomProgressBar()
        {
            ProgressColor = Color.FromRgb(0, 122, 204);
            new System.Threading.Thread(new ParameterizedThreadStart(DownloadQueu.ProgressBarMarquee)).Start(this);
        }
    }
}
