
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidExtendedCommands.Permissions;
namespace Remote_Browser.Droid
{
    [Activity(Label = "Remote Browser", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            PermissionHandler.Activity = this;
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            MainPage.Activity = this;
            //App.detail_resource_id = Resource.Layout.detail_dialog;
            LoadApplication(new App());
            ph = new PermissionHandler(Manifest.Permission.Internet, Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage);
            ph.RequestAllPermissions();
        }
        public PermissionHandler ph;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            ph.RequestPermissionResult(requestCode, permissions, grantResults);
        }
    }
}