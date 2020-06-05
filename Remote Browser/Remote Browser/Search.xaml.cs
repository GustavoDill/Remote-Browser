using System;
using System.Collections.Generic;
using System.IO;
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
            Items = new List<DisplayItem>();
            BindingContext = this;
        }
        List<DisplayItem> Items { get; set; }
        void UpdateItems() { Device.BeginInvokeOnMainThread(() => { resultView.ItemsSource = null; resultView.ItemsSource = Items; }); }
        private void searchTerm_Completed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(searchTerm.Text))
            {
                string[] fileResults = Client.SearchFile(searchTerm.Text);
                Items.Clear();
                foreach (var f in fileResults)
                    Items.Add(new DisplayItem(new FileInfo(f.Replace("\\", "/")).Name, "File"));
                UpdateItems();
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => DisplayAlert("Search", "Please put something to search!", "OK"));
            }
        }
    }
}