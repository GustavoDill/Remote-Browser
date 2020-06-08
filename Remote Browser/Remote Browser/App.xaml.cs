namespace Remote_Browser
{
    public partial class App : Xamarin.Forms.Application
    {
        MainPage browser;
        public App()
        {
            InitializeComponent();
            browser = new MainPage() { };
            MainPage = browser;
        }
        public static int detail_resource_id;
        protected override void OnStart()
        {
        }
        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {
        }
    }
}
