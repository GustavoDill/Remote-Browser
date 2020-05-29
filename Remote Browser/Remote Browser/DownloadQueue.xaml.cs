using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            downloadThread = new Thread(DownloadQueue);
            downloadThread.Start();
        }
        Thread downloadThread;
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
        async Task AddQueueItem(QueueDownload item)
        {
            Items.Add(item);
            await Device.InvokeOnMainThreadAsync(() => { list.ItemsSource = null; list.ItemsSource = Items; });
        }
        async void RemoveQueueItem(int index)
        {
            Items.RemoveAt(index);
            await Device.InvokeOnMainThreadAsync(() => { list.ItemsSource = null; list.ItemsSource = Items; });
        }
        async void RemoveQueueItem(string text)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Text == text)
                { Items.Remove(Items[i]); break; }
            await Device.InvokeOnMainThreadAsync(() => { list.ItemsSource = null; list.ItemsSource = Items; });
        }
        async Task<bool> Download(object path)
        {
            var item = (QueueDownload)path;
            //var name = new FileInfo(path.ToString().Replace("\\", "/")).Name;
            var name = item.Text;
            var file = Client.RetrieveFile(item.Path);
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
                    return false;
                }
            if (!Directory.Exists(Settings.SaveDirectory))
                Directory.CreateDirectory(Settings.SaveDirectory);
            var f = new BinaryWriter(File.OpenWrite(Path.Combine(Settings.SaveDirectory, name)));
            f.Write(buffer);
            f.Close();
            return true;
        }
        public class DownloadFinishArgs : System.EventArgs
        {
            public DownloadFinishArgs(string name)
            {
                Name = name;
            }
            public override string ToString()
            {
                return "Download of '" + Name + "' finished";
            }
            public string Name { get; }
        }

        public event System.EventHandler<DownloadFinishArgs> DownloadFinished;
        bool needToShowFinishedDialog;
        public async void DownloadQueue()
        {
            while (true)
            {
                if (Items.Count == 0)
                    if (needToShowFinishedDialog)
                    {
                        DownloadFinished?.Invoke(this, new DownloadFinishArgs(""));
                        //MainPage.Activity.RunOnUiThread(() => DisplayAlert("Download Finished", "Download of the file has finished", "OK"));
                        needToShowFinishedDialog = false;
                    }
                    else
                        Thread.Sleep(20);
                else
                {
                    needToShowFinishedDialog = true;
                    var r = await Download(Items[0]);
                    if (!r)
                        DownloadFinished?.Invoke(this, new DownloadFinishArgs(Items[0].Text));
                    RemoveQueueItem(0);
                }
            }
        }
        public async Task AddDownloadToQueue(string path)
        {
            var name = new FileInfo(path.Replace("\\", "/")).Name;
            var item = new QueueDownload(name, path);
            await AddQueueItem(item);
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
        public QueueDownload(string text, string path)
        {
            Text = text;
            Path = path;
        }
        public string Text { get; }
        public string Path { get; }
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
