using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Remote_Browser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Search : ContentPage
    {
        public MainPage ParentPage { get; set; }
        public RemoteBrowserClient Client { get => ParentPage.Client; }
        public Search()
        {
            InitializeComponent();
            Items = new List<DisplaySearchItem>();
            BindingContext = this;
        }
        List<DisplaySearchItem> Items { get; set; }
        protected override void OnAppearing()
        {
            searchTerm.Focus();
        }
        void UpdateItems() { Device.BeginInvokeOnMainThread(() => { resultView.ItemsSource = null; resultView.ItemsSource = Items; totResults.Text = Items.Count.ToString() + " Results"; }); }
        private void searchTerm_Completed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(searchTerm.Text))
            {
                Items = Client.SearchFile(searchTerm.Text).ToList();
                UpdateItems();
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => DisplayAlert("Search", "Please put something to search!", "OK"));
            }
        }

        private void resultView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                bool flag = false;
                var item = new DisplayItemDetails(Client, ((DisplaySearchItem)e.Item).FullPath);
                var fs = Client.GetFileSize(((DisplaySearchItem)e.Item).FullPath);
                resultView.SelectedItem = null;
                item.RequestDetails();
                if (item.Type == "File")
                    flag = true;
                long size = 0;
                string s = size + " Bytes";
                if (fs != "ERROR_FILE_INEXISTENT")
                {
                    size = Convert.ToInt64(fs, 16);
                    s = size + " Bytes";
                    if (size / 1024 > 1)
                    {
                        s = (size / 1024) + " KB";
                        if (size / 1024 / 1024 > 1)
                        {
                            s = (size / 1024 / 1024) + " MB";
                            if (size / 1024 / 1024 / 1024 > 1)
                                s = (size / 1024 / 1024 / 1024) + " GB";
                        }
                    }
                }
                Navigation.PushModalAsync(new DetailDialog() { DownloadButtonEnabled = flag, ParentPage = ParentPage, Name = item.Name, Type = item.Type, Size = s, Directory = item.ParentDirectory, FullPath = item.FullPath });
            }
        }
        public class DisplaySearchItem : DisplayItem
        {
            public DisplaySearchItem(string text, string type, string fullPath)
            {
                Text = text;
                typeString = type;
                FullPath = fullPath;
            }

            public string FullPath { get; }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {

        }

        private void return_Tapped(object sender, EventArgs e)
        {
        }

        private void returnBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync(true);
        }
    }
}