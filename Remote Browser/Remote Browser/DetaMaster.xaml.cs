using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Remote_Browser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetaMaster : ContentPage
    {
        public ListView ListView;

        public DetaMaster()
        {
            InitializeComponent();

            BindingContext = new DetaMasterViewModel();
            ListView = MenuItemsListView;
        }

        class DetaMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<DetaMasterMenuItem> MenuItems { get; set; }

            public DetaMasterViewModel()
            {
                MenuItems = new ObservableCollection<DetaMasterMenuItem>(new[]
                {
                    new DetaMasterMenuItem { Id = 0, Title = "Page 1" },
                    new DetaMasterMenuItem { Id = 1, Title = "Page 2" },
                    new DetaMasterMenuItem { Id = 2, Title = "Page 3" },
                    new DetaMasterMenuItem { Id = 3, Title = "Page 4" },
                    new DetaMasterMenuItem { Id = 4, Title = "Page 5" },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}